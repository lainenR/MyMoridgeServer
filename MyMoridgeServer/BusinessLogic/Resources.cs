using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Resources
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();
        private struct TimeSchedule
        {
            public const int MorningStartHour = 8;
            public const int MorningEndHour = 12;
            public const int AfterLunchStartHour = 13;
            public const int AfterLunchEndHour = 16;
        }

        public Resources()
        {
          
        }

        public int GetResourceIdAvailableForBooking(BookingEvent bookingEvent)
        {
            int resourceId = -1;

            try
            {
                foreach (Resource resource in db.Resources.OrderBy(r => r.BookingPriority))
                {
                    GoogleCalendar googleCalendar = new GoogleCalendar(resource.CalendarEmail, resource.CalendarServiceAccountEmail);
                    var events = GoogleCalendarHelper.HandleEventStartEndNull(googleCalendar.GetEventList(resource.CalendarEmail));
                    var afterLunchStartHourUTC = TimeSchedule.AfterLunchStartHour - Common.GetSwedishDateTimeOffsetFromUTC(bookingEvent.StartDateTime);

                    int maxBookings = 0;

                    if(afterLunchStartHourUTC != bookingEvent.StartDateTime.Hour)
                    {
                        maxBookings = resource.MaxBookingsBeforeLunch;
                    }
                    else
                    {
                        maxBookings = resource.MaxBookingsAfterLunch;
                    }

                    
                    bool available = IsSlotAvailable(events.Items, bookingEvent.StartDateTime, bookingEvent.EndDateTime, maxBookings, resource.DaysBeforeBooking);
                    
                    if (available)
                    {
                        resourceId = resource.Id;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to check if slot is available", ex);
            }

            return resourceId;
        }

        public List<BookingEvent> Get15AvailableDatesForBooking()
        {
            List<BookingEvent> main = new List<BookingEvent>();

            try
            {
                foreach (Resource resource in db.Resources.OrderBy(r => r.BookingPriority))
                {                    
                    List<BookingEvent> list = Get15AvailableDatesForBooking(resource);
                    var newItems = list.Where(x => !main.Any(y => x.StartDateTime == y.StartDateTime));
                    foreach (var item in newItems)
                    {
                        main.Add(item);
                    }
                }

                if (main.Count() == 0)
                {
                    throw new Exception("Resources missing in database, please add at least one resource!");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }

            return main.OrderBy(x => x.StartDateTime).Take(15).ToList();
        }

        private List<BookingEvent> Get15AvailableDatesForBooking(Resource resource)
        {
            GoogleCalendar googleCalendar = new GoogleCalendar(resource.CalendarEmail, resource.CalendarServiceAccountEmail);
            var events = GoogleCalendarHelper.HandleEventStartEndNull(googleCalendar.GetEventList(resource.CalendarEmail));

            //Debugging purpose
            //googleCalendar.DeleteEvent(Common.GetAppConfigValue(resource.CalendarEmail), "7dfntteacvfore0o5qdivp2g28");

            var daysBeforeBooking = resource.DaysBeforeBooking;

            DateTime currentStartDate = DateTime.Now.AddDays(daysBeforeBooking).ToUniversalTime();
            DateTime currentEndDate = currentStartDate;

            TimeScheduler timeScheduler = new TimeScheduler(currentStartDate);
            TimeSpan morningStartTime = timeScheduler.GetMorningStartTimeSwedishTimeCompensateUTC();
            TimeSpan morningEndTime = timeScheduler.GetMorningEndTimeSwedishTimeCompensateUTC();
            TimeSpan afterLunchStartTime = timeScheduler.GetAfterLunchStartTimeSwedishTimeCompensateUTC();
            TimeSpan afterLunchEndTime = timeScheduler.GetAfterLunchEndTimeSwedishTimeCompensateUTC();

            List<BookingEvent> dateList = new List<BookingEvent>();

            bool isMorningTime = true;
            int maxBookings = 0;
            int maxBookingsBeforeLunch = resource.MaxBookingsBeforeLunch;
            int maxBookingsAfterLunch = resource.MaxBookingsAfterLunch;

            while (dateList.Count < 15)
            {
                if (currentStartDate.DayOfWeek != DayOfWeek.Saturday && currentStartDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (isMorningTime)
                    {
                        currentStartDate = currentStartDate.Date + morningStartTime;
                        currentEndDate = currentEndDate.Date + morningEndTime;
                        maxBookings = maxBookingsBeforeLunch;
                        isMorningTime = false;
                    }
                    else
                    {
                        currentStartDate = currentStartDate.Date + afterLunchStartTime;
                        currentEndDate = currentEndDate.Date + afterLunchEndTime;
                        maxBookings = maxBookingsAfterLunch;
                        isMorningTime = true;
                    }

                    if(IsSlotAvailable(events.Items, currentStartDate, currentEndDate, maxBookings, daysBeforeBooking) &&
                        IsResourceWorking(events.Items, currentStartDate, currentEndDate))
                    {
                        BookingEvent bookingEvent = new BookingEvent();

                        bookingEvent.IsBooked = false;
                        bookingEvent.StartDateTime = currentStartDate;
                        bookingEvent.EndDateTime = currentEndDate;
                        bookingEvent.ResourceId = resource.Id;

                        dateList.Add(bookingEvent);
                    }
                }

                if (isMorningTime)
                {
                    currentStartDate = currentStartDate.AddDays(1);
                    currentEndDate = currentEndDate.AddDays(1);
                }
            }

            return dateList;
        }

        private bool IsSlotAvailable(IList<Google.Apis.Calendar.v3.Data.Event> events, DateTime startDate, DateTime endDate, int maxBookings, int daysBeforeBooking)
        {
            var isSlotAvailable = false;

            if (endDate.ToUniversalTime() > DateTime.UtcNow.AddDays(daysBeforeBooking))
            {
                isSlotAvailable = events.Count(booked =>
                            (
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() >= startDate.ToUniversalTime() &&
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() < endDate.ToUniversalTime()
                            )
                            ||
                            (
                                ((DateTime)booked.End.DateTime).ToUniversalTime() > startDate.ToUniversalTime() &&
                                ((DateTime)booked.End.DateTime).ToUniversalTime() <= endDate.ToUniversalTime()
                            )
                            ||
                            (
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() < startDate.ToUniversalTime() &&
                                ((DateTime)booked.End.DateTime).ToUniversalTime() > endDate.ToUniversalTime()
                            )
                            ||
                            (
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() > startDate.ToUniversalTime() &&
                                ((DateTime)booked.End.DateTime).ToUniversalTime() < endDate.ToUniversalTime()
                            )
                        ) < maxBookings;
            }

            return isSlotAvailable;
        }
        
        private bool IsResourceWorking(IList<Google.Apis.Calendar.v3.Data.Event> events, DateTime startDate, DateTime endDate)
        {
            return (events.Count(free =>
                        (
                            (
                                ((DateTime)free.Start.DateTime).ToUniversalTime() >= startDate.ToUniversalTime() &&
                                ((DateTime)free.Start.DateTime).ToUniversalTime() < endDate.ToUniversalTime()
                             )
                             ||
                             (
                                ((DateTime)free.End.DateTime).ToUniversalTime() > startDate.ToUniversalTime() &&
                                ((DateTime)free.End.DateTime).ToUniversalTime() <= endDate.ToUniversalTime()
                             )
                             ||
                             (
                                ((DateTime)free.Start.DateTime).ToUniversalTime() < startDate.ToUniversalTime() &&
                                ((DateTime)free.End.DateTime).ToUniversalTime() > endDate.ToUniversalTime()
                             )
                             ||
                             (
                                ((DateTime)free.Start.DateTime).ToUniversalTime() > startDate.ToUniversalTime() &&
                                ((DateTime)free.End.DateTime).ToUniversalTime() < endDate.ToUniversalTime()
                             )
                        )
                        &&
                        free.Summary.ToLower().StartsWith("ledig")) == 0);
        }
    }
}
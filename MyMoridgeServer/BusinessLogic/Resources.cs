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
                    var events = GoogleCalendarHelper.HandleEventStartEndNull(googleCalendar.GetEventList());
                    var afterLunchStartHourUTC = TimeSchedule.AfterLunchStartHour - Common.GetSwedishDateTimeOffsetFromUTC(bookingEvent.StartDateTime);

                    BookingSlot bookingSlot = new BookingSlot(
                        bookingEvent.StartDateTime, 
                        bookingEvent.EndDateTime,
                        afterLunchStartHourUTC != bookingEvent.StartDateTime.Hour ? resource.MaxBookingsBeforeLunch : resource.MaxBookingsAfterLunch,
                        afterLunchStartHourUTC != bookingEvent.StartDateTime.Hour ? true : false);
                    
                    bool available = IsSlotAvailable(events.Items, bookingSlot, resource.DaysBeforeBooking);
                    
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

        public List<BookingEvent> GetAvailableDatesForBooking(Product product, int maxCount = 15)
        {
            List<BookingEvent> main = new List<BookingEvent>();

            try
            {
                foreach (Resource resource in db.Resources.OrderBy(r => r.BookingPriority))
                {
                    List<BookingEvent> list = GetAvailableDatesForBooking(resource, product, maxCount);
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

        private List<BookingEvent> GetAvailableDatesForBooking(Resource resource, Product product, int maxCount)
        {
            GoogleCalendar googleCalendar = new GoogleCalendar(resource.CalendarEmail, resource.CalendarServiceAccountEmail);
            var events = GoogleCalendarHelper.HandleEventStartEndNull(googleCalendar.GetEventList());
            var daysBeforeBooking = GetDaysBeforeBooking(resource, product);

            DateTime currentStartDate = DateTime.Now.AddDays(daysBeforeBooking).ToUniversalTime();
            DateTime currentEndDate = currentStartDate;

            TimeScheduler timeScheduler = new TimeScheduler(currentStartDate);
            List<BookingEvent> dateList = new List<BookingEvent>();

            bool isMorningTime = true;
      
            while (dateList.Count < maxCount)
            {
                if (currentStartDate.DayOfWeek != DayOfWeek.Saturday && currentStartDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (product.HoursToBook > 4)
                    {
                        BookingSlot morningSlot = new BookingSlot(
                                currentStartDate.Date + timeScheduler.GetMorningStartTimeSwedishTimeCompensateUTC(),
                                currentEndDate.Date + timeScheduler.GetMorningEndTimeSwedishTimeCompensateUTC(),
                                resource.MaxBookingsBeforeLunch,
                                true);

                        BookingSlot afterLunchSlot = new BookingSlot(
                                currentStartDate.Date + timeScheduler.GetAfterLunchStartTimeSwedishTimeCompensateUTC(),
                                currentEndDate.Date + timeScheduler.GetAfterLunchEndTimeSwedishTimeCompensateUTC(),
                                resource.MaxBookingsAfterLunch,
                                false);

                        if (IsSlotAvailable(events.Items, morningSlot, daysBeforeBooking) &&
                            IsResourceWorking(events.Items, morningSlot) &&
                            IsSlotAvailable(events.Items, afterLunchSlot, daysBeforeBooking) &&
                            IsResourceWorking(events.Items, afterLunchSlot))
                        {
                            BookingEvent bookingEvent = new BookingEvent();

                            bookingEvent.IsBooked = false;
                            bookingEvent.StartDateTime = morningSlot.StartDateTime;
                            bookingEvent.EndDateTime = afterLunchSlot.EndDateTime;
                            bookingEvent.ResourceId = resource.Id;

                            dateList.Add(bookingEvent);
                        }   
                    }
                    else
                    {
                        BookingSlot bookingSlot;

                        if (isMorningTime)
                        {
                            bookingSlot = new BookingSlot(
                                currentStartDate.Date + timeScheduler.GetMorningStartTimeSwedishTimeCompensateUTC(),
                                currentEndDate.Date + timeScheduler.GetMorningEndTimeSwedishTimeCompensateUTC(),
                                resource.MaxBookingsBeforeLunch,
                                true);

                            isMorningTime = false;
                        }
                        else
                        {
                            bookingSlot = new BookingSlot(
                                currentStartDate.Date + timeScheduler.GetAfterLunchStartTimeSwedishTimeCompensateUTC(),
                                currentEndDate.Date + timeScheduler.GetAfterLunchEndTimeSwedishTimeCompensateUTC(),
                                resource.MaxBookingsAfterLunch,
                                false);

                            isMorningTime = true;
                        }

                        if (IsSlotAvailable(events.Items, bookingSlot, daysBeforeBooking) &&
                            IsResourceWorking(events.Items, bookingSlot))
                        {
                            BookingEvent bookingEvent = new BookingEvent();

                            bookingEvent.IsBooked = false;
                            bookingEvent.StartDateTime = bookingSlot.StartDateTime;
                            bookingEvent.EndDateTime = bookingSlot.EndDateTime;
                            bookingEvent.ResourceId = resource.Id;

                            dateList.Add(bookingEvent);
                        }
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

        private bool IsSlotAvailable(IList<Google.Apis.Calendar.v3.Data.Event> events, BookingSlot bookingSlot, int daysBeforeBooking)
        {
            var isSlotAvailable = false;

            if (bookingSlot.EndDateTime.ToUniversalTime() > DateTime.UtcNow.AddDays(daysBeforeBooking))
            {
                isSlotAvailable = events.Count(booked =>
                            (
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() >= bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() < bookingSlot.EndDateTime.ToUniversalTime()
                            )
                            ||
                            (
                                ((DateTime)booked.End.DateTime).ToUniversalTime() > bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)booked.End.DateTime).ToUniversalTime() <= bookingSlot.EndDateTime.ToUniversalTime()
                            )
                            ||
                            (
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() < bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)booked.End.DateTime).ToUniversalTime() > bookingSlot.EndDateTime.ToUniversalTime()
                            )
                            ||
                            (
                                ((DateTime)booked.Start.DateTime).ToUniversalTime() > bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)booked.End.DateTime).ToUniversalTime() < bookingSlot.EndDateTime.ToUniversalTime()
                            )
                        ) < bookingSlot.MaxBookings;
            }

            return isSlotAvailable;
        }
        
        private bool IsResourceWorking(IList<Google.Apis.Calendar.v3.Data.Event> events, BookingSlot bookingSlot)
        {
            return (events.Count(free =>
                        (
                            (
                                ((DateTime)free.Start.DateTime).ToUniversalTime() >= bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)free.Start.DateTime).ToUniversalTime() < bookingSlot.EndDateTime.ToUniversalTime()
                             )
                             ||
                             (
                                ((DateTime)free.End.DateTime).ToUniversalTime() > bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)free.End.DateTime).ToUniversalTime() <= bookingSlot.EndDateTime.ToUniversalTime()
                             )
                             ||
                             (
                                ((DateTime)free.Start.DateTime).ToUniversalTime() < bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)free.End.DateTime).ToUniversalTime() > bookingSlot.EndDateTime.ToUniversalTime()
                             )
                             ||
                             (
                                ((DateTime)free.Start.DateTime).ToUniversalTime() > bookingSlot.StartDateTime.ToUniversalTime() &&
                                ((DateTime)free.End.DateTime).ToUniversalTime() < bookingSlot.EndDateTime.ToUniversalTime()
                             )
                        )
                        &&
                        free.Summary.ToLower().StartsWith("ledig")) == 0);
        }

        private int GetDaysBeforeBooking(Resource currentResource, Product currentProduct)
        {
            return currentResource.DaysBeforeBooking > currentProduct.DaysBeforeBooking ? currentResource.DaysBeforeBooking : currentProduct.DaysBeforeBooking;
        }
    }
}
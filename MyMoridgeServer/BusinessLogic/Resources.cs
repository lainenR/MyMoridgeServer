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
            var events = googleCalendar.GetEventList();

            //Debugging purpose
            //googleCalendar.DeleteEvent(Common.GetAppConfigValue(resource.CalendarEmail), "7dfntteacvfore0o5qdivp2g28");

            var daysBeforeBooking = resource.DaysBeforeBooking;

            DateTime currentStartDate = DateTime.Now.AddDays(daysBeforeBooking).ToUniversalTime().AddHours(1); //Set swedish time
            DateTime currentEndDate = currentStartDate;

            TimeSpan morningStartTime = new TimeSpan(8, 0, 0);
            TimeSpan morningEndTime = new TimeSpan(12, 0, 0);
            TimeSpan afterLunchStartTime = new TimeSpan(13, 0, 0);
            TimeSpan afterLunchEndTime = new TimeSpan(17, 0, 0);

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

                    //Check if not all slots are booked
                    if (events.Items.Count(date => (DateTime)date.End.DateTime > currentStartDate &&
                                            (DateTime)date.End.DateTime <= currentEndDate) < maxBookings)
                    {
                        //Check if resource is working
                        if (events.Items.Count(free => (DateTime)free.End.DateTime > currentStartDate &&
                        (DateTime)free.End.DateTime <= currentEndDate &&
                        free.Summary.ToLower().StartsWith("ledig")) == 0)
                        {
                            BookingEvent bookingEvent = new BookingEvent();

                            bookingEvent.IsBooked = false;
                            bookingEvent.StartDateTime = currentStartDate;
                            bookingEvent.EndDateTime = currentEndDate;
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
    }
}
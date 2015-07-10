using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Calendar.v3.Data;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Booking
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();

        public Booking()
        {
            
        }

        public void BookEvent(BookingEvent bookingEvent)
        {
            if(bookingEvent.StartDateTime.CompareTo(DateTime.UtcNow.AddYears(-1)) < 0 ||
                        bookingEvent.EndDateTime.CompareTo(DateTime.UtcNow.AddYears(-1)) < 0)
            {
                throw new Exception("Date error: " + bookingEvent.StartDateTime.ToString() + 
                    " : " + bookingEvent.EndDateTime.ToString());
            }

            if(db.CustomerVehicles.Count(v => 
                v.CustomerOrgNo == bookingEvent.CustomerOrgNo &&
                v.VehicleRegNo == bookingEvent.VehicleRegNo) == 0)
            {
                CustomerVehicle customerVehicle = new CustomerVehicle();
                customerVehicle.CustomerOrgNo = bookingEvent.CustomerOrgNo;
                customerVehicle.VehicleRegNo = bookingEvent.VehicleRegNo;
                db.CustomerVehicles.Add(customerVehicle);
                db.SaveChanges();
            }

            var ev = GetGoogleEvent(bookingEvent);
            ev.Attendees = new List<EventAttendee>();
            ev.Attendees.Add(GetUserAttende(bookingEvent.CustomerEmail));
            ev.Attendees.Add(GetMoridgeDriverCalendarAttende());
            ev.Organizer = GetEventOrganizer();

            ev.Start = GetEventStart(bookingEvent.StartDateTime.AddHours(8));
            ev.End = GetEventEnd(bookingEvent.EndDateTime.AddHours(8));
            ev.Location = bookingEvent.CustomerAddress;
            ev.Description = bookingEvent.BookingMessage;

            GoogleCalendar calendar = new GoogleCalendar();
            calendar.InsertEvent(ev, Resource.GetAppConfigValue("MoridgeDriverCalendarEmail"));

            db.BookingLogs.Add(GetBookingLog(bookingEvent));
            db.SaveChanges();
        }

        public List<BookingEvent> Get15AvailableDatesForBooking()
        {
            GoogleCalendar googleCalendar = new GoogleCalendar();
            var events = googleCalendar.GetEventList(Resource.GetAppConfigValue("MoridgeDriverCalendarEmail"));

            //googleCalendar.DeleteEvent(Resource.GetAppConfigValue("MoridgeDriverCalendarEmail"), "7dfntteacvfore0o5qdivp2g28");

            var daysBeforeBooking = 1;
            try
            {
                daysBeforeBooking = Convert.ToInt32(Resource.GetAppConfigValue("DaysBeforeBooking"));
            }
            catch(Exception ex)
            {
                Resource.LogError(ex);
            }

            DateTime currentStartDate = DateTime.Now.AddDays(daysBeforeBooking).ToUniversalTime().AddHours(1); //Set swedish time
            DateTime currentEndDate = currentStartDate;
            
            TimeSpan morningStartTime = new TimeSpan(8, 0, 0);
            TimeSpan morningEndTime = new TimeSpan(12, 0, 0);
            TimeSpan afterLunchStartTime = new TimeSpan(13, 0, 0);
            TimeSpan afterLunchEndTime = new TimeSpan(17, 0, 0);

            List<BookingEvent> dateList = new List<BookingEvent>();

            bool isMorningTime = true;
            int maxBookings = 0;
            int maxBookingsBeforeLunch = Convert.ToInt32(Resource.GetAppConfigValue("MaxBookingsBeforeLunch"));
            int maxBookingsAfterLunch = Convert.ToInt32(Resource.GetAppConfigValue("MaxBookingsAfterLunch"));

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

                    if (events.Items.Count(date => (DateTime)date.Start.DateTime >= currentStartDate &&
                                            (DateTime)date.End.DateTime <= currentEndDate) < maxBookings)
                    {
                        BookingEvent bookingEvent = new BookingEvent();

                        bookingEvent.IsBooked = false;
                        bookingEvent.StartDateTime = currentStartDate;
                        bookingEvent.EndDateTime = currentEndDate;

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

        public List<BookingEvent> GetBookingsByOrganisation(string customerOrgNo)
        {
            var log = db.BookingLogs.Where(bl => bl.CustomerOrgNo == customerOrgNo);
            List<BookingEvent> events = new List<BookingEvent>();
            DateTime now = DateTime.Now.AddHours(1).ToUniversalTime();

            foreach (var item in log.Where(l => l.EndDateTime >= now))
            {
                BookingEvent ev = new BookingEvent();

                ev.StartDateTime = item.StartDateTime;
                ev.EndDateTime = item.EndDateTime;
                ev.VehicleRegNo = item.VehicleRegNo;
                ev.CustomerAddress = item.CustomerAddress;
                ev.CustomerEmail = item.CustomerEmail;
                ev.CustomerOrgNo = item.CustomerOrgNo;
                ev.IsBooked = true;
                ev.BookingMessage = item.BookingMessage;
                ev.CompanyName = item.CompanyName;

                events.Add(ev);
            }

            return events;
        }


        private Event GetGoogleEvent(BookingEvent ev)
        {
            var googleEvent = new Event();
            StringBuilder sb = new StringBuilder();
            
            sb.Append("Moridge - ").Append(ev.CompanyName).Append(" ").Append(ev.VehicleRegNo);
            googleEvent.Summary = sb.ToString();
            googleEvent.Description = ev.BookingMessage;
            
            return googleEvent;
        }

        private EventAttendee GetUserAttende(string email)
        {
            
            var attende = new EventAttendee();
            attende.Email = email;

            return attende;
        }

        private EventAttendee GetMainMoridgeCalendarAttende()
        {
            var attende = new EventAttendee();

            attende.Email = Resource.GetAppConfigValue("MoridgeMainCalendarEmail");

            return attende;
        }

        private EventAttendee GetMoridgeDriverCalendarAttende()
        {
            var attende = new EventAttendee();

            attende.Email = Resource.GetAppConfigValue("MoridgeDriverCalendarEmail");

            return attende;
        }

        private EventDateTime GetEventStart(DateTime start)
        {
            var eventDateTime = new EventDateTime();

            eventDateTime.DateTimeRaw = start.ToString("yyyy-MM-dd") + "T" + start.TimeOfDay.ToString() + "+01:00";

            return eventDateTime;
        }

        private EventDateTime GetEventEnd(DateTime end)
        {
            var eventDateTime = new EventDateTime();

            eventDateTime.DateTimeRaw = end.Date.ToString("yyyy-MM-dd") + "T" + end.TimeOfDay.ToString() + "+01:00";

            return eventDateTime;
        }

        private Event.OrganizerData GetEventOrganizer()
        {
            Event.OrganizerData data = new Event.OrganizerData();

            data.DisplayName = "Moridge";
            data.DisplayName = Resource.GetAppConfigValue("MoridgeDriverCalendarEmail");

            return data;
        }

        private BookingLog GetBookingLog(BookingEvent ev)
        {
            BookingLog log = new BookingLog();

            log.StartDateTime = ev.StartDateTime;
            log.EndDateTime = ev.EndDateTime;
            log.VehicleRegNo = ev.VehicleRegNo;
            log.CustomerAddress = ev.CustomerAddress;
            log.CustomerEmail = ev.CustomerEmail;
            log.CustomerOrgNo = ev.CustomerOrgNo;
            log.BookingMessage = ev.BookingMessage;
            log.CompanyName = ev.CompanyName;

            return log;
        }
    }
}
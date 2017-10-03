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

        public bool IsBookingDateAvailable(BookingEvent bookingEvent)
        {
            Resources resources = new Resources();

            bookingEvent.ResourceId = resources.GetResourceIdAvailableForBooking(bookingEvent);

            return bookingEvent.ResourceId > -1; 
        }

        public int BookEvent(BookingEvent bookingEvent)
        {
            Resource resource = db.Resources.Find(bookingEvent.ResourceId);

            if (resource == null)
            {
                throw new Exception("Error trying to find resource with id: " + bookingEvent.ResourceId.ToString());
            }

            if(bookingEvent.StartDateTime.CompareTo(DateTime.UtcNow.AddYears(-1)) < 0 ||
                        bookingEvent.EndDateTime.CompareTo(DateTime.UtcNow.AddYears(-1)) < 0)
            {
                throw new Exception("Date error: " + bookingEvent.StartDateTime.ToString() + 
                    " : " + bookingEvent.EndDateTime.ToString());
            }

            //If customer and vehicle doesn´t exist, add
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

            List<string> emails = bookingEvent.Attendees ?? new List<string>();
            emails.Add(bookingEvent.CustomerEmail);
            emails.Add(resource.CalendarEmail);
            if (bookingEvent.SupplierEmailAddress != null) { emails.Add(bookingEvent.SupplierEmailAddress); } 

            var ev = GoogleCalendarHelper.GetGoogleEvent(bookingEvent);
            ev.Attendees = GoogleCalendarHelper.GetAttendees(emails);
              
            ev.Organizer = GoogleCalendarHelper.GetEventOrganizer();

            ev.Start = GoogleCalendarHelper.GetEventDateTime(bookingEvent.StartDateTime);
            ev.End = GoogleCalendarHelper.GetEventDateTime(bookingEvent.EndDateTime);

            var bookingLog = CreateBookingLog(bookingEvent);
            db.BookingLogs.Add(bookingLog);
            db.SaveChanges();

            GoogleCalendar calendar = new GoogleCalendar(Common.GetAppConfigValue("MoridgeOrganizerCalendarEmail"), Common.GetAppConfigValue("MoridgeMainCalendarEmail"));
            calendar.InsertEvent(ev);

            return bookingLog.Id;
        }

        public List<BookingEvent> GetAvailableDatesForBookingSwedishOffset(int productId, int maxCount)
        {
            List<BookingEvent> list = GetAvailableDatesForBooking(productId, maxCount);

            foreach (BookingEvent ev in list)
            {
                ev.StartDateTime = ev.StartDateTime.AddHours(Common.GetSwedishDateTimeOffsetFromUTC(ev.StartDateTime));
                ev.EndDateTime = ev.EndDateTime.AddHours(Common.GetSwedishDateTimeOffsetFromUTC(ev.EndDateTime));
            }

            return list;
        }

        public List<BookingEvent> GetAvailableDatesForBooking(int productId, int maxCount)
        {
            Resources resources = new Resources();
            Product product = db.Products.Find(productId);
            if (product == null)
            {
                throw new Exception("Product missing in database");
            }

            return resources.GetAvailableDatesForBooking(product, maxCount);
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
                ev.BookingHeader = item.BookingHeader;
                ev.BookingMessage = item.BookingMessage;
                ev.SupplierEmailAddress = item.SupplierEmailAddress;
                ev.CompanyName = item.CompanyName;

                events.Add(ev);
            }

            return events;
        }

        public List<BookingEvent> GetBookingsLastBooking()
        {
            var logPreSorted = db.BookingLogs.ToList().OrderBy(c => c.CustomerEmail);

            var log = from l in logPreSorted
                     group l by l.CustomerEmail into grp
                     select grp.OrderByDescending(g => g.StartDateTime).First();

            List<BookingEvent> events = new List<BookingEvent>();

            foreach (var item in log)
            {
                BookingEvent ev = new BookingEvent();

                ev.StartDateTime = item.StartDateTime;
                ev.EndDateTime = item.EndDateTime;
                ev.VehicleRegNo = item.VehicleRegNo;
                ev.CustomerAddress = item.CustomerAddress;
                ev.CustomerEmail = item.CustomerEmail;
                ev.CustomerOrgNo = item.CustomerOrgNo;
                ev.IsBooked = true;
                ev.BookingHeader = item.BookingHeader;
                ev.BookingMessage = item.BookingMessage;
                ev.SupplierEmailAddress = item.SupplierEmailAddress;
                ev.CompanyName = item.CompanyName;
                ev.ProductId = item.ProductId;

                events.Add(ev);
            }

            return events;
        }

        public void DeleteEvent(BookingEvent bookingEvent)
        {
            GoogleCalendar calendar = new GoogleCalendar(Common.GetAppConfigValue("MoridgeOrganizerCalendarEmail"), Common.GetAppConfigValue("MoridgeMainCalendarEmail"));

            calendar.DeleteEvent(calendar.FindGoogleEventId(bookingEvent));
        }

        private BookingLog CreateBookingLog(BookingEvent ev)
        {
            BookingLog log = new BookingLog();

            log.StartDateTime = ev.StartDateTime.ToLocalTime();
            log.EndDateTime = ev.EndDateTime.ToLocalTime();
            log.VehicleRegNo = ev.VehicleRegNo;
            log.CustomerAddress = ev.CustomerAddress;
            log.CustomerEmail = ev.CustomerEmail;
            log.CustomerOrgNo = ev.CustomerOrgNo;
            log.BookingMessage = ev.BookingMessage;
            log.CompanyName = ev.CompanyName;
            log.ResourceId = ev.ResourceId;
            log.BookingHeader = ev.BookingHeader;
            log.SupplierEmailAddress = String.IsNullOrEmpty(ev.SupplierEmailAddress) ? "" : ev.SupplierEmailAddress;
            log.ProductId = ev.ProductId;

            return log;
        }
    }
}
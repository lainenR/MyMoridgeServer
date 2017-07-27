using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MyMoridgeServer.BusinessLogic;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.Controllers
{
    public class BookingController : ApiController
    {
        // GET api/booking
        public IEnumerable<BookingEvent> Get()
        {
            Booking booking = new Booking();

            var dates = booking.Get15AvailableDatesForBookingSwedishOffset();

            return dates;
        }

        // GET api/booking/5
        public IEnumerable<BookingEvent> Get(string id)
        {
            Booking booking = new Booking();

            var bookings = booking.GetBookingsByOrganisation(id);

            return bookings;
        }

        // POST api/booking
        public void Post([FromBody]BookingEventDTO bookingEventDTO)
        {
            try
            {
                if (bookingEventDTO != null)
                {
                    BookingEvent bookingEvent = new BookingEvent();

                    var startDateTime = Convert.ToDateTime(bookingEventDTO.StartDateTime); 
                    var endDateTime = Convert.ToDateTime(bookingEventDTO.EndDateTime);
                    var offsetFromSwedish = Common.GetSwedishDateTimeOffsetFromUTC(startDateTime);

                    bookingEvent.StartDateTime = startDateTime.AddHours(offsetFromSwedish * -1).ToUniversalTime(); //Go to UTC time
                    bookingEvent.EndDateTime = endDateTime.AddHours(offsetFromSwedish * -1).ToUniversalTime(); //Go to UTC time
                    bookingEvent.CustomerOrgNo = bookingEventDTO.CustomerOrgNo;
                    bookingEvent.CustomerEmail = bookingEventDTO.CustomerEmail;
                    bookingEvent.CustomerAddress = bookingEventDTO.CustomerAddress;
                    bookingEvent.VehicleRegNo = bookingEventDTO.VehicleRegNo;
                    bookingEvent.IsBooked = true;
                    bookingEvent.CompanyName = bookingEventDTO.CompanyName;
                    bookingEvent.BookingHeader = bookingEventDTO.BookingHeader;
                    bookingEvent.BookingMessage = bookingEventDTO.BookingMessage;
                    bookingEvent.ResourceId = bookingEventDTO.ResourceId;
                    bookingEvent.SupplierEmailAddress = bookingEventDTO.SupplierEmailAddress;
                    bookingEvent.Attendees = bookingEventDTO.Attendees;

                    Booking booking = new Booking();
                    booking.BookEvent(bookingEvent);
                }
                else
                {
                    throw new Exception("BookingEventDTO is null");
                }
            }
            catch (Exception ex)
            {
                Exception e = new Exception(ex.Message + " - CustomerOrgNo:" + bookingEventDTO.CustomerOrgNo ?? "", ex);

                Common.LogError(e);    
            }
        }

        // PUT api/booking/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/booking/5
        public void Delete(int id)
        {
        }
    }
}
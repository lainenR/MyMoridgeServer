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

            var dates = booking.Get15AvailableDatesForBooking();

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

                    bookingEvent.StartDateTime = Convert.ToDateTime(bookingEventDTO.StartDateTime);
                    bookingEvent.EndDateTime = Convert.ToDateTime(bookingEventDTO.EndDateTime);
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
                Common.LogError(ex);
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
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
            return Get(1, 15);
        }

        // GET api/booking/
        public IEnumerable<BookingEvent> Get(int productId, int maxCount)
        {
            Booking booking = new Booking();
            List<BookingEvent> dates = null;

            try
            {
                dates = booking.GetAvailableDatesForBookingSwedishOffset(productId, maxCount);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }

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
                var bookingEvent = BookingEventBO.CreateBookingEvent(bookingEventDTO);

                Booking booking = new Booking();
                booking.BookEvent(bookingEvent);
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
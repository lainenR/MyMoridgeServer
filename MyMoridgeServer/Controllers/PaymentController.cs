using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using MyMoridgeServer.BusinessLogic;
using MyMoridgeServer.Models;
using Braintree;

namespace MyMoridgeServer.Controllers
{
    public class PaymentController : ApiController
    {
        private const string CONST_BOOKING_MESSAGE = "Moridge AB kommer under denna period att leverera tjänsten {0} avseende fordonet {1}. Fordonet beräknas hämtas och levereras inom detta angivna tidsintervall. Observera att viss avvikelse av tiden kan förekomma.";

        // GET api/payment
        public string Get()
        {
            IBraintreeConfiguration config = new BraintreeConfiguration();
            var gateway = config.GetGateway();
            var clientToken = gateway.ClientToken.generate();

            return clientToken;
        }

        // GET api/payment/5
        public string Get(int value)
        {
            return "value";
        }

        // POST api/payment
        public void Post([FromBody]BookingEventPaymentDTO bookingEventPaymentDTO)
        {
            try
            {
                var bookingLogId = -1;
                var bookingEvent = BookingEventBO.CreateBookingEvent(bookingEventPaymentDTO.BookingEventDTO);
                var product = new MyMoridgeServerModelContainer1().Products.Find(bookingEvent.ProductId);
                
                bookingEvent.BookingMessage = String.Format(CONST_BOOKING_MESSAGE, new object[] { product.Name, bookingEvent.VehicleRegNo });

                try
                {
                    bookingLogId = new Booking().BookEvent(bookingEvent);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while trying to book event!", ex);
                }

                var paymentHandler = new Charge();
                var transactionComplete = paymentHandler.ChargeTransaction(bookingLogId, product.PriceInclVat, bookingEventPaymentDTO.PaymentMethodNonce);

                if (!transactionComplete)
                {
                    new Booking().DeleteEvent(bookingEvent);
                    throw new Exception("Error while trying to charge payment!");
                }
            }
            catch (Exception ex)
            {

                Exception e = new Exception(ex.Message + " - CustomerOrgNo:" + bookingEventPaymentDTO.BookingEventDTO.CustomerOrgNo ?? "", ex);

                Common.LogError(e);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }
        }

        // PUT api/payment/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/payment/5
        public void Delete(int id)
        {
        }
    }
}

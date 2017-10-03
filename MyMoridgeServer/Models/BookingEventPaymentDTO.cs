using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoridgeServer.Models
{
    public class BookingEventPaymentDTO
    {
        public BookingEventDTO BookingEventDTO { get; set; }
        public string PaymentMethodNonce { get; set; }
    }
}
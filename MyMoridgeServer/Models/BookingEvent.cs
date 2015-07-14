using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoridgeServer.Models
{
    public class BookingEvent
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string CustomerOrgNo { get; set; }
        public string CustomerEmail { get; set; } 
        public string CustomerAddress { get; set; }
        public string VehicleRegNo { get; set; }
        public bool IsBooked { get; set; }
        public string CompanyName { get; set; }
        public string BookingMessage { get; set; }
        public int ResourceId { get; set; }
    }
}
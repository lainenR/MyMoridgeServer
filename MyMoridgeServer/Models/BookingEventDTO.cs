using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoridgeServer.Models
{
    public class BookingEventDTO
    {
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public string CompanyName { get; set; }
        public string CustomerOrgNo { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string VehicleRegNo { get; set; }
        public string BookingHeader { get; set; }
        public string BookingMessage { get; set; }
        public string IsBooked { get; set; }
        public int ResourceId { get; set; }
        public string SupplierEmailAddress { get; set; }
        public List<string> Attendees { get; set; }
    }
}
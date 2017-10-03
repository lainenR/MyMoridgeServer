using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoridgeServer.Models
{
    public class BookingSlot
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int MaxBookings { get; set; }
        public bool IsMorningTime { get; set; }

        public BookingSlot(DateTime startDateTime, DateTime endDateTime, int maxBookings, bool isMorningTime)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            MaxBookings = maxBookings;
            IsMorningTime = isMorningTime;
        }
    }
}
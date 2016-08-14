using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.BusinessLogic;

namespace MyMoridgeServer.Models
{
    public class InvitationModel
    {
        public List<BookingEvent> BookingEvents { get; set; }
        public List<BookingEvent> BookingLogs { get; set; }
        public Email Email { get; set; }
    }
}
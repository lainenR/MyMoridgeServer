using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoridgeServer.Models
{
    public class Email
    {
        public string Subject { get; set; }
        public string BodyText { get; set; }
        public string Footer { get; set; }
        public List<EmailLog> Recipients { get; set; }

        public Email()
        {
            Subject = "Vi har lediga tider till dig";
            BodyText = "Hej!\n\nVi har lediga tvätttider till dig, boka nu genom att klicka på ett av datumen nedan.";
            Recipients = new List<EmailLog>();
        }
    }
}
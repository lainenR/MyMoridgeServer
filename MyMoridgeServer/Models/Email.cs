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
            BodyText = "Hur mår bilen – behöver den tvättas? Nu är det är bara ett klick bort. Vi har tagit fram våra närmaste tider till dig. Välj den tid som passar dig bäst så fixar vi resten.";
            Recipients = new List<EmailLog>();
        }
    }
}
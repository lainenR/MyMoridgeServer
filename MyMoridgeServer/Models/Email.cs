using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMoridgeServer.Models
{
    public class Email
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Footer { get; set; }
        public string SuggestedLinks { get; set; }
        public List<EmailLog> Recipients { get; set; }
    }
}
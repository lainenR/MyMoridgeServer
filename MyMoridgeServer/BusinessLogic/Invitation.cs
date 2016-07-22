using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Invitation
    {
        Email email;

        public Invitation()
        {
            email = new Email();

            email.Subject = "Vi har lediga tider till dig";
            email.Body = "Hej!\n Följande tider finns lediga hos oss just nu för bokning, skynda att boka innan dom försvinner genom att klicka på länken.";
            email.Footer = "\n Med vänliga hälsningar\n Moridgeteamet";
            email.Recipients = new List<EmailLog>();
        }

        public void AddRecipent(string customerEmail, string companyName, string vehicleRegNo)
        {
            EmailLog log = new EmailLog();

            log.CustomerEmail = customerEmail;
            log.CompanyName = companyName;
            log.VehicleRegNo = vehicleRegNo;
            log.Sent = DateTime.MinValue;

            email.Recipients.Add(log);
        }

        public void send()
        {
            string host = Common.GetAppConfigValue("InvitationHost");
            int port = Convert.ToInt32(Common.GetAppConfigValue("invitationPort"));
            string userName = Common.GetAppConfigValue("InvitationUserName");
            string password = Common.GetAppConfigValue("InvitationPassword");
            string from = Common.GetAppConfigValue("invitationFrom");
            string fromDisplayName = Common.GetAppConfigValue("invitationFromDisplayName");

            SmtpClient smtpClient = new SmtpClient(host, port);

            smtpClient.Credentials = new System.Net.NetworkCredential(userName, password);
            smtpClient.UseDefaultCredentials = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            MailMessage mail = new MailMessage();
            
            mail.From = new MailAddress(from, fromDisplayName);
            mail.Subject = email.Subject;
            mail.Body = email.Body;

            foreach(var rec in email.Recipients)
            {
                mail.To.Add(new MailAddress(rec.CustomerEmail));
                smtpClient.Send(mail);
                rec.Sent = DateTime.UtcNow;
            }
        }
    }
}
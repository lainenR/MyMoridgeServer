using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Net.Mail;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Invitation
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();
        private Email Email;
        public struct Date
        {
            public DateTime StartDateTime {get; set;}
            public DateTime EndDateTime { get; set; }
        };

        private List<Date> Dates;

        public Invitation()
        {
            Email = new Email();
            Dates = new List<Date>();

            new InvitationVoucherHelper().RemoveOldVouchers();
        }

        public void AddRecipent(string customerEmail)
        {
            var booking = db.BookingLogs.First(l => l.CustomerEmail == customerEmail);

            if (booking != null)
            {
                EmailLog log = new EmailLog();

                log.CustomerEmail = booking.CustomerEmail;
                log.CompanyName = booking.CompanyName;
                log.VehicleRegNo = booking.VehicleRegNo;
                log.Sent = new DateTime(2000, 1, 1);

                Email.Recipients.Add(log);
            }
        }

        public void AddDate(string start, string end)
        {
            try
            {
                Date date = new Date();
                date.StartDateTime = Convert.ToDateTime(start);
                date.EndDateTime = Convert.ToDateTime(end);
                
                Dates.Add(date);
            }
            catch (Exception ex)
            {
                Common.LogError(new Exception("Unable to add date, probably because of conversion from string to datetime", ex));
            }
        }

        public void SetBodyText(string body)
        {
            Email.BodyText = body.Replace("\n", "<br \\>");
        }

        public void Send()
        {
            string host = Common.GetAppConfigValue("InvitationHost");
            int port = Convert.ToInt32(Common.GetAppConfigValue("InvitationPort"));
            string userName = Common.GetAppConfigValue("InvitationUserName");
            string password = Common.GetAppConfigValue("InvitationPassword");
            string from = Common.GetAppConfigValue("InvitationFrom");
            string fromDisplayName = Common.GetAppConfigValue("InvitationFromDisplayName");

            SmtpClient smtpClient = new SmtpClient(host, port);

            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(userName, password);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;

            foreach(var rec in Email.Recipients)
            {
                try
                {
                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress(from, fromDisplayName);
                    mail.Subject = Email.Subject;
                    mail.IsBodyHtml = true;

                    db.EmailLogs.Add(rec);
                    rec.Sent = DateTime.UtcNow;
                    db.SaveChanges();

                    mail.Body = new InvitationMessage(rec, Dates).GetHtmlBody(Email.BodyText);

                    mail.To.Add(new MailAddress(rec.CustomerEmail));
                    smtpClient.Send(mail);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex);
                }
            }
        }
    }
}
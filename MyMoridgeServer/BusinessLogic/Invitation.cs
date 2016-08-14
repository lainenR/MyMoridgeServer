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
        private const string CONST_MORNING = "(Förmiddag)";
        private const string CONST_AFTERNOON = "(Eftermiddag)";
        private const string CONST_INVITATIONHTMLFILE = "invitation.html";

        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();
        private Email Email;
        private struct Date
        {
            public DateTime StartDateTime {get; set;}
            public DateTime EndDateTime { get; set; }
        };

        private List<Date> Dates;

        public Invitation()
        {
            Email = new Email();
            Dates = new List<Date>();

            RemoveOldVouchers();
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
                log.Sent = DateTime.MinValue;

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
            MailMessage mail = new MailMessage();
            
            mail.From = new MailAddress(from, fromDisplayName);
            mail.Subject = Email.Subject;
            mail.IsBodyHtml = true;

            foreach(var rec in Email.Recipients)
            {
                try
                {
                    mail.Body = GetHtmlBody(rec.CustomerEmail);

                    mail.To.Add(new MailAddress(rec.CustomerEmail));
                    smtpClient.Send(mail);

                    rec.Sent = DateTime.UtcNow;
                    db.EmailLogs.Add(rec);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.LogError(ex);
                }
            }
        }

        public string GetHtmlBody(string customerEmail)
        {
            string html;
            string fileName = System.Web.HttpContext.Current.Server.MapPath(@"~/" + CONST_INVITATIONHTMLFILE);

            try
            {
                html = File.ReadAllText(fileName);
            }
            catch (IOException ex)
            {
                Exception e = new Exception("Error locating or handling file: " + fileName, ex);
                Common.LogError(e);
                throw e;
            }

            string text = GetHtmlForDate(customerEmail);

            return html.Replace("{Invitation.Body}", Email.BodyText).Replace("{Invitation.Dates}", text);
        }

        private string GetHtmlForDate(string customerEmail)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var date in Dates)
            {
                sb.Append("<tr><td>");
                sb.Append("<a href=\"http://service.moridge.se/invitationbooking.aspx?v=");
                sb.Append(AddVoucher(customerEmail, date.StartDateTime, date.EndDateTime).ToString());
                sb.Append("\">").Append(GetDateText(date.StartDateTime)).Append("</a>");
                sb.Append("</td></tr>");
            }

            return sb.ToString();
        }

        private string GetDateText(DateTime date)
        {
            StringBuilder text = new StringBuilder(date.ToShortDateString()).Append(" ");

            if (date.Hour.CompareTo(12) < 0)
            {
                text.Append(CONST_MORNING);
            }
            else
            {
                text.Append(CONST_AFTERNOON);
            }
           
            return text.ToString();
        }

        private Guid AddVoucher(string customerEmail, DateTime start, DateTime end)
        {
            var booking = db.BookingLogs.First(l => l.CustomerEmail == customerEmail);
            InvitationVoucher voucher = new InvitationVoucher();

            if (booking != null)
            {
                voucher.VoucherId = Guid.NewGuid();
                voucher.BookingLogId = booking.Id;
                voucher.StartDateTime = start;
                voucher.EndDateTime = end;

                db.InvitationVouchers.Add(voucher);
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Error creating voucher for customer email: " + customerEmail);
            }

            return voucher.VoucherId;
        }

        private void RemoveOldVouchers()
        {
            DateTime compareDate = DateTime.Now.AddDays(-3);

            try
            {
                foreach (var voucher in db.InvitationVouchers.Where(item => item.StartDateTime.CompareTo(compareDate) < 0))
                {
                    db.InvitationVouchers.Remove(voucher);
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Exception e = new Exception("Error removing old vouchers in db", ex);

                Common.LogError(e);
            }
        }
    }
}
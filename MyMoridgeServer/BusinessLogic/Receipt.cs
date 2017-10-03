using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Net.Mail;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class Receipt
    {
        private const string CONST_INVITATIONHTMLFILE = "receipt.html";
        private const string CONST_EMAILHEADER = "Kvitto för betalning till Moridge AB";

        Payment Payment;

        public Receipt(Payment payment)
        {
            Payment = payment;
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
            mail.Subject = CONST_EMAILHEADER;
            mail.IsBodyHtml = true;

            mail.Body = GetHtmlBody();

            mail.To.Add(new MailAddress(Payment.BookingLog.CustomerEmail));
            smtpClient.Send(mail);
        }

        private string GetHtmlBody()
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

            return html.Replace("{BookingLog.Id}", Payment.BookingLogId.ToString())
                .Replace("{Product.Name}", Payment.BookingLog.Product.Name)
                .Replace("{Price.Total}", Payment.Amount.ToString())
                .Replace("{Price.InclVAT}", Payment.BookingLog.Product.PriceInclVat.ToString())
                .Replace("{Price.TotalVAT}", (Payment.BookingLog.Product.PriceInclVat - Payment.BookingLog.Product.PriceExclVat).ToString());
        }
    }
}
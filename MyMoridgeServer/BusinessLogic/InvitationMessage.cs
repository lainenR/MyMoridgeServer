using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class InvitationMessage : Invitation
    {
        private EmailLog EmailLog;
        private List<Date> Dates;

        private const string CONST_MORNING = "(Förmiddag)";
        private const string CONST_AFTERNOON = "(Eftermiddag)";
        private const string CONST_INVITATIONHTMLFILE = "invitation.html";

        public InvitationMessage(EmailLog emailLog, List<Date> dates)
        {
            EmailLog = emailLog;
            Dates = dates;
        }

        public string GetHtmlBody(string bodyText)
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

            string text = GetHtmlForDate();

            return html.Replace("{Invitation.Body}", bodyText).Replace("{Invitation.Dates}", text);
        }

        private string GetHtmlForDate()
        {
            StringBuilder sb = new StringBuilder();
            InvitationVoucherHelper invitationVoucherHelper = new InvitationVoucherHelper();

            foreach (var date in Dates)
            {
                sb.Append("<tr ><td>");
                sb.Append("<a href=\"http://service.moridge.se/invitationbooking.aspx?v=");
                sb.Append(invitationVoucherHelper.AddVoucher(EmailLog, date).ToString());
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
    }
}
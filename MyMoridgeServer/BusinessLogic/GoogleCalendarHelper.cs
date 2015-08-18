using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Google.Apis.Calendar.v3.Data;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class GoogleCalendarHelper
    {
        public static Event GetGoogleEvent(BookingEvent ev)
        {
            var googleEvent = new Event();
            StringBuilder header = new StringBuilder(ev.BookingHeader);

            if (header.Length == 0)
            {
                if(!String.IsNullOrWhiteSpace(ev.BookingMessage))
                {
                    header.Append("[K] ");
                }
                header.Append("Moridge - ").Append(ev.CompanyName).Append(" ").Append(ev.VehicleRegNo);
                ev.BookingHeader = header.ToString();
            }

            googleEvent.Summary = header.ToString();
            googleEvent.Description = ev.BookingMessage;
            googleEvent.Location = ev.CustomerAddress;

            return googleEvent;
        }

        public static EventAttendee GetAttende(string email)
        {

            var attende = new EventAttendee();
            attende.Email = email;

            return attende;
        }

        public static EventDateTime GetEventStart(DateTime start)
        {
            var eventDateTime = new EventDateTime();

            eventDateTime.DateTimeRaw = start.ToString("yyyy-MM-dd") + "T" + start.TimeOfDay.ToString() + "+01:00";

            return eventDateTime;
        }

        public static EventDateTime GetEventEnd(DateTime end)
        {
            var eventDateTime = new EventDateTime();

            eventDateTime.DateTimeRaw = end.Date.ToString("yyyy-MM-dd") + "T" + end.TimeOfDay.ToString() + "+01:00";

            return eventDateTime;
        }

        public static Event.OrganizerData GetEventOrganizer()
        {
            Event.OrganizerData data = new Event.OrganizerData();

            data.DisplayName = "Moridge";
            data.DisplayName = Common.GetAppConfigValue("MoridgeOrganizerCalendarEmail"); ;

            return data;
        }

        public static Events HandleEventStartEndNull(Events events)
        {
            foreach (var ev in events.Items)
            {
                if (ev.Start.DateTime == null)
                {
                    ev.Start.DateTime = Convert.ToDateTime(ev.Start.Date + " 00:00:00");
                }
                if (ev.End.DateTime == null)
                {
                    ev.End.DateTime = Convert.ToDateTime(ev.End.Date + " 00:00:00");
                }
            }

            return events;
        }
    }
}
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
            StringBuilder sb = new StringBuilder();

            sb.Append("Moridge - ").Append(ev.CompanyName).Append(" ").Append(ev.VehicleRegNo);
            googleEvent.Summary = sb.ToString();
            googleEvent.Description = ev.BookingMessage;

            return googleEvent;
        }

        public static EventAttendee GetUserAttende(string email)
        {

            var attende = new EventAttendee();
            attende.Email = email;

            return attende;
        }

        public static EventAttendee GetMainMoridgeCalendarAttende(string email)
        {
            var attende = new EventAttendee();

            attende.Email = email;

            return attende;
        }

        public static EventAttendee GetMoridgeDriverCalendarAttende(string email)
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
    }
}
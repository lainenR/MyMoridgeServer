using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Configuration;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using MyMoridgeServer.Models;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

using Google.Apis.Util;

namespace MyMoridgeServer.BusinessLogic
{
    public class GoogleCalendar
    {        

        private CalendarService GoogleService { get; set; }

        public GoogleCalendar()
        {

        }

        public CalendarList GetCalendarList()
        {
            try
            {
                CreateGoogleService();
                
                return GoogleService.CalendarList.List().Execute();
            }
            catch (Exception e)
            {
                throw new Exception("Error getting CalendarList from Google", e);
            }
        }

        public Events GetEventList(string calendarId)
        {
            try
            {
                CreateGoogleService();

                return GoogleService.Events.List(calendarId).Execute();
            }
            catch (Exception e)
            {
                throw new Exception("Error getting Events from Google", e);
            }
        }

        public void InsertEvent(Event ev, string calendarId)
        {
            try
            {
                CreateGoogleService();

                var insertRequest = new EventsResource.InsertRequest(GoogleService, ev, calendarId);

                insertRequest.SendNotifications = true;

                insertRequest.Execute();
            }
            catch (Exception e)
            {
                throw new Exception("Error inserting event to Google: " + e.Message, e);
            }
        }

        public void DeleteEvent(string calendarId, string eventId)
        {
            CreateGoogleService();

            GoogleService.Events.Delete(calendarId, eventId).Execute();
        }

        //Service account
        private void CreateGoogleService()
        {
            try
            {
                string serviceAccountEmail = Common.GetAppConfigValue("MoridgeMainCalendarEmail");
                string keyFile = AppDomain.CurrentDomain.BaseDirectory + "key.p12";
                var certificate = new X509Certificate2(keyFile, "notasecret", X509KeyStorageFlags.Exportable);

                // Create credentials
                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail)
                   {
                       Scopes = new[] { CalendarService.Scope.Calendar }
                   }.FromCertificate(certificate));

                // Create the service
                GoogleService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = Common.GetAppConfigValue("GoogleApplicationName"),
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Google service!", ex);
            }
        }

        /* Client account
        private void CreateGoogleService()
        {
            try
            {
                // Create credentials
                UserCredential credential;

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = Common.GetAppConfigValue("GoogleClientId"),
                    ClientSecret = Common.GetAppConfigValue("GoogleClientSecret")
                },
                new[] { CalendarService.Scope.Calendar },
                "user",
                CancellationToken.None,
                    //new FileDataStore("Calendar.ListMyCalendars")).Result;
                    //new WebFileDataStore("Calendar.ListMyCalendars")).Result;
                new DbDataStore()).Result;

                // Create the service.
                GoogleService = new CalendarService(new BaseClientService.Initializer
                {
                    ApplicationName = Common.GetAppConfigValue("GoogleApplicationName"),
                    HttpClientInitializer = credential,
                });

            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Google service!", ex);
            }
        }*/
    }
}
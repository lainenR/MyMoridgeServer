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
        

        private string CalendarServiceAccountEmail { get; set;}
        private string CalendarEmail { get; set; }
        private string FileNameP12 { get; set; }

        public GoogleCalendar(string calendarEmail, string calendarServiceAccountEmail)
        {
            CalendarServiceAccountEmail = calendarServiceAccountEmail;
            CalendarEmail = calendarEmail;
            FileNameP12 = GetFileNameP12(calendarEmail);
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

        public Events GetEventList()
        {
            try
            {
                CreateGoogleService();

                return GoogleService.Events.List(CalendarEmail) .Execute();
            }
            catch (Exception e)
            {
                throw new Exception("Error getting Events from Google", e);
            }
        }

        public void InsertEvent(Event ev)
        {
            try
            {
                CreateGoogleService();

                var insertRequest = new EventsResource.InsertRequest(GoogleService, ev, CalendarEmail);

                insertRequest.SendNotifications = true;

                insertRequest.Execute();
            }
            catch (Exception e)
            {
                throw new Exception("Error inserting event to Google: " + e.Message, e);
            }
        }

        public void DeleteEvent(string eventId)
        {
            CreateGoogleService();

            GoogleService.Events.Delete(CalendarEmail, eventId).Execute();
        }

        //Service account
        private void CreateGoogleService()
        {
            try
            {
                string serviceAccountEmail = CalendarServiceAccountEmail;
                string keyFile = AppDomain.CurrentDomain.BaseDirectory + FileNameP12;
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

        private string GetFileNameP12(string email)
        {
            int index = 0;
            string fileName = "";

            try
            {
                index = email.IndexOf("@");

                if (index == 0)
                {
                    throw new Exception("Calendar email error, check email address");

                }
                fileName = email.Substring(0, index) + ".p12";
            }
            catch(Exception ex)
            {
                Common.LogError(ex);
            }

            return fileName;
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
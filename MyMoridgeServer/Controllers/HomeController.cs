using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyMoridgeServer.Models;
using MyMoridgeServer.BusinessLogic;

namespace MyMoridgeServer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new InvitationModel();
            model.BookingEvents = GetDates();
            model.BookingLogs = GetBookingLogs();
            model.Email = new Email();

            BookingEvent e = new BookingEvent();

            Invitation i = new Invitation();

            return View(model);
        }

        [HttpPost]
        public ActionResult LogIn(string username, string password)
        {
            if (ModelState.IsValid)
            {
                if (new Login().DoLogin(username, password))
                {
                    FormsAuthentication.SetAuthCookie(username, true);
                }
            }

            return RedirectToAction("Index", "home");
        }

        [HttpPost]
        public ActionResult SendInvitation(string[] selectedEmails, string[] selectedDates, string body)
        {
            try
            {
                Invitation invitation = new Invitation();
    
                foreach(var email in selectedEmails)
                {
                    if (email != "false")
                    {
                        invitation.AddRecipent(email);
                    }
                }

                foreach(var date in selectedDates)
                {
                    if (date != "false")
                    {
                        invitation.AddDate(date.Split(';')[0], date.Split(';')[1]);
                    }
                }

                invitation.SetBodyText(body);
                invitation.Send();
            }
            catch(Exception ex)
            {
                Common.LogError(ex);
            }

            return RedirectToAction("Index", "Home");
        }

         
        private List<BookingEvent> GetDates()
        {
            Booking booking = new Booking();

            var events = booking.Get15AvailableDatesForBookingSwedishOffset();

            return events;
        }

        private List<BookingEvent> GetBookingLogs()
        {
            Booking booking = new Booking();

            var bookings = booking.GetBookingsLastBooking();

            return bookings;
        }
    }
}

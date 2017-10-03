using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyMoridgeServer.BusinessLogic;
using MyMoridgeServer.Models;

namespace MyMoridgeServer
{
    public partial class InvitationBooking : System.Web.UI.Page
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();
        private const string CONST_BOOKING_MESSAGE = "Moridge AB kommer under denna period att leverera tjänsten Biltvätt avseende fordonet {0}";
        private const string CONST_BOOKING_HEADER = "(e)Biltvätt av {0} ({1})";

        private enum BookingStatus
        {
            Success,
            NotAvailable,
            Error
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (DoSecurityCheck())
            {
                var result = DoBook();
                RedirectUser(result);
            }
        }

        private bool DoSecurityCheck()
        {
            bool ok = false;

            if (Request.QueryString["v"] != null)
            {
                ok = true;
            }

            return ok;
        }

        private BookingStatus DoBook()
        {
            try
            {
                var voucher = new Guid(Request.QueryString["v"]);

                var invitationVoucher = db.InvitationVouchers.Find(voucher);

                var offsetFromLocal = TimeZone.CurrentTimeZone.GetUtcOffset(invitationVoucher.StartDateTime);
                var startDateTime = invitationVoucher.StartDateTime.AddHours(offsetFromLocal.Hours);
                var endDateTime = invitationVoucher.EndDateTime.AddHours(offsetFromLocal.Hours);

                var bookingEvent = BookingEventBO.CreateBookingEvent(startDateTime, endDateTime, invitationVoucher.BookingLog.CustomerOrgNo,
                    invitationVoucher.BookingLog.CustomerEmail, invitationVoucher.BookingLog.CustomerAddress, invitationVoucher.BookingLog.VehicleRegNo,
                    true, invitationVoucher.BookingLog.CompanyName,
                    String.Format(CONST_BOOKING_HEADER, invitationVoucher.BookingLog.VehicleRegNo, invitationVoucher.BookingLog.CompanyName),
                    String.Format(CONST_BOOKING_MESSAGE, invitationVoucher.BookingLog.VehicleRegNo), 0, invitationVoucher.BookingLog.SupplierEmailAddress, null, 0);


                /*
                TimeZone tz = TimeZone.CurrentTimeZone;
                var offsetFromLocal = TimeZone.CurrentTimeZone.GetUtcOffset(invitationVoucher.StartDateTime);
                var offsetFromSwedish = Common.GetSwedishDateTimeOffsetFromUTC(invitationVoucher.StartDateTime);

                bookingEvent.StartDateTime = invitationVoucher.StartDateTime.AddHours(offsetFromLocal.Hours).AddHours(offsetFromSwedish * -1).ToUniversalTime(); //Go to UTC time
                bookingEvent.EndDateTime = invitationVoucher.EndDateTime.AddHours(offsetFromLocal.Hours).AddHours(offsetFromSwedish * -1).ToUniversalTime(); //Go to UTC time
                bookingEvent.CustomerOrgNo = invitationVoucher.BookingLog.CustomerOrgNo;
                bookingEvent.CustomerEmail = invitationVoucher.BookingLog.CustomerEmail;
                bookingEvent.CustomerAddress = invitationVoucher.BookingLog.CustomerAddress;
                bookingEvent.VehicleRegNo = invitationVoucher.BookingLog.VehicleRegNo;
                bookingEvent.IsBooked = true;
                bookingEvent.CompanyName = invitationVoucher.BookingLog.CompanyName;
                bookingEvent.BookingHeader = String.Format(CONST_BOOKING_HEADER, invitationVoucher.BookingLog.VehicleRegNo, invitationVoucher.BookingLog.CompanyName);
                bookingEvent.BookingMessage = String.Format(CONST_BOOKING_MESSAGE,  invitationVoucher.BookingLog.VehicleRegNo);
                bookingEvent.ResourceId = 0;
                bookingEvent.SupplierEmailAddress = invitationVoucher.BookingLog.SupplierEmailAddress;
                */
                 
                var booking = new Booking();
                BookingStatus returnVal = BookingStatus.Error;

                if (booking.IsBookingDateAvailable(bookingEvent))
                {
                    booking.BookEvent(bookingEvent);
                    returnVal = BookingStatus.Success;
                }
                else
                {
                    returnVal = BookingStatus.NotAvailable;
                }

                return returnVal;
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                return BookingStatus.Error;
            }
        }

        private void RedirectUser(BookingStatus status)
        {
            var redirectUrl = Common.GetAppConfigValue("RedirectAfterBookingSuccess");

            switch(status)
            {
                case BookingStatus.Success:
                    redirectUrl = Common.GetAppConfigValue("RedirectAfterBookingSuccess");
                    break;
                case BookingStatus.NotAvailable:
                    redirectUrl = Common.GetAppConfigValue("RedirectAfterBookingNotAvailable");
                    break;
                case BookingStatus.Error:
                    redirectUrl = Common.GetAppConfigValue("RedirectAfterBookingError");
                    break;
            }

            Response.Redirect(redirectUrl);
        }
    }
}
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
        private const string CONST_BOOKING_HEADER = "Biltvätt av {0} ({1})";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (DoSecurityCheck())
            {
                var ok = DoBook();
                RedirectUser(ok);
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

        private bool DoBook()
        {
            try
            {
                var voucher = new Guid(Request.QueryString["v"]);

                var invitationVoucher = db.InvitationVouchers.Find(voucher);

                var bookingEvent = new BookingEvent();

                bookingEvent.StartDateTime = invitationVoucher.StartDateTime;
                bookingEvent.EndDateTime = invitationVoucher.EndDateTime;
                bookingEvent.CustomerOrgNo = invitationVoucher.BookingLog.CustomerOrgNo;
                bookingEvent.CustomerEmail = invitationVoucher.BookingLog.CustomerEmail;
                bookingEvent.CustomerAddress = invitationVoucher.BookingLog.CustomerAddress;
                bookingEvent.VehicleRegNo = invitationVoucher.BookingLog.VehicleRegNo;
                bookingEvent.IsBooked = true;
                bookingEvent.CompanyName = invitationVoucher.BookingLog.CompanyName;
                bookingEvent.BookingHeader = String.Format(CONST_BOOKING_HEADER, invitationVoucher.BookingLog.VehicleRegNo, invitationVoucher.BookingLog.CompanyName);
                bookingEvent.BookingMessage = String.Format(CONST_BOOKING_MESSAGE,  invitationVoucher.BookingLog.VehicleRegNo);
                bookingEvent.ResourceId = invitationVoucher.BookingLog.ResourceId;
                bookingEvent.SupplierEmailAddress = invitationVoucher.BookingLog.SupplierEmailAddress;

                var booking = new Booking();
                booking.BookEvent(bookingEvent);

                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                return false;
            }
        }

        private void RedirectUser(bool ok)
        {
            var redirectUrl = Common.GetAppConfigValue("RedirectAfterBookingSuccess");

            if (!ok)
            {
                redirectUrl = Common.GetAppConfigValue("RedirectAfterBookingError");
            }

            Response.Redirect(redirectUrl);
        }
    }
}
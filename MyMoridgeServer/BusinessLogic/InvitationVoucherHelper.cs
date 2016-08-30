using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class InvitationVoucherHelper
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();

        public InvitationVoucherHelper()
        {
            RemoveOldVouchers();
        }

        public Guid AddVoucher(EmailLog emailLog, BusinessLogic.Invitation.Date date)
        {
            var booking = db.BookingLogs.First(l => l.CustomerEmail == emailLog.CustomerEmail);
            InvitationVoucher voucher = new InvitationVoucher();

            if (booking != null)
            {
                voucher.VoucherId = Guid.NewGuid();
                voucher.BookingLogId = booking.Id;
                voucher.StartDateTime =date.StartDateTime;
                voucher.EndDateTime = date.EndDateTime;
                voucher.EmailLogId = emailLog.Id;

                db.InvitationVouchers.Add(voucher);
                db.SaveChanges();
            }
            else
            {
                throw new Exception("Error creating voucher for customer email: " + emailLog.CustomerEmail);
            }

            return voucher.VoucherId;
        }

        public void RemoveOldVouchers()
        {
            DateTime compareDate = DateTime.Now.AddDays(-30);

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
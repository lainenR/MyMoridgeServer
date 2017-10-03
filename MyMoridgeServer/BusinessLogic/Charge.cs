using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.Models;
using Braintree;

namespace MyMoridgeServer.BusinessLogic
{
    public class Charge
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();

        public bool ChargeTransaction(int bookingLogId, Decimal amount, string paymentMethodNonce)
        {
            bool ok = false;
            IBraintreeConfiguration config = new BraintreeConfiguration();
            var gateway = config.GetGateway();

            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = paymentMethodNonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                ok = true;

                Transaction transaction = result.Target;
                Payment payment = new Payment();

                if (transaction != null)
                {                    
                    payment.TransId = transaction.Id ?? "";
                    payment.TransType = transaction.Type.ToString() ?? "";
                    payment.Amount = transaction.Amount.Value;
                    payment.Status = transaction.Status.ToString() ?? "";
                    payment.Created = transaction.CreatedAt.Value;
                    payment.PaymentInstrumentType = transaction.PaymentInstrumentType.ToString() ?? "";
                    payment.BookingLogId = bookingLogId;

                    payment.BookingLog = db.BookingLogs.Find(bookingLogId);
                    new Receipt(payment).Send();

                    db.Payments.Add(payment);
                    db.SaveChanges();
                }
            }
            else
            {
                string errorMessages = "";
                foreach (ValidationError error in result.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }

                throw new Exception(errorMessages);
            }

            return ok;
        }
    }
}
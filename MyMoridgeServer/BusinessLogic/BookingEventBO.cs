using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.BusinessLogic
{
    public class BookingEventBO
    {
        public static BookingEvent CreateBookingEvent(BookingEventDTO bookingEventDTO)
        {
            BookingEvent bookingEvent = new BookingEvent();

            if (bookingEventDTO != null)
            {
                var startDateTime = Convert.ToDateTime(bookingEventDTO.StartDateTime);
                var endDateTime = Convert.ToDateTime(bookingEventDTO.EndDateTime);
                bookingEventDTO.Attendees.RemoveAll(item => item == null);

                bookingEvent = CreateBookingEvent(startDateTime, endDateTime, bookingEventDTO.CustomerOrgNo,
                    bookingEventDTO.CustomerEmail, bookingEventDTO.CustomerAddress, bookingEventDTO.VehicleRegNo, true, bookingEventDTO.CompanyName,
                    bookingEventDTO.BookingHeader, bookingEventDTO.BookingMessage, bookingEventDTO.ResourceId, bookingEventDTO.SupplierEmailAddress,
                    bookingEventDTO.Attendees, bookingEventDTO.ProductId);
            }
            else
            {
                throw new Exception("BookingEventDTO is null");
            }

            return bookingEvent;
        }

        public static BookingEvent CreateBookingEvent(DateTime startDateTime, DateTime endDateTime, string customerOrgNo, string customerEmail,
            string customerAddress, string vehicleRegNo, bool isBooked, string companyName, string bookingHeader, string bookingMessage, 
            int resourceId, string supplierEmailAddress, List<string> attendees, int productId)
        {
            BookingEvent bookingEvent = new BookingEvent();

            var offsetFromSwedish = Common.GetSwedishDateTimeOffsetFromUTC(startDateTime);

            bookingEvent.StartDateTime = startDateTime.AddHours(offsetFromSwedish * -1).ToUniversalTime(); //Go to UTC time
            bookingEvent.EndDateTime = endDateTime.AddHours(offsetFromSwedish * -1).ToUniversalTime(); //Go to UTC time
            bookingEvent.CustomerOrgNo = customerOrgNo;
            bookingEvent.CustomerEmail = customerEmail;
            bookingEvent.CustomerAddress = customerAddress;
            bookingEvent.VehicleRegNo = vehicleRegNo;
            bookingEvent.IsBooked = isBooked;
            bookingEvent.CompanyName = companyName;
            bookingEvent.BookingHeader = bookingHeader;
            bookingEvent.BookingMessage = bookingMessage;
            bookingEvent.ResourceId = resourceId;
            bookingEvent.SupplierEmailAddress = supplierEmailAddress;
            bookingEvent.Attendees = attendees;
            bookingEvent.ProductId = productId == 0 ? 1 : productId;
            
            return bookingEvent;
        }
    }
}
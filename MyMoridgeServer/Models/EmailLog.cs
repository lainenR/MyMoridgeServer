//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyMoridgeServer.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmailLog
    {
        public EmailLog()
        {
            this.InvitationVoucher = new HashSet<InvitationVoucher>();
        }
    
        public int Id { get; set; }
        public string CustomerEmail { get; set; }
        public string CompanyName { get; set; }
        public string VehicleRegNo { get; set; }
        public System.DateTime Sent { get; set; }
    
        public virtual ICollection<InvitationVoucher> InvitationVoucher { get; set; }
    }
}

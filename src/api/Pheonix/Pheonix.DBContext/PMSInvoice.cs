//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pheonix.DBContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class PMSInvoice
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PMSInvoice()
        {
            this.PMSInvoiceDetails = new HashSet<PMSInvoiceDetails>();
            this.PMSInvoicePayments = new HashSet<PMSInvoicePayments>();
        }
    
        public int Id { get; set; }
        public Nullable<int> Category { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<int> CreditDays { get; set; }
        public Nullable<int> Currency { get; set; }
        public Nullable<int> ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public Nullable<int> Contract { get; set; }
        public Nullable<int> CustomerAddress { get; set; }
        public Nullable<int> SalesPeriod { get; set; }
        public Nullable<decimal> TotalAmt { get; set; }
        public Nullable<bool> IsDraft { get; set; }
        public Nullable<bool> IsRejected { get; set; }
        public Nullable<int> RaisedBy { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public string Comment { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsHold { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public bool IsApproved { get; set; }
        public Nullable<int> StageId { get; set; }
        public Nullable<System.DateTime> PrimaryApprovalOn { get; set; }
        public Nullable<int> PrimaryApprover { get; set; }
        public string InvoiceFormCode { get; set; }
        public Nullable<System.DateTime> Details { get; set; }
        public Nullable<int> Project { get; set; }
        public Nullable<decimal> TotalDiscount { get; set; }
        public Nullable<decimal> NetAmount { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PMSInvoiceDetails> PMSInvoiceDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PMSInvoicePayments> PMSInvoicePayments { get; set; }
    }
}

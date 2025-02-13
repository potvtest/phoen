using System;
using Pheonix.DBContext;
using System.Collections.Generic;
using Pheonix.Models.VM;

namespace Pheonix.Models
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public Nullable<int> Project { get; set; }
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
        public List<InvoiceDetailsViewModel> InvoiceDetailsModel { get; set; }
        public string ContactPersonName { get; set; }
        public string ContractDet { get; set; }
        public string CustomerAddressDet { get; set; }
        public string irSalesPeriod { get; set; }
        public EmployeeBasicProfile employeeProfile { get; set; }
        public string Comment { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsHold { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public bool IsApproved { get; set; }
        public string FinanceApprover { get; set; }
        public string InvoiceFormCode { get; set; }
        public Nullable<System.DateTime> Details { get; set; }
        public int StageId { get; set; }
        public Nullable<System.DateTime> PrimaryApprovalOn { get; set; }
        public Nullable<int> PrimaryApprover { get; set; }
        public string PrimaryApproverName { get; set; }
        public int totalStages { get; set; }
        public IEnumerable<StageStatus> invoiceStatus { get; set; }
        public string irNumber { get; set; }
        public decimal totalAmtRecieved { get; set; }
        public decimal totalBalAmt { get; set; }
        public Nullable<decimal> TotalDiscount { get; set; }
        public Nullable<decimal> NetAmount { get; set; }
    }


    public class InvoiceDetailsViewModel
    {
        public int Id { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> Rate { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string BillableRsrc { get; set; }
        public string Description { get; set; }
        public Nullable<int> InvoiceID { get; set; }
        public List<int> SOW_Reference { get; set; }
        public string soW_Referencevalue { get; set; }
        public string AttachedFile { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> NetAmount { get; set; }
    }


    public class InvoicePaymentsViewModel
    {
        public int Id { get; set; }
        public Nullable<int> InvoiceId { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public Nullable<decimal> PaymentRecieved { get; set; }
        public Nullable<decimal> BalAmt { get; set; }
    }
}


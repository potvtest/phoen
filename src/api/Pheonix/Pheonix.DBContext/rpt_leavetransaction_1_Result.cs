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
    
    public partial class rpt_leavetransaction_1_Result
    {
        public int ID { get; set; }
        public Nullable<int> PersonID { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public Nullable<int> Leaves { get; set; }
        public Nullable<int> NotConsumed { get; set; }
        public Nullable<int> LWP { get; set; }
        public string Narration { get; set; }
        public Nullable<int> LeaveType { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> ApproverID { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}

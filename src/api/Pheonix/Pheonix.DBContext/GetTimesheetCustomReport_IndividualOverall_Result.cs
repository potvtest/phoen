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
    
    public partial class GetTimesheetCustomReport_IndividualOverall_Result
    {
        public Nullable<int> DUId { get; set; }
        public string DUName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Nullable<decimal> EmployeeBillableHours { get; set; }
        public Nullable<decimal> EmployeeNonBillableHours { get; set; }
        public int OrgReportingManager { get; set; }
        public Nullable<int> RMGManager { get; set; }
        public string OrgReportingManagerName { get; set; }
        public string RMGManagerName { get; set; }
    }
}

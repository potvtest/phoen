using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class EmployeeApprovalViewModel : IEmployeeApprovalViewModel
    {
        public string OldModel { get; set; }
        public string NewModel { get; set; }
        public string StageID { get; set; }
        public int ModuleID { get; set; }
        public string ModuleCode { get; set; }
        public int By { get; set; }
        public int ApprovalStatus { get; set; }
        public string Comments { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public int RecordID { get; set; }
        public List<ApprovalHistory> approvalHistory { get; set; }
    }
}

public class ApprovalHistory
{
    public int action { get; set; }
    public string comments { get; set; }
    public string dateTime { get; set; }
}
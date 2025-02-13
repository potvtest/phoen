using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public interface IEmployeeApprovalViewModel
    {
        string OldModel { get; set; }
        string NewModel { get; set; }
        string StageID { get; set; }
        int ModuleID { get; set; }
        string ModuleCode { get; set; }
        int By { get; set; }
        int ApprovalStatus { get; set; }
        string Comments { get; set; }
        int ApprovedBy { get; set; }
        DateTime ApprovedDate { get; set; }
        int RecordID { get; set; }
        List<ApprovalHistory> approvalHistory { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class NewExpenseViewModel
    {
        public int expenseId { get; set; }

        public DateTime RequestDate { get { return DateTime.Now; } set { value = DateTime.Now; } }
        public int IsClientReimbursment { get; set; }
        public int PrimaryApproverId { get; set; }
        public int SecondaryApproverId { get; set; }
        public int CostCenterId { get; set; }
        public int CurrencyId { get; set; }
        public bool IsDraft { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        public int CreatedBy { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsRejected { get; set; }
        public decimal advance { get; set; }
        public decimal amountReimbursed { get; set; }
        public decimal totalExpenses { get; set; }
        public string formCode { get; set; }
        public string chequeDetails { get; set; }
        public int stageId { get; set; }
        public int totalStages { get; set; }
        public string comments { get; set; }
        public DateTime CreatedDate { get; set; }
        public string officeLocation { get; set; }
        public string primaryApproverName { get; set; }
        public DateTime? expenseApprovedDate { get; set; }
        public string financeApproverName { get; set; }
        public DateTime? financeApprovedDate { get; set; }
        public List<NewEmployeeExpenseDetails> details { get; set; }
        public IEnumerable<StageStatus> expenseStatus { get; set; }
        public EmployeeBasicProfile employeeProfile { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public int financeApproverId { get; set; }
        public bool IsHold { get; set; }
    }
}

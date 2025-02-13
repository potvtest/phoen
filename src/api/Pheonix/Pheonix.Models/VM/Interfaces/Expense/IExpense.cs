using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.Expense
{
    interface IExpense
    {
        int reimFormCode { get; set; }
        string requestDate { get; set; }
        bool isClientReimbursment { get; set; }
        string clientId { get; set; }
        string primaryApproverUserId { get; set; }
        string secondaryApproverUserId { get; set; }
        string costCenterId { get; set; }
        string currencyId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.Expense
{
    interface IExpenseDetail
    {
        int ExpenseDetailId { get; set; }
        string ReceiptNo { get; set; }
        string ExpenseDate { get; set; }
        int ExpenseCategoryId { get; set; }
        decimal Amount { get; set; }
        string Comments { get; set; }
        string AttachedFile { get; set; }
    }
}

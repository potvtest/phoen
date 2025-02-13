using Pheonix.Models.VM.Interfaces.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeExpenseDetails : IExpenseDetail
    {
        public int ExpenseDetailId
        {
            get;
            set;
        }

        public string ReceiptNo
        {
            get;
            set;
        }

        public string ExpenseDate
        {
            get;
            set;
        }

        public int ExpenseCategoryId
        {
            get;
            set;
        }

        public decimal Amount
        {
            get;
            set;
        }

        public string Comments
        {
            get;
            set;
        }

        public string AttachedFile
        {
            get;
            set;
        }

        public decimal advance { get; set; }

        public decimal balance { get; set; }

        [System.ComponentModel.DefaultValue(true)]
        public bool isDetailsValid { get; set; }
    }
}

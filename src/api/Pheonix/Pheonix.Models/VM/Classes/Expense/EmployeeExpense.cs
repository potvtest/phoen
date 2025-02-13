using Pheonix.Models.VM.Interfaces.Expense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Expense
{
    public class EmployeeExpense : IExpense
    {
        public int reimFormCode
        {
            get;
            set;
        }

        public string requestDate
        {
            get;

            set;

        }

        public bool isClientReimbursment
        {
            get;
            set;

        }

        public string clientId
        {
            get;
            set;
        }

        public string primaryApproverUserId
        {
            get;
            set;
        }

        public string secondaryApproverUserId
        {
            get;
            set;
        }

        public string costCenterId
        {
            get;
            set;
        }

        public string currencyId
        {
            get;
            set;
        }
    }
}

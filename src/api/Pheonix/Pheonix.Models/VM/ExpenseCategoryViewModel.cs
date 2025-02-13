using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class ExpenseCategoryViewModel
    {
        ExpenseCategoryViewModel()
        {
            this.CreatedOn = DateTime.Now;
        }

        public int ExpenseCategoryId { get; set; }
        public string Title { get; set; }
        public int LocationID { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public string Description { get; set; }
    }

    public class ExpenseMail
    {
        public int moduleType { get; set; }
        public string fromMail { get; set; }
        public string toMail { get; set; }
        public string ccMail { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public string expenseID { get; set; }
        public string userName { get; set; }
        public string empId { get; set; }
        public string loggedInuser { get; set; }

    }
}

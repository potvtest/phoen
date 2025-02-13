using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.Models.Confirmation
{
    public class Confirmations
    {
        public Confirmations()
        {
            InitiatedDate = DateTime.Today;
            ReviewDate = DateTime.Today;
            ReportingManagerId = 0;
            ReportingManager = "";
            Feedback = new ConfirmationFeedback();
        }

        public int ID { get; set; }

        public Nullable<System.DateTime> JoiningDate { get; set; }
        public Nullable<System.DateTime> ProbationReviewDate { get; set; }
        public Nullable<System.DateTime> ReviewDate { get; set; }
        public Nullable<System.DateTime> InitiatedDate { get; set; }
        public Nullable<int> ReportingManagerId { get; set; }
        public string ReportingManager { get; set; }

        public EmployeeBasicProfile Employee { get; set; }

        public ConfirmationFeedback Feedback { get; set; }

        public int EditStyle { get; set; }

        public int PersonId { get; set; }
    }
}

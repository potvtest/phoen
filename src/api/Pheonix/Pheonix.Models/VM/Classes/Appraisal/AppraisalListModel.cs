using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Appraisal
{
    public class AppraisalListModel
    {
        public int ID { get; set; }
        public string Reviewer { get; set; }
        public string Appraiser { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> Grade { get; set; }
        public string AssignedTo { get; set; }
        public Nullable<int> AppraiserScore { get; set; }
        public Nullable<int> ReviewerScore { get; set; }
        public Nullable<int> FinalReviewerRating { get; set; }
        public EmployeeBasicProfile EmployeeProfile { get; set; }
    }
}

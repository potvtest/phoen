using System;

namespace Pheonix.Models.ViewModels
{
    public class EmployeeJoiningDetailViewModel
    {
        public int ID { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime ExitDate { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public DateTime ProbationReviewDate { get; set; }
        public bool RejoinedWithinYear { get; set; }
    }
}
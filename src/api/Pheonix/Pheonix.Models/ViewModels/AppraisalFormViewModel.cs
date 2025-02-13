using Pheonix.DBContext;
using Pheonix.Models.VM.Classes.Appraisal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class AppraisalFormViewModel
    {
        public List<AppraiseeFormModel> AppraiseForm { get; set; }
        public List<AppraiseeParametersViewModel> ReviewerParameters { get; set; }
        public List<AppraiseeParametersViewModel> AppraiserParameters { get; set; }
        public int Status { get; set; }
        public string AppraiserComments { get; set; }
        public string ReviewerComments { get; set; }
        public string OneToOneComments { get; set; }
        public string OneToOneImprovementComment { get; set; }
        public Nullable<int> FinalReviewerRating { get; set; }
        public Nullable<int> ReviewerRating { get; set; }
        public Nullable<int> AppraiserRating { get; set; }
        public string PromotionFor { get; set; }
        public Nullable<bool> IsPromotion { get; set; }
        public Nullable<bool> IsPromotionByRiviwer { get; set; }
        public string PromotionForByRiviwer { get; set; }
        public Nullable<bool> IsTrainingRequired { get; set; }
        public string TrainingFor { get; set; }
        public Nullable<bool> IsCriticalForOrganize { get; set; }
        public string CriticalForOrganizeFor { get; set; }
        public Nullable<bool> IsCriticalForProject { get; set; }
        public string CriticalForProject { get; set; }
        public int OrgCategoryId { get; set; }
        public string OrgCategoryName { get; set; }
        public string OrgCategoryDescription { get; set; }
    }

    
}

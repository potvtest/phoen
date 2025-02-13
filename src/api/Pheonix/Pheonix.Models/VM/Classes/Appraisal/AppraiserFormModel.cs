using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Appraisal
{
    public class AppraiserFormModel
    {
        public List<AppraiseeParametersViewModel> Parameters { get; set; }
        public int AppraiseeId { get; set; }
        public string Comments { get; set; }
        public string ImprovementComments { get; set; }
        public string PromotionFor { get; set; }
        public bool IsPromotion { get; set; }
        public string TrainingFor { get; set; }
        public bool IsTrainingRequired { get; set; }
        public bool IsPromotionByRiviwer { get; set; }
        public string PromotionForByRiviwer { get; set; }
        public Nullable<bool> IsCriticalForOrganize { get; set; }
        public string CriticalForOrganizeFor { get; set; }
        public Nullable<bool> IsCriticalForProject { get; set; }
        public string CriticalForProject { get; set; }
        public int OrgCategoryId { get; set; }
        public string OrgCategoryName { get; set; }
        public string OrgCategoryDescription { get; set; }
    }
}

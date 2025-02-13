using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class AppraisalReportViewModel
    {
        public Nullable<int> Status { get; set; }
        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public Nullable<int> Year { get; set; }
        public string AppraiserComments { get; set; }
        public string ReviewerComments { get; set; }
        public string OneToOneComments { get; set; }
        public string OneToOneImprovementComment { get; set; }
        public Nullable<int> AppraiserRating { get; set; }
        public Nullable<int> ReviewerRating { get; set; }
        public Nullable<int> OneToOneRating { get; set; }
        public Nullable<int> Grade { get; set; }
        public Nullable<bool> IsPromotion { get; set; }
        public string AppraiserName { get; set; }
        public string ReviewerName { get; set; }
        public string Location { get; set; }
        public Nullable<decimal> AppraiserRatingBySystem { get; set; }
        public Nullable<decimal> ReviewerRatingBySystem { get; set; }
        public Nullable<bool> IsPromotionByRiviwer { get; set; }
        public string PromotionForByRiviwer { get; set; }
        public string PromotionFor { get; set; }
        public Nullable<bool> IsTrainingRequired { get; set; }
        public string TrainingFor { get; set; }
        public Nullable<int> DeliveyTeam { get; set; }
        public string ProjectName { get; set; }
        public Nullable<bool> IsCriticalForOrganize { get; set; }
        public string CriticalForOrganizeFor { get; set; }
        public Nullable<bool> IsCriticalForProject { get; set; }
        public string CriticalForProject { get; set; }
        public Nullable<bool> IsPromotionNorm { get; set; }
        public string PromotionforByNorm { get; set; }
        public string OrganizationCategory { get; set; }
        public string OrganizationComment { get; set; }
        public Nullable<int> LocationId { get; set; }
    }
}
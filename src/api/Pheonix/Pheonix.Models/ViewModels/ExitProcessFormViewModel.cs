using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class ExitProcessFormViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public int FeedbackForLeavingOrg { get; set; }
        public string FeedbackForLeavingOrgRemark { get; set; }
        public int RatingForReportingLead { get; set; }
        public string RatingForReportingLeadRemark { get; set; }
        public int RatingForOrganization { get; set; }
        public string RatingForOrganizationRemark { get; set; }
        public int OrganizationDevelopmentSuggestion { get; set; }
        public string OrgDevelopmentSuggestionRemark { get; set; }
        public int EmployeeDeclaration { get; set; }
        public string EmployeeDeclarationRemark { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public Boolean IsHRReviewDone { get; set; }
    }

    public class FeedbackForLeavingOrgViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public string FutherStudies { get; set; }
        public string FutherStudiesComment { get; set; }
        public string HealthFactor { get; set; }
        public string HealthFactorComment { get; set; }
        public string MarriageFamily { get; set; }
        public string MarriageFamilyComment { get; set; }
        public string OpportunitySalary { get; set; }
        public string OpportunitySalaryComment { get; set; }
        public string Overseas { get; set; }
        public string OverseasComment { get; set; }
        public string JobExpectation { get; set; }
        public string JobExpectationComment { get; set; }
        public string Work { get; set; }
        public string WorkComment { get; set; }
        public string WorkCulture { get; set; }
        public string WorkCultureComment { get; set; }
        public string WorkHours { get; set; }
        public string WorkHoursComment { get; set; }
        public string Responsibilities { get; set; }
        public string ResponsibilitiesComment { get; set; }
        public string TeamIssues { get; set; }
        public string TeamIssuesComment { get; set; }
        public string Recognition { get; set; }
        public string RecognitionComment { get; set; }
        public string ReportingHeadIssues { get; set; }
        public string ReportingHeadIssuesComment { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class RatingForReportingLeadViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public string CommunicationExpectation { get; set; }
        public string CommunicationExpectationComment { get; set; }
        public string ResolvedComplaints { get; set; }
        public string ResolvedComplaintsComment { get; set; }
        public string Teamwork { get; set; }
        public string TeamworkComment { get; set; }
        public string Approachable { get; set; }
        public string ApproachableComment { get; set; }
        public string Receptive { get; set; }
        public string ReceptiveComment { get; set; }
        public string TechnicalGuidance { get; set; }
        public string TechnicalGuidanceComment { get; set; }
        public string DevelopedYou { get; set; }
        public string DevelopedYouComment { get; set; }
        public string PerformanceFeedback { get; set; }
        public string PerformanceFeedbackComment { get; set; }
        public string Accomplishments { get; set; }
        public string AccomplishmentsComment { get; set; }
        public string Assignments { get; set; }
        public string AssignmentsComment { get; set; }
        public string EffectiveLeader { get; set; }
        public string EffectiveLeaderComment { get; set; }
        public string SensitiveToEmployee { get; set; }
        public string SensitiveToEmployeeComment { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class RatingForOrganizationViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public string BenefitsAndPay { get; set; }
        public string BenefitsAndPayComment { get; set; }
        public string TrainingSessions { get; set; }
        public string TrainingSessionsComment { get; set; }
        public string OrgReviewSystem { get; set; }
        public string OrgReviewSystemComment { get; set; }
        public string OpenDoorPolicy { get; set; }
        public string OpenDoorPolicyComment { get; set; }
        public string JobPayment { get; set; }
        public string JobPaymentComment { get; set; }
        public string CareerOpportunity { get; set; }
        public string CareerOpportunityComment { get; set; }
        public string WorkingConditions { get; set; }
        public string WorkingConditionsComment { get; set; }
        public string OrganizationGrowth { get; set; }
        public string OrganizationGrowthComment { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    
    public class OrgDevelopmentSuggestionViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public int Question1 { get; set; }
        public string Question1Comment { get; set; }
        public int Question2 { get; set; }
        public string Question2Comment { get; set; }
        public int Question3 { get; set; }
        public string Question3Comment { get; set; }
        public int Question4 { get; set; }
        public string Question4Comment { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class ExitFormEmployeeDeclarationViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public int Question1 { get; set; }
        public string Question1Comment { get; set; }
        public int Question2 { get; set; }
        public string Question2Comment { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class ExitProcessFormDetailViewModel
    {
        public int ID { get; set; }
        public int SeperationID { get; set; }
        public ExitProcessFormViewModel ExitProcessFormLinking { get; set; }
        public FeedbackForLeavingOrgViewModel LeavingOrgData { get; set; }
        public RatingForReportingLeadViewModel ReportingLeadData { get; set; }
        public RatingForOrganizationViewModel OrgRatingData { get; set; }
        public OrgDevelopmentSuggestionViewModel SuggestionData { get; set; }
        public ExitFormEmployeeDeclarationViewModel EmployeeDeclarationData { get; set; }
    }
}

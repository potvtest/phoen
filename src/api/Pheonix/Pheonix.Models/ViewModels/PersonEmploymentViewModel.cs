using System;

namespace Pheonix.Models
{
    public class PersonEmploymentViewModel
    {
        public int ID { get; set; }

        public int? PersonID { get; set; }

        public DateTime? JoiningDate { get; set; }

        public int? DesignationID { get; set; }

        public int? ProbationMonths { get; set; }

        public DateTime? ProbationReviewDate { get; set; }

        public DateTime? ConfirmationDate { get; set; }

        public DateTime? SeparationRequestDate { get; set; }

        public DateTime? ExitDate { get; set; }

        public bool? RejoinedWithinYear { get; set; }

        public string EmployeeType { get; set; }

        public int? EmploymentStatus { get; set; }

        public string UserName { get; set; }

        public string BusinessGroup { get; set; }

        public string OrgUnit { get; set; }

        public int? CurrentDU { get; set; }

        public int? DeliveryTeam { get; set; }

        public string ResourcePool { get; set; }

        public string Commitment { get; set; }

        public string OfficeExtension { get; set; }

        public string SeatingLocation { get; set; }

        public string OrganizationEmail { get; set; }
        public string SkillDescription { get; set; }
        public int CompetencyID { get; set; }
    }
}
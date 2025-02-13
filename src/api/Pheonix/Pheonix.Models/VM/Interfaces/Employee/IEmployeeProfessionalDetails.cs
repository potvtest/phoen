using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public interface IEmployeeProfessionalDetails
    {
        DateTime? JoiningDate { get; set; }
        DateTime? ConfirmationDate { get; set; }
        DateTime? ProbationReviewDate { get; set; }
        DateTime? ExitDate { get; set; }
        bool RejoinedWithinYear { get; set; }
        string ReportingTo { get; set; }
        string CompetencyManager { get; set; }
        string ExitProcessManager { get; set; }
        string OrganizationEmail { get; set; }
        int? CompetencyID { get; set; }
        string SkillDescription { get; set; }

        IEmployeeManagerViewModel R1 { get; set; }
        IEmployeeManagerViewModel R2 { get; set; }

        IEnumerable<IEmployeeDeclaration> Declarations { get; set; }
        IEnumerable<IEmployeeSkill> Skills { get; set; }

        IEnumerable<IEmployeeSkill> PrimarySkills { get; set; }
        IEnumerable<IEmployeeSkill> SecondarySkills { get; set; }
        IEnumerable<IEmployeeRole> Role { get; set; }
    }
}
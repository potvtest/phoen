using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class EmployeeProfessionalDetails : IEmployeeProfessionalDetails
    {
        public DateTime? JoiningDate
        {
            get;
            set;
        }

        public DateTime? ConfirmationDate
        {
            get;
            set;
        }

        public DateTime? ProbationReviewDate
        {
            get;
            set;
        }

        public DateTime? ExitDate
        {
            get;
            set;
        }

        public bool RejoinedWithinYear
        {
            get;
            set;
        }

        public string ReportingTo
        {
            get;
            set;
        }

        public string CompetencyManager
        {
            get;
            set;
        }

        public string ExitProcessManager
        {
            get;
            set;
        }

        public string OrganizationEmail
        {
            get;
            set;
        }

        public int OL
        {
            get;
            set;
        }

        public string OLText
        {
            get;
            set;
        }

        public int? CompetencyID
        {
            get;
            set;
        }

        public string SkillDescription
        {
            get;
            set;
        }

        public IEmployeeManagerViewModel R1
        {
            get;
            set;
        }

        public IEmployeeManagerViewModel R2
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeSkill> Skills { get; set; }

        public IEnumerable<IEmployeeSkill> PrimarySkills { get; set; }

        public IEnumerable<IEmployeeSkill> SecondarySkills { get; set; }

        public IEnumerable<IEmployeeDeclaration> Declarations { get; set; }


        public IEnumerable<IEmployeeRole> Role
        {
            get;
            set;
        }
    }
}
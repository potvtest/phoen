using System;

namespace Pheonix.Models
{
    public class PersonEmploymentHistoryViewModel
    {
        public int ID { get; set; }

        public string OrganisationName { get; set; }

        public string Location { get; set; }

        public DateTime? JoiningDate { get; set; }

        public DateTime? WorkedTill { get; set; }

        public string EmploymentType { get; set; }

        public string LastDesignation { get; set; }

        public string RoleDescription { get; set; }

        public int? PersonID { get; set; }
    }
}
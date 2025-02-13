using System;

namespace Pheonix.Web.Models
{
    public class PersonEmploymentHistory
    {
        public int ID { get; set; }

        public string OrganisationName { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string LastDesignation { get; set; }

        public string RoleDescription { get; set; }

        public int PersonID { get; set; }
    }
}
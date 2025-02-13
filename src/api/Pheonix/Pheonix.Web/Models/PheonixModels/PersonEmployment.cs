using System;

namespace Pheonix.Web.Models
{
    public class PersonEmployment
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        public DateTime JoiningDate { get; set; }

        public int DesignationID { get; set; }

        public DateTime SeparationDate { get; set; }

        public string OfficialEmail { get; set; }

        public string PersonalEmail { get; set; }
    }
}
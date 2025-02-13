using System;

namespace Pheonix.Web.Models
{
    public class PersonPersonal
    {
        public int PersonID { get; set; }

        public string BloodGroup { get; set; }

        public bool IsMarried { get; set; }

        public string SpouseName { get; set; }

        public DateTime DateOfMarriage { get; set; }

        public string FatherName { get; set; }

        public string MotherName { get; set; }
    }
}
using System;

namespace Pheonix.Models.VM
{
    public class EmployeeVisa : IEmployeeVisa
    {
        public int VisaTypeID { get; set; }
        public string VisaName { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public DateTime? ValidTill { get; set; }
        public int ID { get; set; }
        public bool IsDeleted { get; set; }
        public int StageStatusID { get; set; }
        public string visaFileUrl { get; set; }
        public int SearchUserID { get; set; }
    }
}
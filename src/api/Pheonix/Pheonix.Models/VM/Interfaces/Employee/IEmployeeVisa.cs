using System;

namespace Pheonix.Models.VM
{
    public interface IEmployeeVisa : IBaseModel
    {
        int VisaTypeID { get; set; }
        string VisaName { get; set; }
        int CountryID { get; set; }
        string CountryName { get; set; }
        DateTime? ValidTill { get; set; }
        bool IsDeleted { get; set; }
        string visaFileUrl { get; set; }
    }
}
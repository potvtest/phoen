using System;

namespace Pheonix.Models.VM
{
    public interface IEmployeeDependent : IBaseModel
    {
        string DependentName { get; set; }
        int RelationWithDependent { get; set; }
        DateTime? DateOfBirthOfDependent { get; set; }
        int Age { get; set; }
        bool IsDeleted { get; set; }
    }
}
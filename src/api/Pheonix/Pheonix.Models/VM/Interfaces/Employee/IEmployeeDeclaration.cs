using System;

namespace Pheonix.Models.VM
{
    public interface IEmployeeDeclaration : IBaseModel
    {
        string DeclaredPerson { get; set; }
        int V2PersonID { get; set; }
        string RelationType { get; set; }
        DateTime? BirthDate { get; set; }
        bool IsDeleted { get; set; }
    }
}
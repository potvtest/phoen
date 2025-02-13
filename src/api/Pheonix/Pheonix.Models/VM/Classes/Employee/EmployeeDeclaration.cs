using System;

namespace Pheonix.Models.VM
{
    public class EmployeeDeclaration : IEmployeeDeclaration
    {
        public string DeclaredPerson { get; set; }
        public int V2PersonID { get; set; }
        public string RelationType { get; set; }
        public int ID { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsDeleted { get; set; }
        public int StageStatusID { get; set; }
        public int SearchUserID { get; set; }
    }
}
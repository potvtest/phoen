using System.Collections.Generic;
namespace Pheonix.Models.ViewModels
{
    public class EmployeeProfileViewModel
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string Pan { get; set; }
        public string Passport { get; set; }
        public string PF { get; set; }
        public string Designation { get; set; }
        public string ImagePath { get { return "https://avatars0.githubusercontent.com/u/125464?v=3&s=96"; } }
        public PersonPersonalViewModel PersonPersonal { get; set; }
        public ICollection<PersonEmploymentViewModel> PersonEmployment { get; set; }
    }
}
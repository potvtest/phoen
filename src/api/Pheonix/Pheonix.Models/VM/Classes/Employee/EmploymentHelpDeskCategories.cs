using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Employee
{
    public class EmploymentHelpDeskCategories : IEmploymentHelpDeskCategories
    {
        //Id Is Prefix
       public string Id { get; set; }
        // Text Is Name
        public string Text { get; set; }
    }
}

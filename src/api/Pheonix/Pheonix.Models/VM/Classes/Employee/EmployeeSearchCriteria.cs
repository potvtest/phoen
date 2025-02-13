using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Employee
{
    public class EmployeeSearchCriteria
    {
        public string SearchQuery { get; set; }
        public bool ShowInActive { get; set; }
        public int EmpListFor { get; set; }
        public int MinRating { get; set; }
        public int MaxRating { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class CelebrationListViewModel 
    {
        //public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string SpouseName { get; set; }
        public DateTime CelebrationDate { get; set; }
        public string OrganizationEmail { get; set; }
        public string LocationName { get; set; }
        public string OnNoticePeriod { get; set; }
        public string Category { get; set; }
    }
}

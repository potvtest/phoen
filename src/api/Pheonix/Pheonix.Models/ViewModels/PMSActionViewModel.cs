using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class PMSActionViewModel
    {
        public int PMSActionID { get; set; }
        public string PMSAction1 { get; set; }
        public int PMSRoleMapID { get; set; }
        public bool IsChecked { get; set; }
        public int OrgRoleID { get; set; }
        public int PMSRoleID { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.DBContext
{
    public partial class ProjectList
    {
        public string CustomerName { get; set; }
        public string DelUnitName { get; set; }
        public string DelTeamName { get; set; }

        public DateTime? CustomerStartDate { get; set; }

        public DateTime CustomerEndDate { get; set; }

        public string ProjectManagerName { get; set; }
        public string DeliveryManagerName { get; set; }
    }
}

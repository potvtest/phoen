using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.HelpDesk
{
    public class HelpDeskListFilterObj
    {
        public int raisedByPersonID { get; set; }
        public int assignedToID { get; set; }
        public int statusID { get; set; }
        public int[] categoriesIdList { get; set; }
    }
}

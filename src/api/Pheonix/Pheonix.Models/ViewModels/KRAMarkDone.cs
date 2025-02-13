using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class KRAMarkDone
    {
        public int KRAId { get; set; }
        public int PersonId { get; set; }
        public DateTime KRADoneDate { get; set; }
        public int KRAGoalId { get; set; }
    }
}

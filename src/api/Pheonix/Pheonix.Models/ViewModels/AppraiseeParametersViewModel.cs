using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class AppraiseeParametersViewModel
    {
        public int ID { get; set; }
        public string Parameter { get; set; }
        public int Score { get; set; }
        public Nullable<int> Weightage { get; set; }
        public Nullable<bool> IsDelete { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<int> Levels { get; set; }
        public Nullable<bool> IsEditeble { get; set; }
    }

}

using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class NomineeDetailsVM: INomineeDetails
    {
        public string Name { get; set; }
        public Nullable<int> Relationship { get; set; }
        public string RelationshipValue { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
    }
}

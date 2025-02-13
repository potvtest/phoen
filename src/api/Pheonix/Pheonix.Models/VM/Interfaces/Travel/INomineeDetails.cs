using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.Travel
{
    public interface INomineeDetails
    {
        string Name { get; set; }
        Nullable<int> Relationship { get; set; }
        string RelationshipValue { get; set; }
        string ContactNumber { get; set; }
        string Address { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces
{
    public interface IAvailableLeaves
    {
        int? TotalLeaves { get; set; }
        int? LeavesTaken { get; set; }
    }
}

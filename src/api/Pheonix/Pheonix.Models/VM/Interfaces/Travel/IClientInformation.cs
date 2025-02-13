using Pheonix.Models.VM.Classes.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.Travel
{
    public interface IClientInformation
    {
        string purposeOfVisit { get; set; }
        string adddress { get; set; }
        string description { get; set; }
        int clientId { get; set; }

        ClientNameVM clientName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.Travel
{
    public interface ITravelDetails
    {
        int travelType { get; set; }
        string source { get; set; }
        string destination { get; set; }
        DateTime journeyDate { get; set; }
        DateTime returnJourneyDate { get; set; }
        Nullable<int> tripType { get; set; }
        Nullable<bool> accommodation { get; set; }
        DateTime checkInDate { get; set; }
        DateTime checkOutDate { get; set; }
        Nullable<int> reportingManagerId { get; set; }
        string reportingManagerName { get; set; }
        Nullable<int> exitManagerId { get; set; }
        string exitManagerName { get; set; }
        string projectName { get; set; }
        Nullable<int> requestType { get; set; }
    }

}

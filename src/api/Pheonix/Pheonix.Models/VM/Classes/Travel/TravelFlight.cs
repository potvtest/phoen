using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class TravelFlight
    {
        public int id { get; set; }
        public int travelId { get; set; }
        public int travelType { get; set; }
        public int tripType { get; set; }
        public string vendorName { get; set; }
        public DateTime? createdDate { get; set; }
        public string totalFlightCost { get; set; }
        public List<TravelFlightDetails> flightDetails { get; set; }
        public List<TravelInsuranceclass> travelInsurance { get; set; }
        public string comments { get; set; }
    }
}

public class TravelFlightDetails
{
    public int id { get; set; }
    public int flightId { get; set; }
    public string source { get; set; }
    public string destination { get; set; }
    public DateTime? journeyDate { get; set; }
    public DateTime? returnJourneyDate { get; set; }
}

public class TravelFlightClass
{
    public int id { get; set; }
    public string title { get; set; }
}

public class TravelInsuranceclass
{
    public int flightId { get; set; }
    public string vendorName { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public string totalInsuranceCost { get; set; }
}




using Pheonix.DBContext;
using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class TravelDetailsVM : ITravelDetails
    {
        public int travelType { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public DateTime journeyDate { get; set; }
        public DateTime returnJourneyDate { get; set; }
        public Nullable<int> tripType { get; set; }
        public Nullable<bool> accommodation { get; set; }
        public DateTime checkInDate { get; set; }
        public DateTime checkOutDate { get; set; }
        public Nullable<int> reportingManagerId { get; set; }
        public string reportingManagerName { get; set; }
        public Nullable<int> exitManagerId { get; set; }
        public string exitManagerName { get; set; }
        public string projectName { get; set; }
        public Nullable<int> requestType { get; set; }

        //public MealPreferenceVM mealPreference { get; set; }
        //public SeatLocationPreferenceVM seatLocationPreference { get; set; }
    }

}

public class MealPreferenceVM
{
    public int id { get; set; }
    public string description { get; set; }
}

public class SeatLocationPreferenceVM
{
    public int id { get; set; }
    public string description { get; set; }
}

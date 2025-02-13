using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class TravelServicesPreferences
    {
        public bool services { get; set; }
        public int serviceId { get; set; }
        public bool atmOnSite { get; set; }
        public bool laundry { get; set; }
        public bool shoppingCenter { get; set; }
        public bool pickUpDrop { get; set; }

    }
}

public class TravelInternetPreferences
{
    public bool internet { get; set; }
    public int internetId { get; set; }
    public bool freeWiFi { get; set; }
    public bool paidInternet { get; set; }
    public string amount { get; set; }
}

public class TravelGeneralPreferences
{
    public bool general { get; set; }
    public int generalId { get; set; }
    public bool iron { get; set; }
    public bool hairDryer { get; set; }
    public bool ironFacilites { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class TravelHotelBooking
    {
        public int hotelId { get; set; }
        public int travelId { get; set; }
        public DateTime checkIn { get; set; }
        public DateTime checkOut { get; set; }
        public int occupancyType { get; set; }
        public string hotelName { get; set; }
        public string vendorName { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string emailId { get; set; }
        public string comment { get; set; }
        public string totalSummaryCost { get; set; }

    }
}

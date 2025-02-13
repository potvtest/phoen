using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class TravelExtension
    {
        public int id { get; set; }
        public int travelId { get; set; }
        public DateTime departure { get; set; }
        public DateTime arrival { get; set; }
        public string comments { get; set; }
        public DateTime? visaDate { get; set; }
        public DateTime? I94Date { get; set; }
    }
}

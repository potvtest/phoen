using System;

namespace Pheonix.Web.Models
{
    public class SISO
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        public DateTime InTime { get; set; }

        public DateTime OutTime { get; set; }

        public int Mode { get; set; }

        public int Location { get; set; }
    }
}
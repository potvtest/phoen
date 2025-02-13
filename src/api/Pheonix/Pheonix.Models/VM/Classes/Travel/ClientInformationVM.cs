using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class ClientInformationVM : IClientInformation
    {
        public string purposeOfVisit { get; set; }
        public string adddress { get; set; }
        public int clientId { get; set; }
        public string description { get; set; }

        public ClientNameVM clientName { get; set; }
    }
}

using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class ClientNameVM : IClientName
    {
        public int clientId { get; set; }
        public string clientName { get; set; }
    }

}

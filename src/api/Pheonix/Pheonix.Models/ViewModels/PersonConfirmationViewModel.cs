using Pheonix.Models.Models.Confirmation;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class PersonConfirmationViewModel
    {
        public PersonConfirmationViewModel()
        {
            Confirmations = new List<Confirmations>();
        }

        public int EditStyle { get; set; }
        public List<Confirmations> Confirmations { get; set; }
    }
}

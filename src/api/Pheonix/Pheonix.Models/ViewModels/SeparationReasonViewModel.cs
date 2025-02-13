using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class SeparationReasonViewModel : IViewModel
    {
        public int ID { get; set; }
        public string ReasonDescription { get; set; }
        public int ReasonCode { get; set; }
        public bool IsActive { get; set; }
        public string ReasonCodeName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string ReasonCodeID { get; set; }
        
    }
}

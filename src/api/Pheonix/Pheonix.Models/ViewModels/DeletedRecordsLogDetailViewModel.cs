using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class DeletedRecordsLogDetailViewModel:IViewModel
    {
        public int ID { get; set; }
        public int? ModuleID { get; set; }
        public int? DeletedRecordID { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}

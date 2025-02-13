using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Pheonix.Models.ViewModels
{
    public class KRALogViewModel
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int? Table_PK_Id { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int? ModifiedById { get; set; }
        public string ModifiedByName { get; set; }
        public string ModifiedDate { get; set; }
    }
}
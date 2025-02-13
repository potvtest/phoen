using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public interface IBaseModel
    {
        int ID { get; set; }
        int StageStatusID { get; set; }
    }
}

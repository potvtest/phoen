using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.DBContext
{
    public partial class Person:IBaseModel
    {
        public int Key { get { return this.ID; } }
    }
}

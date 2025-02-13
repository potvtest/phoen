using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.DBContext
{
    public partial class PMSInvoiceDetails
    {
        public List<int> SOW_ReferenceValue
        {
            get
            {
                if (!string.IsNullOrEmpty(this.SOW_Reference))
                {
                    return SOW_Reference.Split(',').Select(s => Convert.ToInt32(s)).ToList();
                }
                return new List<int>();
            }
        }
    }
}

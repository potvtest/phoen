using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Services
{
    public interface ICustomerContactPersonService
    {
        IEnumerable<CustomerContactPersonViewModel> GetList(string filters);

        ActionResult Add(CustomerContactPersonViewModel model);

        ActionResult Update(CustomerContactPersonViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerContactPersonViewModel GetCustomer(int id);
    }
}

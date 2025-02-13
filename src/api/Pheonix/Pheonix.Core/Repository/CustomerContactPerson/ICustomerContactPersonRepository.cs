using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Repository
{
    public interface ICustomerContactPersonRepository
    {
        IEnumerable<CustomerContactPerson> GetList(string filters);

        ActionResult Add(CustomerContactPersonViewModel model);

        ActionResult Update(CustomerContactPersonViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerContactPerson GetCustomerContactPerson(int id);
    }
}

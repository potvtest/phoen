using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;

namespace Pheonix.Core.Repository
{
    public interface ICustomerAddressRepository
    {
        IEnumerable<CustomerAddress> GetList(string filters);

        ActionResult Add(CustomerAddressViewModel model);

        ActionResult Update(CustomerAddressViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerAddress GetCustomerAddress(int id);
    }
}

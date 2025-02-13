using Pheonix.Models;
using System.Collections.Generic;

namespace Pheonix.Core.Services
{
    public interface ICustomerAddressService
    {
        IEnumerable<CustomerAddressViewModel> GetList(string filters);

        ActionResult Add(CustomerAddressViewModel model);

        ActionResult Update(CustomerAddressViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerAddressViewModel GetCustomer(int id);
    }
}

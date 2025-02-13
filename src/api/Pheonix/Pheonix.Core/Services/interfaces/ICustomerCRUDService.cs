using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace Pheonix.Core.Services
{
    public interface ICustomerCRUDService<T>
    {
        ActionResult Add(T model);
        ActionResult Update(T model);
        ActionResult Delete(int id);
        T GetCustomer(int id);       
    }
}

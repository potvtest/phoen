using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace Pheonix.Core.Services
{
    public interface ICustomerService
    {
        IEnumerable<object> GetList(string query, bool showInActive);
        ActionResult Add(CustomerViewModel model,int userId);

        ActionResult Update(CustomerViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerViewModel GetCustomer(int id);

        HttpResponseMessage GetDownload(List<object> reportQueryParams);
    }
}

using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace Pheonix.Core.Repository
{
    public interface ICustomerRepository
    {
        IEnumerable<object> GetList(string query, bool showInActive);
        
        ActionResult Add(CustomerViewModel model,int userid);

        ActionResult Update(CustomerViewModel model);

        ActionResult Delete(int id, int personid);

        Customer GetCustomer(int id);
        List<int> getCustomerBGMapping(int id);

        HttpResponseMessage GetDownload(List<object> reportQueryParams);
    }
}

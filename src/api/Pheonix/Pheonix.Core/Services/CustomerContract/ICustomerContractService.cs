using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Services
{
    public interface ICustomerContractService
    {
        IEnumerable<CustomerContractViewModel> GetList(string filters);

        ActionResult Add(CustomerContractViewModel model);

        ActionResult Update(CustomerContractViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerContractViewModel GetCustomer(int id);

        ActionResult Add(ContractAttachmentViewModel model, int personID);
    }
}

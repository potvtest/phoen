using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Repository
{
    public interface ICustomerContractRepository
    {
        IEnumerable<CustomerContractViewModel> GetList(string filters);

        ActionResult Add(CustomerContractViewModel model);

        ActionResult Update(CustomerContractViewModel model);

        ActionResult Delete(int id, int personid);

        CustomerContract GetCustomerContract(int id);

        ActionResult Add(ContractAttachmentViewModel model, int personID);
    }
}

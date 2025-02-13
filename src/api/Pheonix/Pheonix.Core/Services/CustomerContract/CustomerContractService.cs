using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Services
{
    public class CustomerContractService : ICustomerContractService
    {
        private ICustomerContractRepository _customerContractRepository;

        public CustomerContractService(ICustomerContractRepository customerContractRepository)
        {
            _customerContractRepository = customerContractRepository;
        }

        public IEnumerable<CustomerContractViewModel> GetList(string filters)
        {
            var customerContractList = _customerContractRepository.GetList(filters);
            return customerContractList;
        }

        public ActionResult Add(CustomerContractViewModel model)
        {
            var statusCode = _customerContractRepository.Add(model);
            return statusCode;
        }

        public ActionResult Update(CustomerContractViewModel model)
        {
            var statusCode = _customerContractRepository.Update(model);
            return statusCode;
        }

        public ActionResult Delete(int id, int personid)
        {
            return _customerContractRepository.Delete(id, personid);
        }

        public CustomerContractViewModel GetCustomer(int id)
        {
            var customerContractList = Mapper.Map<CustomerContract, CustomerContractViewModel>(_customerContractRepository.GetCustomerContract(id));
            return customerContractList;
        }

        public ActionResult Add(ContractAttachmentViewModel model, int personID)
        {
            var stausCode = _customerContractRepository.Add(model, personID);
            return stausCode;
        }
    }
}

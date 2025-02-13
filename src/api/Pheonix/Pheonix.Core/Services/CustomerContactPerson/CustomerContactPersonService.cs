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
    public class CustomerContactPersonService : ICustomerContactPersonService
    {
        private ICustomerContactPersonRepository _customerContactPersonRepository;

        public CustomerContactPersonService(ICustomerContactPersonRepository customerContactPersonRepository)
        {
            _customerContactPersonRepository = customerContactPersonRepository;
        }

        public IEnumerable<CustomerContactPersonViewModel> GetList(string filters)
        {
            var customerAddressList = Mapper.Map<IEnumerable<CustomerContactPerson>, IEnumerable<CustomerContactPersonViewModel>>(_customerContactPersonRepository.GetList(filters));
            return customerAddressList;
        }

        public ActionResult Add(CustomerContactPersonViewModel model)
        {
            var statusCode = _customerContactPersonRepository.Add(model);
            return statusCode;
        }

        public ActionResult Update(CustomerContactPersonViewModel model)
        {
            var statusCode = _customerContactPersonRepository.Update(model);
            return statusCode;
        }

        public ActionResult Delete(int id, int personid)
        {
            return _customerContactPersonRepository.Delete(id, personid);
        }

        public CustomerContactPersonViewModel GetCustomer(int id)
        {
            var customerAddressList = Mapper.Map<CustomerContactPerson, CustomerContactPersonViewModel>(_customerContactPersonRepository.GetCustomerContactPerson(id));
            return customerAddressList;
        }
    }
}

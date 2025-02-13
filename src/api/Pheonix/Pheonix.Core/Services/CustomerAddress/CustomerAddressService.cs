using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;

namespace Pheonix.Core.Services
{
    public class CustomerAddressService : ICustomerAddressService
    {
        private ICustomerAddressRepository _customerAddressRepository;

        public CustomerAddressService(ICustomerAddressRepository customerAddressRepository)
        {
            _customerAddressRepository = customerAddressRepository;
        }

        public IEnumerable<CustomerAddressViewModel> GetList(string filters)
        {
            var customerAddressList = Mapper.Map<IEnumerable<CustomerAddress>, IEnumerable<CustomerAddressViewModel>>(_customerAddressRepository.GetList(filters));
            return customerAddressList;
        }

        public ActionResult Add(CustomerAddressViewModel model)
        {
            var statusCode = _customerAddressRepository.Add(model);
            return statusCode;
        }

        public ActionResult Update(CustomerAddressViewModel model)
        {
            var statusCode = _customerAddressRepository.Update(model);
            return statusCode;
        }

        public ActionResult Delete(int id, int personid)
        {
            return _customerAddressRepository.Delete(id, personid);
        }

        public CustomerAddressViewModel GetCustomer(int id)
        {
            var customerAddressList = Mapper.Map<CustomerAddress, CustomerAddressViewModel>(_customerAddressRepository.GetCustomerAddress(id));
            return customerAddressList;
        }
    }
}

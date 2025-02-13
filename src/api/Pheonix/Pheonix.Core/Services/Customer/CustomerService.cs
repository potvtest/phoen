using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace Pheonix.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public IEnumerable<object> GetList(string query, bool showInActive)
        {           
            return _customerRepository.GetList(query, showInActive);
        }

        public ActionResult Add(CustomerViewModel model,int userId)
        {
            var statusCode = _customerRepository.Add(model,userId);
            return statusCode;
        }

        public ActionResult Update(CustomerViewModel model)
        {
            var statusCode = _customerRepository.Update(model);
            return statusCode;
        }

        public ActionResult Delete(int id, int personid)
        {
            return _customerRepository.Delete(id, personid);
        }

        public CustomerViewModel GetCustomer(int id)
        {
            var customerList = Mapper.Map<Customer, CustomerViewModel>(_customerRepository.GetCustomer(id));
            List<int> lstBgcParameter = _customerRepository.getCustomerBGMapping(id);
            customerList.BgcParameter = lstBgcParameter;
            return customerList;
        }

        public HttpResponseMessage GetDownload(List<object> reportQueryParams)
        {
            return _customerRepository.GetDownload(reportQueryParams);
        }
    }
}

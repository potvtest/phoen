using Pheonix.Core.Services;
using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using Pheonix.Web.Extensions;
using Pheonix.Web.Authorization;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/customeraddress"), Authorize]
    public class CustomerAddressController : ApiController, ICustomerCRUDService<CustomerAddressViewModel>
    {
        private ICustomerAddressService _customerAddressService;

        public CustomerAddressController(ICustomerAddressService customerAddressService)
        {
            _customerAddressService = customerAddressService;
        }

        [Route("list")]
        public IEnumerable<CustomerAddressViewModel> GetList(string filters = null)
        {
            var customerAddressList = _customerAddressService.GetList(filters);

            return customerAddressList;
        }

        [Route("add"), HttpPost]
        public ActionResult Add(CustomerAddressViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerAddressService.Add(model);
        }

        [Route("update"), HttpPost]
        public ActionResult Update(CustomerAddressViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerAddressService.Update(model);
        }

        [Route("delete/{id:int}"), HttpPost]
        public ActionResult Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");
            var personid = Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            return _customerAddressService.Delete(id, personid);
        }

        [Route("getcustomer/{id:int}"), HttpPost]
        public CustomerAddressViewModel GetCustomer(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            return _customerAddressService.GetCustomer(id);
        }
    }
}
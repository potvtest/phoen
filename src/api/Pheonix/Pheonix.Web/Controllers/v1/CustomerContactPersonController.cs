using Pheonix.Core.Services;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Security.Claims;
using Pheonix.Web.Extensions;
using System;
using Pheonix.Web.Authorization;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/customercontactperson"), Authorize]
    public class CustomerContactPersonController : ApiController, ICustomerCRUDService<CustomerContactPersonViewModel>
    {
        private ICustomerContactPersonService _customerContactPersonService;

        public CustomerContactPersonController(ICustomerContactPersonService customerContactPersonService)
        {
            _customerContactPersonService = customerContactPersonService;
        }

        [Route("list")]
        public IEnumerable<CustomerContactPersonViewModel> GetList(string filters = null)
        {
            var customerContactPersonList = _customerContactPersonService.GetList(filters);

            return customerContactPersonList;
        }

        [Route("add"), HttpPost]
        public ActionResult Add(CustomerContactPersonViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerContactPersonService.Add(model);
        }

        [Route("update"), HttpPost]
        public ActionResult Update(CustomerContactPersonViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerContactPersonService.Update(model);
        }

        [Route("delete/{id:int}"), HttpPost]

        public ActionResult Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");
            var personid = Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            return _customerContactPersonService.Delete(id, personid);
        }

        [Route("getcustomer/{id:int}"), HttpPost]
        public CustomerContactPersonViewModel GetCustomer(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            return _customerContactPersonService.GetCustomer(id);
        }
    }
}
using Pheonix.Core.Services;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using Pheonix.Web.Extensions;
using Pheonix.Web.Authorization;
using System.Net.Http;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/customer"), Authorize]
    public class CustomerController : ApiController, ICustomerCRUDService<CustomerViewModel>
    {
        private ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [Route("list/{*query}"), HttpGet]
        public IEnumerable<object> GetList(string query, bool showInActive = false)
        {
            return _customerService.GetList(query, showInActive);
        }

        [Route("add"), HttpPost]
        public ActionResult Add(CustomerViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerService.Add(model, Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("Update"), HttpPost]
        public ActionResult Update(CustomerViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerService.Update(model);
        }

        [Route("delete/{id:int}"), HttpPost]
        public ActionResult Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");
            var personid = Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            return _customerService.Delete(id, personid);
        }

        [Route("getcustomer/{id:int}"), HttpGet]
        public CustomerViewModel GetCustomer(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            return _customerService.GetCustomer(id);
        }

        [HttpPost, Route("download")]
        public HttpResponseMessage GetDownload([FromBody]List<object> reportQueryParams)
        {
            return _customerService.GetDownload(reportQueryParams);
        }
    }
}
using Pheonix.Core.Services;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Pheonix.Web.Extensions;
using System;
using System.Security.Claims;
using Pheonix.Web.Authorization;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/customercontract"), Authorize]
    public class CustomerContractController : ApiController, ICustomerCRUDService<CustomerContractViewModel>
    {
        private ICustomerContractService _customerContractService;

        public CustomerContractController(ICustomerContractService customerContractService)
        {
            _customerContractService = customerContractService;
        }

        [Route("list")]
        public IEnumerable<CustomerContractViewModel> GetList(string filters = null)
        {
            var customerContractList = _customerContractService.GetList(filters);

            return customerContractList;
        }

        [Route("add"), HttpPost]
        public ActionResult Add(CustomerContractViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerContractService.Add(model);
        }

        [Route("update"), HttpPost]
        public ActionResult Update(CustomerContractViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _customerContractService.Update(model);
        }

        [Route("delete/{id:int}"), HttpPost]
        public ActionResult Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");
            var personid = Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            return _customerContractService.Delete(id, personid);
        }

        [Route("getcustomer/{id:int}"), HttpPost]
        public CustomerContractViewModel GetCustomer(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            return _customerContractService.GetCustomer(id);
        }

        [Route("addContract"), HttpPost]
        public ActionResult AddContract(ContractAttachmentViewModel model)
        {
            return _customerContractService.Add(model, Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }
    }
}
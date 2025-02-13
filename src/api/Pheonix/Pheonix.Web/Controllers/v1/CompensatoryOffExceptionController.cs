using System.Collections.Generic;
using System.Web.Http;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Models;
using System.Threading.Tasks;
using Pheonix.Web.Filters;
using Pheonix.Web.Extensions;
using System.Security.Claims;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.ViewModels;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/compoffexception"), Authorize]
    public class CompensatoryOffExceptionController : ApiController
    {
        private ICompOffExceptionService _service;
        public CompensatoryOffExceptionController(ICompOffExceptionService service)
        {
            _service = service;
        }
        [HttpGet, Route("ManagerList")]
        public async Task<List<PersonViewModel>> GetManagerList()
        {
            var list = new List<PersonViewModel>();
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                list = await _service.GetManagerList();
            }
            return list;
        }
        [HttpGet, Route("ExceptionList")]
        public async Task<List<PersonViewModel>> GetExecptionList()
        {
            var list = new List<PersonViewModel>();
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                list = await _service.GetExecptionList();
            }
            return list;
        }
        [HttpGet, Route("Search/{*query}")]
        public async Task<ActiveEmpViewModel> GetEmployee(string query, string managerID)
        {
            var model = new ActiveEmpViewModel();
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                model = await _service.GetActiveEmployeeList(query.Trim(), managerID.Trim());
            }
            return model;
        }
        [HttpPost, Route("Add")]
        public async Task<AdminActionResult> Add(List<CompOffExceptionViewModel> model)
        {
            AdminActionResult result = new AdminActionResult();
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                 result = await _service.Add(model);
            }
            return result;
        }
        [HttpPost, Route("Remove")]
        public async Task<AdminActionResult> Remove(List<CompOffExceptionViewModel> model)
        {
            AdminActionResult result = new AdminActionResult();
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                result = await _service.Remove(model);
            }
            return result;
        }
    }
}


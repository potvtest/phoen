using Pheonix.Core.v1.Services.PMS;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web;
using System.Security.Claims;
using Pheonix.Web.Authorization;
using Pheonix.Web.Extensions;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/pms"), Authorize]
    public class PMSController : ApiController
    {
        private readonly IPMSService pmsService;
        public PMSController(IPMSService _pmsService)
        {
            this.pmsService = _pmsService;
        }

        [HttpGet, Route("getPMSActions/{project:int}")]
        public List<int?> GetPMSActions(int project)
        {
            return pmsService.GetPMSActionsResult(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), project);
        }

        [HttpGet, Route("get-org-roles")]
        public async Task<IEnumerable<RoleViewModel>> GetOrganizationRoles()
        {
            return await pmsService.GetOrganizationRoles();
        }

        [HttpGet, Route("get-pms-roles")]
        public async Task<IEnumerable<PMSRolesViewModel>> GetPMSRoles()
        {
            return await pmsService.GetPMSRoles();
        }

        [HttpGet, Route("get-pms-actions/{filter}")]
        public async Task<IEnumerable<PMSActionViewModel>> GetPMSActions(string filter)
        {
            try
            {
                var filterResult = new JavaScriptSerializer().Deserialize<Filter>(filter);
                return await pmsService.GetPMSActions(filterResult.PmsRoleID, filterResult.OrgRoleID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Route("save-pms-role")]
        public async Task<PMSRolesViewModel> SavePMSRole([FromBody]Filter filter)
        {
            return await pmsService.SavePMSRole(filter.Name, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [HttpPost, Route("save-pms-actions")]
        public async Task<bool> SavePMSActions(List<PMSActionViewModel> list)
        {
            return await pmsService.SavePMSActions(list, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }
    }
    public class Filter
    {
        public int PmsRoleID { get; set; }
        public int OrgRoleID { get; set; }
        public string Name { get; set; }
    }
}

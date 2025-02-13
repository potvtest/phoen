using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM.Classes.ResourceAllocation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using Pheonix.Web.Extensions;
using System.Web.Http;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.DBContext;
using Pheonix.Models.ViewModels;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/resourceallocation"), Authorize]
    public class ResourceAllocationController : ApiController
    {
        private IResourceAllocationService service;
        public ResourceAllocationController(IResourceAllocationService service)
        {
            this.service = service;
        }

        [Route("get-project-list/{userId:int}/{rmg:bool}"), HttpGet]
        public async Task<IEnumerable<ResourceAllocationProjectDetails>> GetProjectList(int userId, bool rmg)
        {
            return await service.GetProjectList(userId, rmg);
        }

        [Route("get-resource-allocation-history-detail/{PersonID:int}"), HttpGet]
        public async Task<IEnumerable<CurrentResourceAllocationModel>> GetResourceAllocationHistoryDetail(int PersonID)
        {
            return await service.GetResourceAllocationHistoryDetail(PersonID);
        }

        [Route("get-other-project-employee/{projectId:int}"), HttpGet]
        public async Task<IEnumerable<ResourceViewModel>> GetOtherProjectEmployee(int projectId)
        {
            return await service.GetOtherProjectEmployee(projectId);
        }

        [Route("ra-raised-request"), HttpPost]
        public async Task<List<ResourceAllocationResponse>> RARaisedRequest(RARaisedRequest model)
        {
            return await service.RARaisedRequest(model, Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [Route("ra-get-project-skill-list/{projectID:int}"), HttpGet]
        public async Task<List<ProjectSkillDetails>> RAGetProjectSkill(int projectID)
        {
            return await service.RAGetProjectSkill(projectID);
        }

        [Route("ra-get-request-by-projectID/{userID:int}/{rmg:bool}"), HttpGet]
        public async Task<IEnumerable<RAGetRaisedRequest>> RAGetRequestByProjectID(int userID, bool rmg)
        {
            return await service.RAGetRequestByProjectID(userID, rmg);
        }

        [Route("ra-delete-full-raised-request/{requestID:int}/{requestType:int}"), HttpGet]
        public async Task<bool> RADeleteFullRaisedRequest(int requestID, int requestType)
        {
            var result = await service.RADeleteFullRaisedRequest(requestID, requestType);
            return result;

        }

        [Route("ra-edit-full-raised-request"), HttpPost]
        public async Task<bool> RAEditFullRaisedRequest(RAGetRaisedRequest model)
        {
            return await service.RAEditFullRaisedRequest(model);
        }

        [Route("ra-approve-full-raised-request"), HttpPost]
        public async Task<List<ResourceAllocationResponse>> RAApproveFullRaisedRequest(RAGetRaisedRequest model)
        {
            return await service.RAApproveFullRaisedRequest(model, Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }
        [Route("ra-reject-full-raised-request"), HttpPost]
        public async Task<bool> RARejectFullRaisedRequest(RARejectRequest rARejectRequest)
        {
            var result = await service.RARejectFullRaisedRequest(rARejectRequest);
            return result;
        }

        [Route("ra-get-my-allocation/{userID:int?}/{projectID:int?}"), HttpGet]
        public async Task<RAMyAllocation> RAGetMyAllocation(int userID = 0, int projectID = 0)
        {
            return await service.RAGetMyAllocation(userID, projectID);
        }

        [Route("ra-get-pms-roles"), HttpGet]
        public async Task<List<PMSRolesViewModel>> RAGetPMSRoles()
        {
            return await service.RAGetPMSRoles();
        }

        [Route("ra-get-pms-billable-type"), HttpGet]
        public async Task<List<PMSAllocationBillableType>> RAGetPMSBillableType()
        {
            return await service.RAGetPMSBillableType();
        }

        [Route("ra-perform-action-on-current-allocation"), HttpPost]
        public async Task<List<ResourceAllocationResponse>> RAPerformActionOnCurrentAllocation(CurrentResourceAllocationModel model)
        {
            return await service.RAPerformActionOnCurrentAllocation(model);
        }
    }
}
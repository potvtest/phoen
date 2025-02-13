using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.ResourceAllocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IResourceAllocationService
    {
        Task<IEnumerable<ResourceAllocationProjectDetails>> GetProjectList(int userId, bool rmg);
        Task<IEnumerable<CurrentResourceAllocationModel>> GetResourceAllocationHistoryDetail(int PersonID);
        Task<IEnumerable<ResourceViewModel>> GetOtherProjectEmployee(int projectId);
        Task<List<ResourceAllocationResponse>> RARaisedRequest(RARaisedRequest model, int userId);
        Task<List<ProjectSkillDetails>> RAGetProjectSkill(int projectID);
        Task<List<RAGetRaisedRequest>> RAGetRequestByProjectID(int userID, bool rmg);
        Task<bool> RADeleteFullRaisedRequest(int RequestId, int requestType);
        Task<bool> RAEditFullRaisedRequest(RAGetRaisedRequest model);
        Task<List<ResourceAllocationResponse>> RAApproveFullRaisedRequest(RAGetRaisedRequest model, int userId);
        Task<bool> RARejectFullRaisedRequest(RARejectRequest rARejectRequest);
        Task<RAMyAllocation> RAGetMyAllocation(int userID, int projectID);
        Task<List<PMSRolesViewModel>> RAGetPMSRoles();
        Task<List<PMSAllocationBillableType>> RAGetPMSBillableType();
        Task<List<ResourceAllocationResponse>> RAPerformActionOnCurrentAllocation(CurrentResourceAllocationModel model);
        bool RAApproveDirectNewRequest(List<RAResource> model);
        bool RAApproveDirectUpdateRequest(List<RAResource> model);
        bool RAApproveDirectExtentionRequest(List<RAResource> model);
        bool RAApproveDirectReleaseRequest(List<RAResource> model);
        
        bool DeleteRAData(int userId, DateTime ApprovalDate);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.ViewModels;

namespace Pheonix.Core.v1.Services.ValuePortal//ValuePortal.Services
{
    public interface IValuePortalService
    {
        Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaList(VPApproverFlowHandlerModel approverFlowPayload);
        Task<List<LimitedDataIdeaDetailsViewModel>> getLimitedDataIdealist(VPApproverFlowHandlerModel approverFlowPayload);
        Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListForBUApprover(long userId, int Status, VPBUApproverHandlerModel buApproverPayload);
        //Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListByUser(int userId);
        Task<VCIdeaMasterViewModel> GetIdea(long id, long loginuserId);
        Task<LimitedDataIdeaDetailsViewModel> getLimitedIdeaDetailsData(long id, long loginuserId);
        Task<bool> SaveIdea(VCIdeaMasterViewModel idea);//,int userId);

        Task<bool> UpdateIdea(VCIdeaMasterViewModel idea, int userId , bool isGlobalApprover, bool isBUApprover, string DirtyValuesList);

        // Commented on 8th March 22 after Sync-up call as per disscussion with Amit Sharma
        //Task<bool> LockUnlockTheIdea(int ideaId, int userId);

        Task<VPSubmittedCountViewModel> GetSubmittedIdeaCountbyId(VPApproverFlowHandlerModel vcfUserModel);
        Task<bool> SaveComment(VPCommentsViewModel ideaCommentVM);
        //Task<IEnumerable<VPPriorityViewModel>> GetPriorityList();
        // Task<IEnumerable<VPCommentsViewModel>> GetVPCommentData(long id);
        Task<VPAllMastersdataViewModel> GetMastersdata();
        Task<IEnumerable<VPConfigurationViewModel>> GetConfigurationdata();
        Task<VPBUApproverHandlerModel> checkIsBUApprover(VPBUApproverHandlerModel vPBUApproverVM);
    }
}

using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
//using ValuePortal.Services;
using Pheonix.Core.v1.Services.ValuePortal;
namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/vp")]
 
    public class VPController : ApiController
    {
        private IValuePortalService _valuePortalService;
        // GET: VP

        //public VPController() 
        //{
           
        //}
        public VPController(IValuePortalService valuePortalService)
        {
            _valuePortalService = valuePortalService;
        }

        [HttpPost, Route("idealist")]
        public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaList(VPApproverFlowHandlerModel approverFlowPayload) => await _valuePortalService.GetIdeaList(approverFlowPayload);

        [HttpPost, Route("getLimitedDataIdealist")]
        public async Task<List<LimitedDataIdeaDetailsViewModel>> getLimitedDataIdealist(VPApproverFlowHandlerModel approverFlowPayload) => await _valuePortalService.getLimitedDataIdealist(approverFlowPayload);


        [HttpPost, Route("ideaListForBUApprover/{userId}/{status}")]
        public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListForBUApprover(long userId, int status, VPBUApproverHandlerModel buApproverPayload) => await _valuePortalService.GetIdeaListForBUApprover(userId, status, buApproverPayload);

        //[HttpGet, Route("idealistbyuser")]
        //public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListByUser()
        //{            
        //    return await _valuePortalService.GetIdeaListByUser(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        //}

        [HttpGet, Route("getidea/{id}/{loginuserId}")]
        public async Task<VCIdeaMasterViewModel> GetIdea(long id, long loginuserId)
        {
            return await _valuePortalService.GetIdea(id, loginuserId);
        }

        [HttpGet, Route("getLimitedIdeaDetailsById/{id}/{loginuserId}")]
        public async Task<LimitedDataIdeaDetailsViewModel> getLimitedIdeaDetailsData(long id, long loginuserId) => await _valuePortalService.getLimitedIdeaDetailsData(id, loginuserId);

        [HttpGet, Route("GetAllMasterdata")]
        public async Task<VPAllMastersdataViewModel> GetAllMasterdata()
        {
            return await _valuePortalService.GetMastersdata();
        }
        [HttpGet, Route("GetConfigurationdata")]
        public async Task<IEnumerable<VPConfigurationViewModel>> GetConfigurationdata()
        {
            return await _valuePortalService.GetConfigurationdata();
        }
        [HttpPost, Route("SaveIdea")]//submitidea
        public async Task<bool> SaveIdea(VCIdeaMasterViewModel idea)//VCIdeaMasterViewModel //SubmitIdea
        {
            return await _valuePortalService.SaveIdea(idea);//, 1);// RequestContext.GetClaimInt(ClaimTypes.PrimarySid));            
            //return await _valuePortalService.SaveIdea(idea, 1);
        }

        [HttpPost, Route("GetSubmittedIdeaCountbyId")]
        public async Task<VPSubmittedCountViewModel>  GetSubmittedIdeaCountbyId(VPApproverFlowHandlerModel vcfUserModel)
        {
            return await _valuePortalService.GetSubmittedIdeaCountbyId(vcfUserModel);
        }

        [HttpPost, Route("UpdateIdea/{isGlobalApprover}/{isBUApprover}/{dirtyValuesList}")]
        public async Task<bool> UpdateIdea(VCIdeaMasterViewModel idea, bool isGlobalApprover, bool isBUApprover, string dirtyValuesList)
        {
            int loggedInUderId = RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
            return await _valuePortalService.UpdateIdea(idea, loggedInUderId, isGlobalApprover , isBUApprover, dirtyValuesList);;            
        }

        // Commented on 8th March 22 after Sync-up call as per disscussion with Amit Sharma
        //[HttpPost, Route("LockUnlockTheIdea/{ideaId}/{userId}")]//submitidea
        //public async Task<bool> LockUnlockTheIdea(int ideaId, int userId)
        //{
        //    return await _valuePortalService.LockUnlockTheIdea(ideaId, userId);// RequestContext.GetClaimInt(ClaimTypes.PrimarySid));            
        //    //return await _valuePortalService.SaveIdea(idea, 1);
        //}

        [HttpPost, Route("SaveComment/{ideaNewCommentModel}")]
        public async Task<bool> SaveComment(VPCommentsViewModel ideaNewCommentVM)
        {
            return await _valuePortalService.SaveComment(ideaNewCommentVM);// RequestContext.GetClaimInt(ClaimTypes.PrimarySid));            
            //return await _valuePortalService.SaveIdea(idea, 1);
        }

        [HttpPost, Route("checkIsBUApprover")]
        public async Task<VPBUApproverHandlerModel> checkIsBUApprover(VPBUApproverHandlerModel vPBUApproverVM)
        {
            return await _valuePortalService.checkIsBUApprover(vPBUApproverVM);
        }

        //[HttpGet, Route("GetVPCommentData/{id}")]
        //public async Task<IEnumerable<VPCommentsViewModel>> GetVPCommentData(long id)
        //{
        //    return await _valuePortalService.GetVPCommentData(id);
        //}
    }
}
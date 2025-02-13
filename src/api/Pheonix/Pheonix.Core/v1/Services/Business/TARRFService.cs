using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Core.Repository.TARRFrequest;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.TalentAcqRRF;

namespace Pheonix.Core.v1.Services.Business
{
    public class TARRFService : ITARRFService
    {
        private ITARRFRepository RepoService;

        public TARRFService(ITARRFRepository _RepoService)
        {
            RepoService = _RepoService;
        }
        public async Task<IEnumerable<TARRFViewModel>> GetAllTalentAcqRequestsAsync()
        {
            return await RepoService.GetAllTalentAcqRequestsAsync();
        }

        public async Task<IEnumerable<TARRFViewModel>> GetMyTalentAcqRequestsAsync(int personId, int isApproval, bool isHR)
        {
            return await RepoService.GetMyTalentAcqRequestsAsync(personId, isApproval, isHR);
        }

        //--- to get all req for Approver.
        public async Task<IEnumerable<TARRFViewModel>> GetReqForAppover(int personId)
        {
            return await RepoService.GetReqForAppover(personId);
        }

        //--- to get all req for HR with status = "Approved"
        public async Task<IEnumerable<TARRFViewModel>> GetReqForHR()
        {
            return await RepoService.GetReqForHR();
        }

        //--- For get
        public async Task<TARRFViewModel> GetTalentAcqReqById(int ReqId)
        {
            return await RepoService.GetTalentAcqReqById(ReqId);
        }

        //--- For get
        public async Task<List<TARRFDetailViewModel>> GetTalentAcqReqDetails(int ReqId)
        {
            return await RepoService.GetTalentAcqReqDetails(ReqId);
        }

        //---- for save/save as draft/update
        public async Task<bool> SaveUpdateTalentAcqRequestsAsync(TARRFViewModel model)
        {
            return await RepoService.SaveUpdateTalentAcqRequests(model);
        }

        //---- for save/update request details (subsequent RRF requests bifurcated after HR approval)
        public async Task<bool> SaveUpdateRequestDetailsAsync(TARRFDetailViewModel model)
        {
            return await RepoService.SaveUpdateRequestDetailsAsync(model);
        }

        //-- For ApprovedByHR
        public async Task<bool> ReqApprovedByHR(int SLA, string Comments, int ReqId)
        {
            return await RepoService.ReqApprovedByHR(SLA, Comments, ReqId);
        }

        public async Task<bool> DeleteReq(int ReqId)
        {
            return await RepoService.DeleteReq(ReqId);
        }

        public async Task<List<DropdownItems>> GetDesignationDropDown()
        {
            return await RepoService.GetDesignationDropDown();
        }

        public async Task<List<DropdownItems>> GetJobDescriptionDropDown()
        {
            return await RepoService.GetJobDescriptionDropDown();
        }

        public async Task<List<DropdownItems>> GetRRFApproverDropDown()
        {
            return await RepoService.GetRRFApprover();
        }

        public bool SwapRRFOwner(int userId) {
            return RepoService.SwapRRFOwner(userId);
        }

    }
}

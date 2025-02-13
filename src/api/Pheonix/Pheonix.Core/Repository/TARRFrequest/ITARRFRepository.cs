using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.Repository.TARRFrequest
{
    public interface ITARRFRepository
    {
        Task<List<TARRFViewModel>> GetAllTalentAcqRequestsAsync(); // -- for HR 
        Task<List<TARRFViewModel>> GetMyTalentAcqRequestsAsync(int UserId, int isApproval, bool isHR);   // --- MY REQ ONLY
        Task<TARRFViewModel> GetTalentAcqReqById(int ReqId);
        Task<List<TARRFDetailViewModel>> GetTalentAcqReqDetails(int ReqId);
        Task<bool> SaveUpdateTalentAcqRequests(TARRFViewModel model);
        Task<bool> SaveUpdateRequestDetailsAsync(TARRFDetailViewModel model);
        Task<bool> DeleteReq(int ReqId);
        Task<bool> ReqApprovedByHR(int SLA, string Comments, int ReqId);
        Task<List<TARRFViewModel>> GetReqForHR();
        Task<List<TARRFViewModel>> GetReqForAppover(int personId);
        Task<List<DropdownItems>> GetDesignationDropDown();
        Task<List<DropdownItems>> GetJobDescriptionDropDown();
        Task<List<DropdownItems>> GetRRFApprover();
        bool SwapRRFOwner(int userId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using Pheonix.Models.VM;


namespace Pheonix.Core.v1.Services.Business
{
    public interface ITARRFService
    {
        Task<IEnumerable<TARRFViewModel>> GetAllTalentAcqRequestsAsync(); // -- for HR 
        Task<IEnumerable<TARRFViewModel>> GetMyTalentAcqRequestsAsync(int UserId, int isApproval, bool isHR);   // --- MY REQ ONLY
        Task<TARRFViewModel> GetTalentAcqReqById(int ReqId);
        Task<List<TARRFDetailViewModel>> GetTalentAcqReqDetails(int ReqId);
        Task<bool> SaveUpdateTalentAcqRequestsAsync(TARRFViewModel model);
        Task<bool> SaveUpdateRequestDetailsAsync(TARRFDetailViewModel model);
        Task<bool> DeleteReq(int ReqId);
        Task<bool> ReqApprovedByHR(int SLA, string Comments, int ReqId);
        Task<IEnumerable<TARRFViewModel>> GetReqForHR();
        Task<IEnumerable<TARRFViewModel>> GetReqForAppover(int personId);
        Task<List<DropdownItems>> GetDesignationDropDown();
        Task<List<DropdownItems>> GetJobDescriptionDropDown();
        Task<List<DropdownItems>> GetRRFApproverDropDown();
        bool SwapRRFOwner(int userId);


    }
}

using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using Pheonix.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/talentacqrequest")]
    public class TARRFController : ApiController
    {
        private ITARRFService  _Service;
        public TARRFController(ITARRFService service)
        {
            this._Service = service;
        }

        [HttpGet, Route("get-allreqs-list")]
        public async Task<IEnumerable<TARRFViewModel>> GetAllTalentAcqRequests()
        {
            return await _Service.GetAllTalentAcqRequestsAsync();
        }

        [HttpPost, Route("for-addupdate-req")]
        public async Task<bool> SaveUpdateTalentAcqRequests(TARRFViewModel model)
        {
            return await _Service.SaveUpdateTalentAcqRequestsAsync(model);
        }

        [HttpPost, Route("for-addupdate-reqdetails")]
        public async Task<bool> SaveUpdateRequestDetails(TARRFDetailViewModel model)
        {
            return await _Service.SaveUpdateRequestDetailsAsync(model);
        }

        [HttpGet, Route("get-my-reqs-list/{listFlag:int}/{isHR:bool}")]
        public async Task<IEnumerable<TARRFViewModel>> GetMyTalentAcqRequests(int listFlag, bool isHR)
        {
            return await _Service.GetMyTalentAcqRequestsAsync(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), listFlag, isHR);
        }
               
        [HttpGet,Route("get-req-byId/{ReqId}")]
        public async Task<TARRFViewModel> GetTalentAcqReqById(int ReqId)
        {
            return await _Service.GetTalentAcqReqById(ReqId);
        }

        [HttpGet, Route("get-req-details/{ReqId}")]
        public async Task<List<TARRFDetailViewModel>> GetTalentAcqReqDetails(int ReqId)
        {
            return await _Service.GetTalentAcqReqDetails(ReqId);
        }
        
        [HttpGet, Route("get-JD-dropdown")]
        public async Task<List<DropdownItems>> GetJDDropDown()
        {
            return await _Service.GetJobDescriptionDropDown();
        }

        [HttpGet, Route("get-Designation-dropdown")]
        public async Task<List<DropdownItems>> GetDesignationDropDown()
        {
            return await _Service.GetDesignationDropDown();
        }

        [HttpGet, Route("get-Approver-dropdown")]
        public async Task<List<DropdownItems>> GetRRFApproverDropDown()
        {
            return await _Service.GetRRFApproverDropDown();
        }




    }
}

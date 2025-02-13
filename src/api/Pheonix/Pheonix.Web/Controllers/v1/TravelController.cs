using Pheonix.Core.v1.Services.Business;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Security.Claims;
using Pheonix.Web.Extensions;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Travel;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/travel"), Authorize]
    public class TravelController : ApiController
    {
        private ITravelService service;
        public TravelController(ITravelService service)
        {
            this.service = service;
        }

        [Route("save-update-travel"), HttpPost]
        public async Task<IHttpActionResult> Save(TravelViewModel model)
        {
            var asdads = (await service.SaveTravel(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
            return Ok(model);
        }

        [Route("get-mytravel-list"), HttpGet]
        public async Task<IHttpActionResult> GetMyTravelRequests()
        {
            return Ok(await service.GetMyTravelRequests(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("save-update-emergencycontacts")]
        public async Task<IHttpActionResult> SaveEmergencyContacts(EmployeeEmergencyContact model)
        {
            return Ok(await service.SaveEmergencyContacts(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("delete-emergencycontacts"), HttpPost]
        public async Task<IHttpActionResult> DeleteEmergencyContact(EmployeeEmergencyContact model)
        {
            return Ok(await service.DeleteEmergencyContact(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("save-update-visadetails"), HttpPost]
        public async Task<IHttpActionResult> SaveVisaDetail(EmployeeVisa model)
        {
            return Ok(await service.SaveVisaDetail(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("delete-visa"), HttpPost]
        public async Task<IHttpActionResult> DeleteEmergencyContact(EmployeeVisa model)
        {
            return Ok(await service.DeleteEmergencyContact(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("get-travelapprovals"), HttpGet]
        public async Task<IHttpActionResult> GetRequestsToApprove()
        {
            return Ok(await service.GetRequestsToApprove(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("get-travelapprovalslist/{year}"), HttpGet]
        public async Task<IHttpActionResult> GetRequestsToApproveList(int year)
        {
            return Ok(await service.GetRequestsToApproveList(RequestContext.GetClaimInt(ClaimTypes.PrimarySid),year));
        }

        [Route("get-travel-pending-requests"), HttpGet]
        public async Task<IHttpActionResult> GetPendingRequests()
        {
            return Ok(await service.GetPendingRequests(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("view-request/{id}"), HttpGet]
        public async Task<IHttpActionResult> ViewTravelRequest(int id)
        {
            return Ok(await service.ViewTravelRequest(id, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("upload-money"), HttpPost]
        public async Task<IHttpActionResult> UploadMoney(MoneyTransactionViewModel model)
        {
            return Ok(await service.UploadMoney(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("upload-document"), HttpPost]
        public async Task<IHttpActionResult> UploadDocument(UploadedDocumentViewModel model)
        {
            return Ok(await service.UploadDocuments(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("delete-document/{id}"), HttpGet]
        public async Task<IHttpActionResult> DeleteDocument(int id)
        {
            return Ok(await service.DeleteDocument(id, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("save-hotelbooking"), HttpPost]
        public async Task<IHttpActionResult> SaveUpdateHotelBooking(TravelHotelBooking model)
        {
            return Ok(await service.SaveUpdateHotelBooking(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("save-flight-booking"), HttpPost]
        public async Task<IHttpActionResult> SaveFlightBooking(TravelFlight model)
        {
            return Ok(await service.SaveFligtBooking(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("get-flight-details/{travelId}"), HttpGet]
        public async Task<IHttpActionResult> GetFlightDetails(int travelId)
        {
            return Ok(await service.GetFlightDetails(travelId));
        }

        [Route("approve-travel/{travelId}"), HttpGet]
        public async Task<IHttpActionResult> ApprroveTravel(int travelId)
        {
            return Ok(await service.ApproveTravel(travelId, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("reject-travel/{travelId}/{*comments}"), HttpGet]
        public async Task<IHttpActionResult> RejectTravel(int travelId, string comments)
        {
            return Ok(await service.RejectTravel(travelId, RequestContext.GetClaimInt(ClaimTypes.PrimarySid), comments));
        }

        [Route("onHold-travel/{travelId}/{*comments}"), HttpGet]
        public async Task<IHttpActionResult> OnHoldTravel(int travelId, string comments)
        {
            return Ok(await service.OnHoldTravel(travelId, RequestContext.GetClaimInt(ClaimTypes.PrimarySid), comments));
        }

        [Route("add-travel-extension"), HttpPost]
        public async Task<IHttpActionResult> AddTravelExtension(TravelExtension model)
        {
            return Ok(await service.AddTravelExtension(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("close-travel/{travelId}/{comments}"), HttpGet]
        public async Task<IHttpActionResult> CloseTravelRequest(int travelId, string comments)
        {
            return Ok(await service.CloseTravelRequest(travelId, RequestContext.GetClaimInt(ClaimTypes.PrimarySid), comments));
        }

        [Route("approved-travelrequests"), HttpGet]
        public async Task<IHttpActionResult> GetApprovedTravelRequests()
        {
            return Ok(await service.ApprovalHistory(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("isApprovalAdmin"), HttpGet]
        public async Task<IHttpActionResult> IsApproverOrAdmin()
        {
            return Ok(await service.IsApproverOrAdmin(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("dropdowns"), HttpGet]
        public async Task<IHttpActionResult> GetDropdowns()
        {
            return Ok(await service.GetDropdowns(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("travel-details/{travelid}"), HttpGet]
        public async Task<IHttpActionResult> GetTravelDetails(int travelid)
        {
            return Ok(await service.GetTravelDetails(travelid));
        }

        [Route("delete-flight/{flightId}"), HttpGet]
        public async Task<IHttpActionResult> DeleteFlight(int flightId)
        {
            return Ok(await service.DeleteFlight(flightId));
        }

        [Route("delete-hotelBooking/{hotelId}"), HttpGet]
        public async Task<IHttpActionResult> DeleteHotel(int hotelId)
        {
            return Ok(await service.DeleteHotelBooking(hotelId));
        }

        [Route("travel-activity"), HttpGet]
        public IHttpActionResult DashboardCardActivity()
        {
            return Ok(service.DashBoardTravelCardView(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("update-travel"), HttpPost]
        public async Task<IHttpActionResult> UpdateTravel(TravelViewModel model)
        {
            var asdads = (await service.UpdateTravel(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
            return Ok(model);
        }

        [Route("getSearchTravelDetail/{id}"), HttpGet]
        public async Task<IHttpActionResult> GetTravelRequest(int id)
        {
            return Ok(await service.GetSearchTravelCardDetl(id, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("addOtherEmpTravelRequest/{id}"), HttpPost]
        public async Task<IHttpActionResult> AddOtherEmpTravelRequest(TravelViewModel model, int id)
        {
            var result = (await service.AddOtherEmpTravelRequest(model, id, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
            return Ok(model);
        }
    }
}

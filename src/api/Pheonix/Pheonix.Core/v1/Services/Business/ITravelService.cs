using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes;
using Pheonix.Models.VM.Classes.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ITravelService
    {
        Task<bool> SaveTravel(TravelViewModel model, int id);
        Task<List<TravelViewModel>> GetMyTravelRequests(int id);
        Task<EmployeeEmergencyContact> SaveEmergencyContacts(EmployeeEmergencyContact model, int id);
        Task<bool> DeleteEmergencyContact(EmployeeEmergencyContact model, int id);
        Task<EmployeeVisa> SaveVisaDetail(EmployeeVisa model, int id);
        Task<bool> DeleteEmergencyContact(EmployeeVisa model, int id);
        Task<IEnumerable<TravelViewModel>> GetRequestsToApprove(int id);
        Task<List<TravelListViewModel>> GetRequestsToApproveList(int id, int year);
        Task<IEnumerable<TravelViewModel>> GetPendingRequests(int id);
        Task<TravelViewModel> ViewTravelRequest(int requestId, int id);
        Task<List<MoneyTransactionViewModel>> UploadMoney(MoneyTransactionViewModel model, int id);
        Task<UploadedDocumentViewModel> UploadDocuments(UploadedDocumentViewModel model, int id);
        Task<bool> DeleteDocument(int documentId, int id);
        Task<TravelHotelBooking> SaveUpdateHotelBooking(TravelHotelBooking model, int id);
        Task<TravelFlight> SaveFligtBooking(TravelFlight model, int id);
        Task<IEnumerable<TravelFlight>> GetFlightDetails(int travelId);
        Task<List<ApprovalDetailVM>> ApproveTravel(int travelId, int id);
        Task<bool> RejectTravel(int travelId, int id, string comments);
        Task<List<ApprovalDetailVM>> OnHoldTravel(int travelId, int id, string comments);
        Task<bool> AddTravelExtension(TravelExtension model, int id);
        Task<bool> CloseTravelRequest(int travelId, int id, string comments);
        Task<IEnumerable<TravelViewModel>> ApprovalHistory(int userId);
        Task<bool> IsApproverOrAdmin(int userID);
        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId);
        Task<TravelDetailsVM> GetTravelDetails(int travelId);
        Task<bool> DeleteFlight(int flightId);
        Task<bool> DeleteHotelBooking(int hotelId);
        IEnumerable<TravelViewModel> DashBoardTravelCardView(int userId);
        Task<bool> UpdateTravel(TravelViewModel model, int id);
        Task<TravelViewModel> GetSearchTravelCardDetl(int personID, int id);
        Task<bool> AddOtherEmpTravelRequest(TravelViewModel model, int personID, int userId);
    }
}

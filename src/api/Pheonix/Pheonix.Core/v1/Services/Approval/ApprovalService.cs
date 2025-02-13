///Approval Status
///0-Submitted
///1-Approved
///2-Rejected
///3-On Hold

using log4net;
using Pheonix.Core.Helpers;
using Pheonix.DBContext;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DTO = Pheonix.DBContext;

namespace Pheonix.Core.v1.Services.Approval
{
    public class ApprovalService : Pheonix.Core.v1.Services.IApprovalService
    {
        public IBasicOperationsService _service;
        private static readonly ILog Log4Net = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ApprovalService(IBasicOperationsService service)
        {
            _service = service;
        }

        public ApprovalService()
        { }

        public int SendForApproval(int currentUserID, int requestType, int requestId, int[] approvers, int componentID = 0)
        {
            var model = new DTO.Approval
            {
                RequestBy = currentUserID,
                RequestType = requestType,
                RequestID = requestId != 0 ? requestId : requestType,
                RequestedDate = DateTime.Now,
                Component = componentID,
                Status = 0
            };
            var approvalDetails = new List<DTO.ApprovalDetail>();
            for (var i = 0; i < approvers.Length; i++)
            {
                approvalDetails.Add(new DTO.ApprovalDetail { ApproverID = approvers[i], Stage = i + 1, Status = 0 });
            }

            model.ApprovalDetail = approvalDetails;
            var pass = _service.Create<Pheonix.DBContext.Approval>(model, null);
            return _service.Finalize(pass);
        }

        public async Task<int> UpdateApproval(int empID, int requestType, int requestId, int statusID, string statusComment, int componentID = 0)
        {
            var oldApprovalModel = new DTO.Approval();

            if (componentID == 0)
                oldApprovalModel = _service.First<DTO.Approval>(x => x.RequestType == requestType && x.RequestID == requestId && (x.Status == 3 || x.Status == 0));
            else
                oldApprovalModel = _service.First<DTO.Approval>(x => x.Person.ID == empID && x.RequestType == requestType && x.RequestID == requestId && x.Component == componentID && (x.Status == 3 || x.Status == 0));

            var oldApprovalDetailModel = _service.First<DTO.ApprovalDetail>(x => x.Approval.ID == oldApprovalModel.ID);

            if (statusID == 3 && requestType != 4)
            {
                var onHoldHistory = new DTO.ApprovalDetail
                {
                    ID = oldApprovalDetailModel.ID,
                    ApprovalID = oldApprovalDetailModel.ApprovalID,
                    ApproverID = oldApprovalDetailModel.ApproverID,
                    Stage = oldApprovalDetailModel.Stage,
                    ApprovalDate = DateTime.Now,
                    Status = 3,
                    StatusComment = statusComment,
                    IsDeleted = true
                };
                bool isUpdated = _service.Create<DTO.ApprovalDetail>(onHoldHistory, null);

                return await Task.Run(() => { return isUpdated ? _service.Finalize(true) : _service.Finalize(false); });
            }


            var newApprovalModel = new DTO.Approval
            {
                ID = oldApprovalModel.ID,
                StatusDate = DateTime.Now,
                Status = statusID
            };

            var newApprovalDetailModel = new DTO.ApprovalDetail
            {
                ID = oldApprovalDetailModel.ID,
                ApprovalID = oldApprovalDetailModel.ApprovalID,
                ApprovalDate = DateTime.Now,
                Status = statusID,
                StatusComment = statusComment
            };

            bool isApprovalUpdated = _service.Update<DTO.Approval>(newApprovalModel, oldApprovalModel);
            bool isApprovalDetailUpdated = _service.Update<DTO.ApprovalDetail>(newApprovalDetailModel, oldApprovalDetailModel);

            return await Task.Run(() =>
            {
                int stat = (isApprovalUpdated && isApprovalDetailUpdated) ? _service.Finalize(true) : _service.Finalize(false);
                return stat;
            });
        }

        public int UpdateMultiLevelApproval(int empID, int requestType, int requestID, int statusID, string comments, int approverID)
        {
            var oldApprovalModel = new DTO.Approval();
            oldApprovalModel = _service.First<DTO.Approval>(x => x.Person.ID == empID && x.RequestType == requestType && x.RequestID == requestID && x.IsDeleted == false);
            var newApprovalModel = new DTO.Approval
            {
                StatusDate = DateTime.Now
            };
            var oldApprovalDetailModel = _service.Top<DTO.ApprovalDetail>(0, x => x.ApprovalID == oldApprovalModel.ID).ToList();
            var newApprovalDetailModel = new DTO.ApprovalDetail
            {
                ApprovalDate = DateTime.Now
            };
            switch (statusID)
            {
                case 1:
                    newApprovalModel.Status = 1;
                    newApprovalDetailModel.Status = 1;
                    newApprovalDetailModel.StatusComment = comments;
                    break;

                case 2:
                    newApprovalModel.Status = 2;
                    newApprovalDetailModel.Status = 2;
                    newApprovalDetailModel.StatusComment = comments;
                    break;

                case 3:
                    newApprovalModel.Status = 3;
                    newApprovalDetailModel.Status = 3;
                    newApprovalDetailModel.StatusComment = comments;
                    break;

                default:
                    break;
            }



            bool isApprovalDetailUpdated = _service.Update<DTO.ApprovalDetail>(newApprovalDetailModel, oldApprovalDetailModel.Where(x => x.ApproverID == approverID).FirstOrDefault());
            if (!oldApprovalDetailModel.Where(x => x.Status == 0).Any())
            {
                bool isApprovalUpdated = _service.Update<DTO.Approval>(newApprovalModel, oldApprovalModel);
                _service.Finalize(isApprovalUpdated);
            }
            return (isApprovalDetailUpdated) ? _service.Finalize(true) : _service.Finalize(false);
        }

        public List<ApprovalsViewModel> ListAllApprovalsFor(int id)
        {
            IEnumerable<ApprovalsViewModel> approvalListData = new List<ApprovalsViewModel>();
            var _approvalList = new List<ApprovalsViewModel>();
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalList = context.GetApprovalByMe(id).ToList();
                    foreach (var item in approvalList)
                    {
                        ApprovalsViewModel _ApprovalListViewModel = new ApprovalsViewModel();
                        _ApprovalListViewModel.Module = item.module;
                        _ApprovalListViewModel.Count = item.count;
                        _ApprovalListViewModel.Url = item.url;
                        _approvalList.Add(_ApprovalListViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
            approvalListData = _approvalList;
            return _approvalList;
        }

        public async Task<int> UpdateApprovalAssignedTo(DTO.Approval oldApproval, DTO.ApprovalDetail oldApprovalDetail, int AssignedTo, int status)
        {
            var newApproval = new DTO.Approval
            {
                ID = oldApproval.ID,
                StatusDate = DateTime.Now,
                RequestBy = oldApproval.RequestBy,
                RequestID = oldApproval.RequestID,
                Status = status
            };

            var newApprovalDetail = new DTO.ApprovalDetail
            {

                ID = oldApprovalDetail.ID,
                ApprovalID = oldApproval.ID,
                ApprovalDate = DateTime.Now,
                ApproverID = 337,
                Status = status,
                StatusComment = "Approved"

            };

            var isApprovalUpdated = _service.Update<DTO.Approval>(newApproval, oldApproval);
            var isApprovalDetailUpdated = _service.Update<DTO.ApprovalDetail>(newApprovalDetail, oldApprovalDetail);


            return await Task.Run(() =>
            {
                int isUpdated = _service.Finalize(isApprovalUpdated && isApprovalDetailUpdated);
                return isUpdated;
            });

        }

        public int UpdateApprovalForSeparation(int empID, int requestType, int requestID, int statusID, string comments, int approverID)
        {
            var oldApprovalModel = new DTO.Approval();
            oldApprovalModel = _service.First<DTO.Approval>(x => x.Person.ID == empID && x.RequestType == requestType && x.RequestID == requestID && x.IsDeleted == false && x.Status == 0);
            var newApprovalModel = new DTO.Approval
            {
                StatusDate = DateTime.Now
            };
            var oldApprovalDetailModel = _service.Top<DTO.ApprovalDetail>(0, x => x.ApprovalID == oldApprovalModel.ID).ToList();
            var newApprovalDetailModel = new DTO.ApprovalDetail
            {
                ApprovalDate = DateTime.Now
            };
            switch (statusID)
            {
                case 1:
                    newApprovalModel.Status = 1;
                    newApprovalDetailModel.Status = 1;
                    newApprovalDetailModel.StatusComment = comments;
                    break;

                case 2:
                    newApprovalModel.Status = 2;
                    newApprovalDetailModel.Status = 2;
                    newApprovalDetailModel.StatusComment = comments;
                    break;

                case 3:
                    newApprovalModel.Status = 3;
                    newApprovalDetailModel.Status = 3;
                    newApprovalDetailModel.StatusComment = comments;
                    break;

                default:
                    break;
            }

            bool isApprovalDetailUpdated = _service.Update<DTO.ApprovalDetail>(newApprovalDetailModel, oldApprovalDetailModel.Where(x => x.ApproverID == approverID).FirstOrDefault());
            if (!oldApprovalDetailModel.Where(x => x.Status == 0).Any())
            {
                bool isApprovalUpdated = _service.Update<DTO.Approval>(newApprovalModel, oldApprovalModel);
                _service.Finalize(isApprovalUpdated);
            }
            return (isApprovalDetailUpdated) ? _service.Finalize(true) : _service.Finalize(false);
        }

        public int SendForHRApproval(int currentUserID, int requestType, int requestId, int approvers, int componentID = 0)
        {
            var model = new DTO.Approval
            {
                RequestBy = currentUserID,
                RequestType = requestType,
                RequestID = requestId != 0 ? requestId : requestType,
                RequestedDate = DateTime.Now,
                Component = componentID,
                Status = 1
            };
            var approvalDetails = new List<DTO.ApprovalDetail>();

            approvalDetails.Add(new DTO.ApprovalDetail { ApproverID = approvers, Stage = 0, Status = 1, StatusComment = "Approved" });

            model.ApprovalDetail = approvalDetails;
            var pass = _service.Create<Pheonix.DBContext.Approval>(model, null);
            return _service.Finalize(pass);
        }

        //This will save approval request by Travel Admin on behalf of Search Employee
        public int SendForApprovalForTravelAdminRequest(int currentUserID, int requestType, int requestId, int[] approvers, int componentID = 0)
        {
            var model = new DTO.Approval
            {
                RequestBy = currentUserID,
                RequestType = requestType,
                RequestID = requestId != 0 ? requestId : requestType,
                RequestedDate = DateTime.Now,
                Component = componentID,
                Status = 0
            };
            var approvalDetails = new List<DTO.ApprovalDetail>();
            for (var i = 0; i < approvers.Length; i++)
            {
                if (i == 0)//This will marked travel request as Approved for Primary Approver stage by Travel Admin
                    approvalDetails.Add(new DTO.ApprovalDetail { ApproverID = approvers[i], Stage = i + 1, Status = 1, StatusComment = "Approved by Travel Admin", ApprovalDate = DateTime.Now });
                else
                    approvalDetails.Add(new DTO.ApprovalDetail { ApproverID = approvers[i], Stage = i + 1, Status = 0 });
            }

            model.ApprovalDetail = approvalDetails;
            var pass = _service.Create<Pheonix.DBContext.Approval>(model, null);
            return _service.Finalize(pass);
        }

        private int GetSignInSignOut(int empID)
        {
            using (PhoenixEntities context = new PhoenixEntities())
                return context.SignInSignOut.Where(x => (x.statusID == 1 || x.statusID == null) && x.ApproverID == empID).Count();
        }

        private int GetPendingRRF(int empID, bool isHR)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var tarrfdetail = context.TARRFDetail.Where(x => x.Status == 0).ToList();
                var approvertarrf = context.TARRF.Where(x => x.PrimaryApprover == empID && x.RRFStatus == 0 && !x.IsDraft).ToList();
                var tarrf = new List<TARRF>();
                if (isHR)
                {
                    tarrf = context.TARRF.Where(x => x.HRApprover == 0 && x.RRFStatus == 1 && !x.IsDraft).ToList();
                    foreach (TARRFDetail item in tarrfdetail)
                    {
                        /// Check and add to list; if Recruiter is having any open request from RRF detailed (splitted) requests.
                        var hrTArrf = context.TARRF.Where(x => x.RRFNo == item.RRFNo).First();
                        if (tarrf.IndexOf(hrTArrf) < 0)
                            tarrf.Add(hrTArrf);
                    }
                    foreach (TARRF item in approvertarrf)
                    {
                        if (tarrf.IndexOf(item) < 0)
                            tarrf.Add(item);
                    }
                }
                else
                    tarrf = approvertarrf;

                return tarrf.Count();
            }
        }
    }
}

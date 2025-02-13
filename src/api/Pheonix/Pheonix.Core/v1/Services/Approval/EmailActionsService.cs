using AutoMapper;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.Email;
using Pheonix.Core.Helpers;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Approval
{
    public class EmailActionsService : IEmailActionsService
    {
        private IBasicOperationsService _opsSrvice;
        private IEmployeeLeaveService _empLeaveService;
        private IEmailService _emailService;
        private IExpenseService _expenseService;

        public EmailActionsService(IBasicOperationsService opsSrvice, IEmployeeLeaveService empLeaveService, IEmailService emailService, IExpenseService expenseService)
        {
            _opsSrvice = opsSrvice;
            _emailService = emailService;
            _empLeaveService = empLeaveService;
            _expenseService = expenseService;
        }


        public async Task<Status> LeaveActionViaEmail(Guid authKey, string action, string email)
        {
            Status status = Status.InvalidAuthKey;

            var emailAction = _opsSrvice.First<EmailAction>(t => t.AuthKey == authKey);
            if (emailAction == null)
            {
                return await Task.Run(() => { return status = Status.InvalidAuthKey; });
            }
            else if (emailAction.ActionOn != null && emailAction.isAccessed == true) //changed isAccessed to true;
            {
                return await Task.Run(() => { return status = Status.LinkExpired; });
            }
            else
            {
                var actionID = int.Parse(emailAction.ActionID);
                var leave = _opsSrvice.First<PersonLeave>(t => t.ID == actionID);
                if (leave.Status == 1)
                {
                    leave.Narration = leave.Narration + " <br><b>" + action.sentanceCase() + " via email.</b>";
                    var model = Mapper.Map<PersonLeave, EmployeeLeaveViewModel>(leave);
                    model.UserId = leave.Person.ID;
                    model.Status = action == "approved" ? 2 : 3;
                    var approver = _opsSrvice.First<PersonEmployment>(t => t.OrganizationEmail == email);
                    if (approver == null)
                        return await Task.Run(() => { return status = Status.InvalidUser; });
                    else
                    {
                        var data = await _empLeaveService.ApproveOrRejectLeave(approver.Person.ID, model, 0);
                        await Task.Run(() => { return status = model.Status == 2 ? Status.Approved : Status.Rejected; });
                        emailAction.ActionOn = DateTime.Now;
                        emailAction.isAccessed = true;
                        _opsSrvice.Update<EmailAction>(emailAction);
                        _opsSrvice.Finalize(true);
                    }
                }
                else
                {
                    if (leave.Status == 2)
                    {
                        return status = Status.AlreadyApproved;
                    }
                    else if (leave.Status == 3) { return status = Status.AlreadyRejected; } else { return status = Status.Deleted; }
                }
            }
            return status;
        }

        public async Task<string> ExpenseApprovalViaEmail(Guid authKey, string action, string email)
        {
            var emailAction = _opsSrvice.First<EmailAction>(x => x.AuthKey == authKey);
            bool result = false;

            if (emailAction == null)
            {
                return await Task.Run(() => { return "Invalid Request."; });
            }
            else if (emailAction.ActionID != null && emailAction.isAccessed == true)
            {
                return await Task.Run(() => { return "The Link has been expired."; });
            }
            else
            {
                int requestId = Convert.ToInt32(emailAction.ActionID);
                var firstApprover = _opsSrvice.First<ApprovalDetail>(x => x.Approval.RequestType == 3 && x.Approval.RequestID == requestId && x.Stage == 1 && x.Status == 0);
                if (firstApprover == null)
                {
                    return await Task.Run(() => { return "Invalid Request."; });
                }

                if (action == "approved")
                    result = await _expenseService.ApproveExpense(Convert.ToInt32(emailAction.ActionID), "", "", firstApprover.ApproverID.Value);
                else if (action == "rejected")
                    result = await _expenseService.RejectExpense(new ExpenseViewModel() { expenseId = Convert.ToInt32(emailAction.ActionID), comments = "Approved" }, firstApprover.ApproverID.Value);

                if (result)
                {
                    emailAction.ActionOn = DateTime.Now;
                    emailAction.isAccessed = true;
                    _opsSrvice.Update<EmailAction>(emailAction);
                    _opsSrvice.Finalize(true);

                    return await Task.Run(() => { return "Operation Successful"; });
                }
                else { return await Task.Run(() => { return "Operation Failed"; }); }

            }

        }

    }
}

public enum Status
{
    Approved,
    Rejected,
    Deleted,
    LinkExpired,
    InvalidAuthKey,
    InvalidUser,
    AlreadyApproved,
    AlreadyRejected,
}


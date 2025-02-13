using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Pheonix.Core.v1.Services
{
    public interface IApprovalService
    {
        List<ApprovalsViewModel> ListAllApprovalsFor(int id);
        int SendForApproval(int currentUserID, int requestType, int requestId, int[] approvers, int componentID = 0);
        //int SetStatus(int approverID, )
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Approval
{
    public interface IEmailActionsService
    {
        Task<Status> LeaveActionViaEmail(Guid authKey, string action, string email);
        Task<string> ExpenseApprovalViaEmail(Guid authKey, string action, string email);
    }
}

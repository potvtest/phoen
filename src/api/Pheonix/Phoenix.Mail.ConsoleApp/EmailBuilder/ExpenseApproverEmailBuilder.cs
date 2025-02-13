using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailBuilder
{
    public class ExpenseApproverEmailBuilder : EmailBuilder
    {
        private IBasicOperationsService _service;
        private Emails _email;

        public ExpenseApproverEmailBuilder(IBasicOperationsService service, Emails email)
        {
            this._service = service;
            this._email = email;
        }

        public override System.Net.Mail.MailMessage[] Build()
        {
            const int expenseRequestType = 3;

            int requestid = Convert.ToInt32(_email.EmailAction);
            var emailUrl = ConfigurationManager.AppSettings["email-action-url"];
            MailMessage[] messages = new MailMessage[2];

            var approverDetail = _service.Top<ApprovalDetail>(0, x => x.Approval.RequestType == expenseRequestType && x.Approval.RequestID.Value == requestid);
            var firstApprover = approverDetail.Where(x => x.Stage == 1).First();

            if (firstApprover.Status == 0)
            {
                messages[0] = CreateExpenseApproverEmail(Guid.NewGuid(), firstApprover.ApproverID.Value, emailUrl);
            }
            else { messages[0] = CreateBasicEmail(_email.EmailTo, _email.EmailFrom, _email.EmailCC, _email.Subject, _email.Content); }

            messages[1] = CreateBasicEmail(_email.EmailFrom, _email.EmailFrom, _email.EmailCC, _email.Subject, _email.Content);

            return messages;
        }

        private MailMessage CreateExpenseApproverEmail(Guid authKey, int approverId, string mailUrl)
        {

            var base64Email = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_email.EmailTo));
            var message = CreateBasicEmail(_email.EmailTo, _email.EmailFrom, _email.EmailCC, _email.Subject, _email.Content);
            message.Body += "<div> <a href='" + string.Format(mailUrl, "approved", authKey, base64Email, EnumHelpers.EmailTemplateType.ExpenseApproval) + "'>Approve</a> | <a href='" + string.Format(mailUrl, "rejected", authKey, base64Email, EnumHelpers.EmailTemplateType.ExpenseApproval) + "'>Reject</a> </div>";

            var emailAction = new EmailAction();
            emailAction.isAccessed = false;
            emailAction.CreatredOn = DateTime.Now;
            emailAction.AuthKey = authKey;
            emailAction.ApproverId = approverId;
            emailAction.ActionID = _email.EmailAction;
            _service.Create<EmailAction>(emailAction, null);
            _service.Finalize(true); ;

            return message;
        }
    }
}

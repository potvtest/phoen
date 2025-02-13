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
    public class LeaveAppliedEmailBuilder : EmailBuilder
    {
        private IBasicOperationsService _service;
        private Emails _email;


        public LeaveAppliedEmailBuilder(IBasicOperationsService service, Emails email)
        {
            this._service = service;
            this._email = email;
        }

        public override MailMessage[] Build()
        {
            var emailUrl = ConfigurationManager.AppSettings["email-action-url"];
            MailMessage[] messages = new MailMessage[2];
            var approverId = _service.First<PersonEmployment>(t => t.OrganizationEmail == _email.EmailTo).PersonID.Value;
            messages[0] = CreateApproverEmail(Guid.NewGuid(), approverId, emailUrl);
            messages[1] = CreateBasicEmail(_email.EmailFrom, string.Empty, _email.EmailCC, _email.Subject, _email.Content);
            messages[1].Body = messages[1].Body.Replace("{{approvalsection}}", "");
            messages[1].Body = messages[1].Body.Replace("{{approvalstatement}}", "");
            messages[1].Body = messages[1].Body.Replace("{{refering}}", "You");

            return messages;
        }

        private MailMessage CreateApproverEmail(Guid authKey, int approverId, string mailUrl)
        {

            var base64Email = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_email.EmailTo));
            var message = CreateBasicEmail(_email.EmailTo, string.Empty, string.Empty, _email.Subject, _email.Content);
            string templateFor = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Leave));

            string approvalHtml = _service.First<EmailTemplate>(x => x.TemplateFor == templateFor).OptionalVariables;

            approvalHtml = approvalHtml.Replace("{{rejecturl}}", string.Format(mailUrl, "rejected", authKey, base64Email, EnumHelpers.EmailTemplateType.Leave));
            approvalHtml = approvalHtml.Replace("{{approvalurl}}", string.Format(mailUrl, "approved", authKey, base64Email, EnumHelpers.EmailTemplateType.Leave));
            message.Body = message.Body.Replace("{{approvalsection}}", approvalHtml);
            message.Body = message.Body.Replace("{{approvalstatement}}", "Do review and approve my leave.");
            message.Body = message.Body.Replace("{{refering}}", "I");

            //message.Body += "<div> <a href='" + string.Format(mailUrl, "approved", authKey, base64Email) + "'>Approve</a> | <a href='" + string.Format(mailUrl, "rejected", authKey, base64Email) + "'>Reject</a> </div>";

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

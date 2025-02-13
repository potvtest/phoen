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
    public class DefaultEmailBuilder : EmailBuilder
    {
        private IBasicOperationsService _service;
        private Emails _email;


        public DefaultEmailBuilder(IBasicOperationsService service, Emails email)
        {
            this._service = service;
            this._email = email;
        }

        public override MailMessage[] Build()
        {
            MailMessage[] messages = new MailMessage[1];
            List<string> dp_EmailIdentifiers = Convert.ToString(ConfigurationManager.AppSettings["Email-Attachment-Identifier"]).Split(',').ToList();
            messages[0] = CreateBasicEmail(_email.EmailTo, _email.EmailFrom, _email.EmailCC, _email.Subject, _email.Content);

            if (!string.IsNullOrEmpty(_email.Attachments) &&  !string.IsNullOrEmpty(_email.Subject) && dp_EmailIdentifiers.Any(s => _email.Subject.StartsWith(s)))
            {

                messages[0] = CreateBasicEmail(_email.EmailTo, _email.EmailFrom, _email.EmailCC, _email.Subject, _email.Content, _email.Attachments);
            }
            else
            {
                messages[0] = CreateBasicEmail(_email.EmailTo, _email.EmailFrom, _email.EmailCC, _email.Subject, _email.Content);
            }

            return messages;
        }
    }
}

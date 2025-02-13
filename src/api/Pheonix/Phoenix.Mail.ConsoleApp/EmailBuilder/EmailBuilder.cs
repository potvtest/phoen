using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailBuilder
{
    public abstract class EmailBuilder
    {
        public abstract MailMessage[] Build();

        public MailMessage CreateBasicEmail(string to, string from, string cc, string subject, string body, bool isHtml = true)
        {
            string helpdeskEmailId = ConfigurationManager.AppSettings["helpdeskEmailId"].ToString();

            var message = new MailMessage();
            string[] toList = to.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (from != string.Empty && !from.Contains("old_"))
                message.CC.Add(from);

            message.To.Add(string.Join(", ", toList));
            message.From = new MailAddress(helpdeskEmailId);
            if (cc != null && cc != string.Empty)
                message.CC.Add(cc);
            message.Subject = subject;
            message.IsBodyHtml = isHtml;
            message.Body = body;

            return message;
        }

        public MailMessage CreateBasicEmail(string to, string from, string cc, string subject, string body, string attachments, bool isHtml = true)
        {
            var message = CreateBasicEmail(to, from, cc, subject, body, isHtml);
            if (!string.IsNullOrEmpty(attachments))
            {
                var attachmentsArray = attachments.Split(';');
                foreach (var attachmentPath in attachmentsArray)
                {
                    //string reportFolder = GetReportPath();
                    Attachment attachment = new Attachment(attachmentPath);
                    message.Attachments.Add(attachment);
                }
            }

            return message;
        }

        /// <summary>
        /// Get reports folder path
        /// </summary>
        /// <returns></returns>
        //string GetReportPath()
        //{
        //    string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPathAllocation"]);
        //    if (!Directory.Exists(path))
        //        Directory.CreateDirectory(path);
        //    return path;
        //}

    }

}

using Microsoft.Practices.Unity;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Linq;
using System.Collections.Generic;
using EmailBuilder;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.Net;

namespace Phoenix.Mail.ConsoleApp
{
    public class MailSender
    {
        private static IUnityContainer _container;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new SmtpClient();
                _container = UnityRegister.LoadContainer();
                var worker = _container.Resolve<BasicOperationsService>(); // I don't like this
                var emailsList = worker.Top<Emails>(0, x => x.IsSent == false);
                List<MailMessage> messages = new List<MailMessage>();

                foreach (var email in emailsList.ToList())
                {
                    messages = new List<MailMessage>();
                    try
                    {
                        var builder = EmailBuilderFactory.Get(worker, email);
                        if (builder != null)
                            messages.AddRange(builder.Build());

                        using (PhoenixEntities context = new PhoenixEntities())
                        {
                            var updatedEmails = context.Emails.Where(x => x.Id == email.Id).First();
                            updatedEmails.IsSent = true;

                            foreach (var message in messages)
                            {
                                client.Send(message);
                            }

                            context.SaveChanges();
                        }
                    }
                    catch (SmtpException ex)
                    {
                        Log4Net.Error("Email not sent for Id: " + email.Id);
                        Log4Net.Error("Smtp Exception Message: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Log4Net.Error("Email not sent for Id: " + email.Id);
                        Log4Net.Error("Exception Message: " + ex.Message);
                    }
                }

                client.Dispose();
            }
            catch (SqlException ex)
            {
                Log4Net.Error("Sql Exception Message: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
            }
        }
    }
}
using log4net;
using Pheonix.Core.Services.Confirmation;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace AttendanceJob.Jobs
{
    public class ConfirmationSchedule : IJob
    {
        public IConfirmationService _Service;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ConfirmationSchedule(IBasicOperationsService service, IPrintReportInPDF printReport)
        {
            _Service = new ConfirmationService(new ContextRepository<PhoenixEntities>(new PhoenixEntities()), new EmailSendingService(service), new BasicOperationsService(new ContextRepository<PhoenixEntities>(new PhoenixEntities())), printReport, new ApprovalService(service));
        }
        public ConfirmationSchedule(IConfirmationService service)
        {
            _Service = service;
        }

        public void RunJob()
        {
            _Service.Initiate();
            //if (ConfigurationManager.AppSettings["isAutoConfirm"].ToString() == "true")
            //    _Service.AutoConfirmEmployee();

            _Service.SendConfirmationReminderMail();
        }
    }
}

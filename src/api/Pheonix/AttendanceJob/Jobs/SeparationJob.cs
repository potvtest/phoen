using log4net;
using Pheonix.Core.Repository;
using Pheonix.Core.Repository.SeparationCard;
using Pheonix.Core.v1;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.Email;
using Pheonix.Core.v1.Services.Seperation;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceJob.Jobs
{
    public class SeparationJob : IJob
    {
        public ISeparationCardService _Service;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SeparationJob(IBasicOperationsService service)
        {
            var basicService = new BasicOperationsService(new ContextRepository<PhoenixEntities>(new PhoenixEntities()));
            var emailService = new EmailSendingService(service);
            _Service = new SeparationCardService(new SeparationCardRepository(), new ApprovalService(service), emailService, basicService); //,new BasicOperationsService(new ContextRepository<PhoenixEntities>(new PhoenixEntities())
        }

        public SeparationJob(ISeparationCardService service)
        {
            _Service = service;
        }

        public void RunJob()
        {
            _Service.InitiateSeparationProcess(0);
           // _Service.SendSeparationReminderMail();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Pheonix.Core;
using log4net;
using Pheonix.Core.v1.Services.syncATS;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using System.Configuration;

namespace AttendanceJob.Jobs
{
    public class SyncATSJob : IJob
    {
            public ISyncAts _Service;
            private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SyncATSJob(IBasicOperationsService service)
        {
            var basicService = new BasicOperationsService(new ContextRepository<PhoenixEntities>(new PhoenixEntities()));
            _Service = new SyncAts(); //,new BasicOperationsService(new ContextRepository<PhoenixEntities>(new PhoenixEntities())
        }

        public SyncATSJob(ISyncAts service)
        {
            _Service = service;
        }

        public void RunJob()
        {
            string ListUrl = ConfigurationManager.AppSettings["ListURL"].ToString();
            string DetailUrl = ConfigurationManager.AppSettings["DetailURL"].ToString();
            string clientID = ConfigurationManager.AppSettings["ClientID"].ToString();
            string clSecret = ConfigurationManager.AppSettings["ClientSecret"].ToString();
            int dayMargin = int.Parse(ConfigurationManager.AppSettings["DayMargin"].ToString());


            _Service.InitiateEmployeeSynchronization(ListUrl, DetailUrl, clientID, clSecret, dayMargin);
            // _Service.SendSeparationReminderMail();
        }
    }
}

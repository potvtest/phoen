using AttendanceJob.JobStrategy;
using log4net;
using Microsoft.Practices.Unity;
using Pheonix.Core.Services.Confirmation;
using Pheonix.Core.v1.Services;
using Pheonix.Web;
using Pheonix.Web.Report;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AttendanceJob
{
    class Program
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;

        static void Main(string[] args)
        {
            _container = UnityRegister.LoadContainer();

            var opsService = _container.Resolve<BasicOperationsService>();
            var reportService = _container.Resolve<ReportPrinting>();

            Log4Net.Debug("Attendance job started: =" + DateTime.Now);

            List<IJobStrategy> jobs = new List<IJobStrategy>();

            //jobs.Add(new SelectJobStrategy(AttendanceJobType.BasicAttendance));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.CompOff));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.ExpiredCompOff));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.LeaveAttendance));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.LeaveAutoApproval));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.Confirmation, opsService, reportService));
            jobs.Add(new SelectJobStrategy(AttendanceJobType.Separation, opsService));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.SyncATS, opsService));
            //jobs.Add(new SelectJobStrategy(AttendanceJobType.ClosePendingApprovalRequestEmail, opsService, reportService));

           // if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
              //  jobs.Add(new SelectJobStrategy(AttendanceJobType.ApprovalSummary, opsService, reportService));

            foreach (var item in jobs)
            {
                item.RunJob();
            }

            Log4Net.Debug("Attendance job finished: =" + DateTime.Now);
        }
    }
}


public enum AttendanceJobType
{
    BasicAttendance,
    CompOff,
    LeaveAttendance,
    ExpiredCompOff,
    LeaveAutoApproval,
    ApprovalSummary,
    Separation,
    Confirmation,
    ClosePendingApprovalRequestEmail,
    SyncATS
}
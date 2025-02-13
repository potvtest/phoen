using AttendanceJob.Jobs;
using Pheonix.Core.Services.Confirmation;
using Pheonix.Core.v1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceJob.JobStrategy
{
    public class SelectJobStrategy : IJobStrategy
    {
        private IJob SelectJob
        {
            get;
            set;
        }

        private IBasicOperationsService _service;
        private IPrintReportInPDF _PrintReport;

        public SelectJobStrategy(AttendanceJobType type, IBasicOperationsService service = null, IPrintReportInPDF printReport = null)
        {
            _service = service;
            _PrintReport = printReport;

            switch (type)
            {
                case AttendanceJobType.BasicAttendance:
                    {
                        SelectJob = new BasicAttendance();
                        break;
                    }
                case AttendanceJobType.CompOff:
                    {
                        SelectJob = new CompOff();
                        break;
                    }
                case AttendanceJobType.LeaveAttendance:
                    {
                        SelectJob = new LeaveAtendance();
                        break;
                    }
                case AttendanceJobType.ExpiredCompOff:
                    {
                        SelectJob = new ExpiredCompOff();
                        break;
                    }
                case AttendanceJobType.LeaveAutoApproval:
                    {
                        SelectJob = new LeaveAutoApproval();
                        break;
                    }
                case AttendanceJobType.ApprovalSummary:
                    {
                        SelectJob = new ApprovalSummaryEmail(_service);
                        break;
                    }
                case AttendanceJobType.Separation:
                    {
                        SelectJob = new SeparationJob(_service);
                        break;
                    }
                case AttendanceJobType.Confirmation:
                    {
                        SelectJob = new ConfirmationSchedule(_service, _PrintReport);
                        break;
                    }
                case AttendanceJobType.ClosePendingApprovalRequestEmail:
                    {
                        SelectJob = new ClosePendingApprovalRequestEmail(_service);
                        break;
                    }
                case AttendanceJobType.SyncATS:
                    {
                        SelectJob = new SyncATSJob(_service);
                        break;
                    }

            }
        }

        public void RunJob()
        {
            SelectJob.RunJob();
        }
    }
}

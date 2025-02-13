using Pheonix.Core.v1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceJob.Jobs
{
    public interface IJob
    {
        void RunJob();
    }
}

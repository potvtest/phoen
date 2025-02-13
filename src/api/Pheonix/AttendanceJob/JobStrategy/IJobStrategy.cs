using AttendanceJob.Jobs;
using Pheonix.Core.v1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceJob.JobStrategy
{
    public interface IJobStrategy
    {
        void RunJob();
    }
}

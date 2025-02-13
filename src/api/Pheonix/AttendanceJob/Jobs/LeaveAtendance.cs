using log4net;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceJob.Jobs
{
    public class LeaveAtendance : IJob, IDisposable
    {
        PhoenixEntities entites = null;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LeaveAtendance()
        {
            entites = new PhoenixEntities();
        }

        public void RunJob()
        {
            try
            {
                entites.PresentOnLeave();
                Dispose();
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
                Dispose();
            }
        }

        public void Dispose()
        {
            entites.Dispose();
        }
    }
}

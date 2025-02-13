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
    public class ExpiredCompOff : IJob, IDisposable
    {
        PhoenixEntities entites = null;
        DateTime todaysDate = DateTime.Now.Date;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ExpiredCompOff()
        {
            entites = new PhoenixEntities();
        }

        public void RunJob()
        {
            try
            {
                #region OldCodelogic                
                //var allExpiredCompOffs = (from a in entites.CompOff
                //                          where a.ExpiresOn.Value == todaysDate
                //                          select a).ToList();

                //foreach (var item in allExpiredCompOffs)
                //{
                //    switch (item.Status)
                //    {
                //        case (int)CompOffStages.Applied:
                //            {
                //                item.Status = (int)CompOffStages.Deleted;
                //                break;
                //            }

                //        case (int)CompOffStages.Approved:
                //            {
                //                var compOffExist = entites.PersonLeaveLedger.Where(l => l.PersonID.Value == item.PersonID.Value).FirstOrDefault();
                //                compOffExist.CompOffs = compOffExist.CompOffs.Value > 0 ? compOffExist.CompOffs.Value - 1 : compOffExist.CompOffs;
                //                item.Status = (int)CompOffStages.Deleted;
                //                break;
                //            }

                //        case (int)CompOffStages.Rejected:
                //            {
                //                item.Status = (int)CompOffStages.Deleted;
                //                break;
                //            }
                //    }
                //}
                //entites.SaveChanges();
                #endregion

                entites.ExpiredCompOff();
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

public enum CompOffStages
{
    Applied,
    Approved,
    Rejected,
    Deleted
}
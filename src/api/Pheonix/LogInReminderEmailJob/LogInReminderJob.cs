using log4net;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace LogInReminderEmailJob
{
    public class LogInReminderJob : ILogInReminderJobService
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;
        public void RunJob()
        {
            try
            {
                _container = UnityRegister.LoadContainer();
                var _dbServices = _container.Resolve<PhoenixEntities>();
                // SP CALL Will be here

                DateTime currentDateTime = DateTime.Now;

                try
                {
                    Log4Net.Debug("Log In Reminder notification job started: =" + DateTime.Now);
                    try
                    {
                       _dbServices.SendReminderForLogIn();
                       Log4Net.Debug("Stored Procedure executed successfully.");            
                    }
                    catch (Exception ex)
                    {
                        Log4Net.Error("Error occured in the LogInReminderJob RunJob() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception in Main method Exception Message: " + ex.StackTrace);
                }
                Log4Net.Debug("Log In Reminder Email job finished: =" + DateTime.Now);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the KRAHistoryDataReminderJob RunJob() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }
    }
}

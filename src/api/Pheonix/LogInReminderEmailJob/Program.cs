using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;

namespace LogInReminderEmailJob
{
    class Program
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;
        static void Main(string[] args)
        {
           _container = UnityRegister.LoadContainer();


            ILogInReminderJobServiceBase service = LogInReminderJobServiceFactory.GetService<LogInReminderJob>(_container);
            service.RunJob();
        }
    }
}

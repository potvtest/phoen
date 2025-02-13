using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInReminderEmailJob
{
    public interface ILogInReminderJobServiceBase
    {
        void RunJob();
    }
}

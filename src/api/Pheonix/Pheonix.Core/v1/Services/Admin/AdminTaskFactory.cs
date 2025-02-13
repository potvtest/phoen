using Pheonix.Models.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Admin
{
    public class AdminTaskFactory
    {
        private static readonly Dictionary<AdminTaskType, Func<IAdminTask>> _tasks;

        static AdminTaskFactory()
        {
            _tasks = new Dictionary<AdminTaskType, Func<IAdminTask>>();

            _tasks.Add(AdminTaskType.SignInSignOut, () =>
            {
                return new AdminSignInSignOutTask();
            });
            _tasks.Add(AdminTaskType.CompOff, () =>
            {
                return new AdminCompOffTask();
            });
            _tasks.Add(AdminTaskType.Leaves, () =>
            {
                return new AdminLeaveTask();
            });
            _tasks.Add(AdminTaskType.BulkSISO, () =>
            {
                return new AdminBulkSISO();
            });

        }

        public static IAdminTask InitAdminTask(AdminTaskType taskType)
        {
            return _tasks[taskType]();
        }
    }


}

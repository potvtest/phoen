using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.Models.AdminConfig;

namespace Pheonix.Core.v1.Services.AdminConfig
{
    public class AdminConfigTaskFactory
    {
        private static readonly Dictionary<AdminConfigTaskType, Func<IAdminConfigTask>> _tasks;

        static AdminConfigTaskFactory()
        {
            _tasks = new Dictionary<AdminConfigTaskType, Func<IAdminConfigTask>>();

            _tasks.Add(AdminConfigTaskType.HolidayList, () =>
            {
                return new AdminHolidayListTask();
            });
            _tasks.Add(AdminConfigTaskType.Leaves, () =>
            {
                return new AdminLeavesTask();
            });
            _tasks.Add(AdminConfigTaskType.Locations, () =>
            {
                return new AdminLocationTask();
            });
            //_tasks.Add(AdminConfigTaskType.BGC, () =>
            //{
            //    return new AdminBGCTask();
            //});
            _tasks.Add(AdminConfigTaskType.VCF, () =>
            {
                return new AdminVCFListTask();
            });
            _tasks.Add(AdminConfigTaskType.VCFApprover, () =>
            {
                return new AdminVCFApproverTask();
            });
            _tasks.Add(AdminConfigTaskType.DeliveryTeam, () =>
            {
                return new AdminDeliveryTeamTask();
            });
            _tasks.Add(AdminConfigTaskType.ResourcePool, () =>
            {
                return new AdminResourcePoolTask();
            });
            _tasks.Add(AdminConfigTaskType.VCFGlobalApprover, () =>
            {
                return new AdminVCFGlobalApproverTask();
            });
            _tasks.Add(AdminConfigTaskType.Skills, () =>
            {
                return new AdminSkillsTask();
            });
            _tasks.Add(AdminConfigTaskType.TaskType, () =>
            {
                return new AdminTaskType();
            });
        }

        public static IAdminConfigTask InitAdminTask(AdminConfigTaskType taskType)
        {
            return _tasks[taskType]();
        }
    }
}

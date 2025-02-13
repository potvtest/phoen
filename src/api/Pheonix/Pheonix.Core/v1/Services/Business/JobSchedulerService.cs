using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pheonix.DBContext;
using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services.Business
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly IBasicOperationsService _service;

        #region Constructor

        public JobSchedulerService(IBasicOperationsService service)
        {
            _service = service;
        }

        #endregion Constructor

        public async Task<IEnumerable<JobSchedulerViewModel>> GetList(int yearId)
        {
            var rtnViewModel = new List<JobSchedulerViewModel>
            {
                await GetSchedulerType(yearId, "SISO-Reminder"),
                await GetSchedulerType(yearId, "SISO-Auto-Approval")
            };

            return await Task.FromResult(rtnViewModel).ConfigureAwait(false);
        }

        private async Task<JobSchedulerViewModel> GetSchedulerType(int yearId, string SchedulerType)
        {
            var viewModelDtls = new List<JobSchedulerDetails>();

            try
            {
                using (var _phoenixEntity = new PhoenixEntities())
                {
                    var viewModel = _phoenixEntity.JobScheduler.Where(x => x.SchedulerDate.Year == yearId && x.SchedulerType == SchedulerType).ToList();
                    foreach (var item in viewModel)
                    {
                        viewModelDtls.Add(new JobSchedulerDetails
                        {
                            SchedulerId = item.SchedulerId,
                            MonthId = item.SchedulerDate.Month,
                            SchedulerDate = item.SchedulerDate,
                            StatusType = item.StatusType,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return await Task.FromResult(new JobSchedulerViewModel
            {
                SchedulerType = SchedulerType,
                Details = viewModelDtls
            }).ConfigureAwait(false);
        }

        public async Task<bool> SaveOrUpdate(IEnumerable<JobSchedulerViewModel> _viewModel)
        {
            bool rtnStatus = false;
            try
            {
                SaveOrUpdateByType("SISO-Reminder", _viewModel);
                SaveOrUpdateByType("SISO-Auto-Approval", _viewModel);
                rtnStatus = true;
            }
            catch (Exception)
            {

            }

            return await Task.FromResult(rtnStatus).ConfigureAwait(false);
        }

        private void SaveOrUpdateByType(string schedulerType, IEnumerable<JobSchedulerViewModel> _viewModel)
        {
            var _reminderModel = _viewModel.Where(x => x.SchedulerType == schedulerType).FirstOrDefault();

            foreach (var item in _reminderModel.Details)
            {
                bool isTaskCreated = false;
                var model = new JobScheduler
                {
                    SchedulerId = item.SchedulerId,
                    SchedulerType = _reminderModel.SchedulerType,
                    SchedulerDate = item.SchedulerDate,
                    StatusType = item.StatusType
                };

                if (model.SchedulerId != 0)
                    isTaskCreated = _service.Update<JobScheduler>(model);
                else
                    isTaskCreated = _service.Create<JobScheduler>(model, x => x.SchedulerId == model.SchedulerId);

                if (isTaskCreated)
                    _service.Finalize(true);
            }
        }
    }
}

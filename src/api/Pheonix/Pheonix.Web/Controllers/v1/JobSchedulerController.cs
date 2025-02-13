using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/jobscheduler")]
    public class JobSchedulerController : ApiController
    {
        private readonly IJobSchedulerService _jobSchedulerService;

        public JobSchedulerController(IJobSchedulerService jobSchedulerService)
        {
            _jobSchedulerService = jobSchedulerService;
        }

        [HttpGet, Route("getlist/{yearId:int?}")]
        public async Task<IEnumerable<JobSchedulerViewModel>> GetJobScheduler(int yearId)
            => await _jobSchedulerService.GetList(yearId);

        [HttpPost, Route("save-update")]
        public async Task<bool> SaveOrUpdate(IEnumerable<JobSchedulerViewModel> _viewModel)
            => await _jobSchedulerService.SaveOrUpdate(_viewModel);
    }
}
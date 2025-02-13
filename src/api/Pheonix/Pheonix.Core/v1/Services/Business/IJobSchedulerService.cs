using System.Collections.Generic;
using System.Threading.Tasks;
using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IJobSchedulerService
    {
        Task<IEnumerable<JobSchedulerViewModel>> GetList(int yearId);
        Task<bool> SaveOrUpdate(IEnumerable<JobSchedulerViewModel> _viewModel);
    }
}
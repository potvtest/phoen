using Pheonix.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ICelebreationListService
    {
        Task<IEnumerable<CelebrationListViewModel>> GetCelebrationList();
    }
}

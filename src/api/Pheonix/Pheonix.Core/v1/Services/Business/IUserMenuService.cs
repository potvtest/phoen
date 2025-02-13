using Pheonix.Models.VM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IUserMenuService : IDataService
    {
        Task<IEnumerable<UserMenuViewModel>> GetUserMenu(int roleId, int userId);
    }
}
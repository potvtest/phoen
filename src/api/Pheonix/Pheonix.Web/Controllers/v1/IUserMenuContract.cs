using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Web.Controllers.v1
{
    public interface IUserMenuContract
    {
        Task<IEnumerable<UserMenuViewModel>> UserMenu();
    }
}

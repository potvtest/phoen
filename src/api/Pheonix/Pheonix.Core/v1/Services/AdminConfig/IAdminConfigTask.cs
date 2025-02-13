using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.AdminConfig;

namespace Pheonix.Core.v1.Services.AdminConfig
{
    public interface IAdminConfigTask
    {
        AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model);        

        AdminConfigActionModel GetList(IContextRepository repo, AdminConfigActionModel model);
    }
}

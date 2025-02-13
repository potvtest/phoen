using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Admin
{
    public interface IAdminTask
    {

        AdminActionResult TakeActionOn(IContextRepository repo, AdminActionModel model);

        AdminActionResult Delete(IContextRepository repo, AdminActionModel model);
    }
}

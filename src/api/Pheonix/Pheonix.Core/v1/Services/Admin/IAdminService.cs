using Pheonix.Models.Models.Admin;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Threading.Tasks;
namespace Pheonix.Core.v1.Services.Admin
{
    public interface IAdminService
    {
        AdminActionResult TakeActionOn(AdminActionModel model);

        AdminActionResult Delete(AdminActionModel model);

        

    }
}

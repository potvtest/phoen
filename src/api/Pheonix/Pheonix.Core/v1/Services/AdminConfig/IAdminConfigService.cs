using Pheonix.Models.Models.AdminConfig;
using Pheonix.Models.Models.Admin;


namespace Pheonix.Core.v1.Services.AdminConfig
{
    public interface IAdminConfigService
    {          
        AdminActionResult TakeActionOn(AdminConfigActionModel model);    
        AdminConfigActionModel GetList(AdminConfigActionModel model);
    }
}

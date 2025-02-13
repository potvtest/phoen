using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Repository.PMS
{
    public interface IPMSRepository
    {
        Task<IEnumerable<PMSRolesViewModel>> GetPMSRoles();
        Task<IEnumerable<RoleViewModel>> GetOrganizationRoles();
        Task<IEnumerable<PMSActionViewModel>> GetPMSActions(int pmsRoleID, int orgRoleID);
        Task<PMSRolesViewModel> SavePMSRole(string name, int personID);
        Task<bool> SavePMSActions(List<PMSActionViewModel> list, int personID);
        List<int?> GetPMSActionsResult(int personID, int project);
    }
}

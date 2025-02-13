using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Core.Repository.PMS;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
namespace Pheonix.Core.v1.Services.PMS
{
    public class PMSService : IPMSService
    {
        private readonly IPMSRepository pmsRepo;
        public PMSService(IPMSRepository _pmsRepo)
        {
            this.pmsRepo = _pmsRepo;
        }
        public async Task<IEnumerable<RoleViewModel>> GetOrganizationRoles()
        {
            return await pmsRepo.GetOrganizationRoles();
        }

        public async Task<IEnumerable<PMSActionViewModel>> GetPMSActions(int pmsRoleID, int orgRoleID)
        {
            return await pmsRepo.GetPMSActions(pmsRoleID, orgRoleID);
        }

        public async Task<IEnumerable<PMSRolesViewModel>> GetPMSRoles()
        {
            return await pmsRepo.GetPMSRoles();
        }

        public async Task<bool> SavePMSActions(List<PMSActionViewModel> list, int personID)
        {
            return await pmsRepo.SavePMSActions(list, personID);
        }

        public async Task<PMSRolesViewModel> SavePMSRole(string name, int personID)
        {
            return await pmsRepo.SavePMSRole(name, personID);
        }

        public List<int?> GetPMSActionsResult(int personID, int project)
        {
            List<int?> actions = null;

            actions = pmsRepo.GetPMSActionsResult(personID, project);
            return actions;
        }
    }
}

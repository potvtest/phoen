using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;

namespace Pheonix.Core.Repository.PMS
{
    public class PMSRepository : IPMSRepository
    {
        private readonly PhoenixEntities _phoenixEntity;

        public PMSRepository()
        {
            _phoenixEntity = new PhoenixEntities();
        }
        public async Task<IEnumerable<RoleViewModel>> GetOrganizationRoles()
        {
            var orgRoles = new List<RoleViewModel>();
            List<int> roles = new List<int>();
            roles.Add(21);
            roles.Add(23);
            roles.Add(24);
            roles.Add(27);
            roles.Add(28);
            roles.Add(33);
            roles.Add(35);
            roles.Add(38);
            roles.Add(45);
            await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var data = _db.Role.Where(c => c.IsDeleted != true && c.IsTemporary != true && roles.Contains(c.ID)).ToList();
                    orgRoles = Mapper.Map<List<RoleViewModel>>(data);
                }
            });
            return orgRoles;
        }

        public async Task<IEnumerable<PMSActionViewModel>> GetPMSActions(int pmsRoleID, int orgRoleID)
        {
            var pmsActions = new List<PMSActionViewModel>();
            await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var act = _db.PMSAction.Where(c => c.IsDeleted != true).ToList();
                    pmsActions = Mapper.Map<List<PMSActionViewModel>>(act);
                    var result = _db.PMSRoleMapping.SingleOrDefault(c => c.PMSRoleID == pmsRoleID && c.OrgRoleID == orgRoleID);
                    if (result != null)
                    {
                        var roleMappingID = result.ID;
                        var mappedActions = _db.PMSActionMapping.Where(c => c.PMSRoleMapID == roleMappingID).Select(c => c.PMSAction).ToList();
                        if (mappedActions != null)
                        {
                            foreach (var item in pmsActions)
                            {
                                item.IsChecked = false;
                                item.PMSRoleMapID = roleMappingID;
                                if (mappedActions.Any(c => c.PMSActionID == item.PMSActionID))
                                {
                                    item.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            });
            return pmsActions;
        }

        public async Task<IEnumerable<PMSRolesViewModel>> GetPMSRoles()
        {
            var pmsRoles = new List<PMSRolesViewModel>();
            await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var result = _db.PMSRoles.Where(c => c.IsDeleted != true).ToList();
                    pmsRoles = Mapper.Map<List<PMSRolesViewModel>>(result);
                }
            });
            return pmsRoles;
        }

        public async Task<bool> SavePMSActions(List<PMSActionViewModel> list, int personID)
        {
            bool isOk = false;
            if (list != null && list.Count > 0)
            {
                var pmsRoleMapID = list.FirstOrDefault().PMSRoleMapID;
                var orgRoleID = list.FirstOrDefault().OrgRoleID;
                var pmsRoleID = list.FirstOrDefault().PMSRoleID;
                var actionMapping = new List<PMSActionMapping>();

                await Task.Run(() =>
                {
                    using (var _db = _phoenixEntity)
                    {
                        var isMappingExisist = _db.PMSRoleMapping.Any(c => c.PMSRoleID == pmsRoleID && c.OrgRoleID == orgRoleID);
                        if (!isMappingExisist)
                        {
                            PMSRoleMapping mapping = new PMSRoleMapping
                            {
                                PMSRoleID = pmsRoleID,
                                OrgRoleID = orgRoleID,
                                IsDeleted = false,
                                CreatedDate = DateTime.Now,
                                CreatedBy = 3942
                            };

                            _db.PMSRoleMapping.Add(mapping);
                            _db.SaveChanges();
                            pmsRoleMapID = mapping.ID;
                        }

                        foreach (var item in list)
                        {
                            var act = new PMSActionMapping
                            {
                                PMSRoleMapID = pmsRoleMapID,
                                ActionID = item.PMSActionID,
                                IsDeleted = false,
                                CreatedDate = System.DateTime.Now,
                                CreatedBy = personID
                            };
                            actionMapping.Add(act);
                        }

                        var data = _db.PMSActionMapping.Where(c => c.PMSRoleMapID == pmsRoleMapID).ToList();
                        _db.PMSActionMapping.RemoveRange(data);
                        _db.SaveChanges();
                        _db.PMSActionMapping.AddRange(actionMapping);
                        _db.SaveChanges();
                        isOk = true;
                    }
                });
            }
            return isOk;
        }

        public async Task<PMSRolesViewModel> SavePMSRole(string name, int personID)
        {
            var pmsRoleData = new PMSRolesViewModel();
            if (!string.IsNullOrEmpty(name))
            {
                await Task.Run(() =>
                {
                    using (var _db = _phoenixEntity)
                    {
                        var pmsRole = new PMSRoles
                        {
                            PMSRoleDescription = name,
                            CreatedBy = personID,
                            CreatedDate = System.DateTime.Now,
                            IsDeleted = false
                        };
                        _db.PMSRoles.Add(pmsRole);
                        _db.SaveChanges();
                        pmsRoleData = Mapper.Map<PMSRolesViewModel>(pmsRole);
                        pmsRoleData.PMSRoleID = pmsRole.PMSRoleID;
                    }
                });
            }
            return pmsRoleData;
        }

        public List<int?> GetPMSActionsResult(int personID, int project)
        {
            using (var db = _phoenixEntity)
            {
                int?[] result = null;
                result = db.getPMSActions(personID, project).ToArray();
                return result.ToList();
            }
        }
    }
}

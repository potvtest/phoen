using AutoMapper;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.AdminConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.AdminConfig
{
    public class AdminVCFGlobalApproverTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.vcfGlobalApproversList.Count > 0)
                {
                    foreach (VCFGlobalApproverModel value in model.vcfGlobalApproversList)
                    {

                        if (value.ID >= 0)
                        {
                            var existingVCFGApproverUser = repo.FindBy<PersonInRole>(x => x.ID == value.ID).FirstOrDefault();
                            if (existingVCFGApproverUser != null && value.IsDeleted)
                            {
                                existingVCFGApproverUser.IsDeleted = value.IsDeleted;
                                repo.Update<PersonInRole>(existingVCFGApproverUser);
                                result.message = string.Format("Global Approver User deleted successfully");
                            }
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            var existingVCFGApproverUser = repo.FindBy<PersonInRole>(x => x.PersonID == value.PersonID && x.RoleID == value.RoleID).FirstOrDefault();

                            if (existingVCFGApproverUser != null && value.IsDeleted ==false)
                            {
                                existingVCFGApproverUser.IsDeleted = value.IsDeleted;
                                repo.Update<PersonInRole>(existingVCFGApproverUser);
                            }
                            else
                            {
                                PersonInRole newVCFGApproverUser = new PersonInRole();
                                newVCFGApproverUser.PersonID = value.PersonID;
                                newVCFGApproverUser.RoleID = value.RoleID;
                                newVCFGApproverUser.IsDeleted = value.IsDeleted;
                                repo.Create(newVCFGApproverUser, null);
                            }
                            result.message = string.Format("Global Approver User added successfully");
                            result.isActionPerformed = true;
                        }
                        repo.Save();
                    }
                }
            }
            catch (Exception)
            {
                result.isActionPerformed = false;
                result.message = string.Format("operation Failed");
            }
            return result;
        }

        public AdminConfigActionModel GetList(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminConfigActionModel configmodel = new AdminConfigActionModel();
            List<PersonInRole> list = repo.GetAll<PersonInRole>().Where(x => x.IsDeleted == false && x.RoleID == 54).OrderByDescending(x => x.ID).ToList();
            configmodel.vcfGlobalApproversList = Mapper.Map<IEnumerable<PersonInRole>, List<VCFGlobalApproverModel>>(list);
            return configmodel;
        }
    }
}

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
    public class AdminVCFApproverTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.vcfApprover.Count > 0)
                {
                    foreach (VCFApproverModel value in model.vcfApprover)
                    {
                      
                         if (value.id >= 0)
                        {
                            var existingVCFBUApproverUser = repo.FindBy<VCFApprover>(x => x.id == value.id).FirstOrDefault();
                            if(existingVCFBUApproverUser != null && value.IsDeleted == 1)
                            {
                                existingVCFBUApproverUser.IsDeleted = value.IsDeleted;
                                repo.Update<VCFApprover>(existingVCFBUApproverUser);
                                result.message = string.Format("Business Unit Approver User deleted successfully");
                            }
                            else if (existingVCFBUApproverUser != null && value.IsDeleted == 0)
                            {
                                existingVCFBUApproverUser.DeliveryUnitID = value.DeliveryUnitID;
                                existingVCFBUApproverUser.ReviewerId = value.ReviewerId;
                                repo.Update<VCFApprover>(existingVCFBUApproverUser);
                                result.message = string.Format("Business Unit Approver User updated successfully");
                            }
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            var existingVCFBUApproverUser = repo.FindBy<VCFApprover>(x => x.ReviewerId == value.ReviewerId && x.DeliveryUnitID == value.DeliveryUnitID).FirstOrDefault();
                            if (existingVCFBUApproverUser != null && value.IsDeleted == 0)
                            {
                                existingVCFBUApproverUser.IsDeleted = value.IsDeleted;
                                repo.Update<VCFApprover>(existingVCFBUApproverUser);
                            }
                            else
                            {
                                VCFApprover VCFApprover = new VCFApprover();
                                VCFApprover.DeliveryUnitID = value.DeliveryUnitID;
                                VCFApprover.ReviewerId = value.ReviewerId;
                                VCFApprover.IsDeleted = value.IsDeleted;
                                repo.Create(VCFApprover, null);
                            }
                            result.message = string.Format("Business Unit Approver User added successfully");
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
            List<VCFApprover> list = repo.GetAll<VCFApprover>().Where(x => x.IsDeleted == 0).OrderByDescending(x => x.id).ToList();
            configmodel.vcfApprover = Mapper.Map<IEnumerable<VCFApprover>, List<VCFApproverModel>>(list);
            return configmodel;
        }
    }
}

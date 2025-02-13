using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.AdminConfig;
using AutoMapper;
using Pheonix.DBContext;


namespace Pheonix.Core.v1.Services.AdminConfig
{
    public class AdminVCFListTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            //var oldList = repo.GetAll<VCFConfigurationList>();
            //int i = oldList.Count();
            //if (i > 0)
            //{
            //    foreach (VCFConfigurationList vcf in oldList)
            //    {
            //        repo.HardRemove<VCFConfigurationList>(vcf, x => x.ID == vcf.ID);
            //    }
            //}

            try
            {
                if (model.VCFDetails.Count > 0)
                {
                    foreach (VCFListModel list in model.VCFDetails)
                    {
                        var oldVCF = repo.FindBy<VCFConfigurationList>(x => x.ID == list.ID).FirstOrDefault();
                        if (list.ID > 0 && oldVCF !=null)
                        {
                            
                            oldVCF.Name  =list.Name;
                            oldVCF.Description = list.Description;
                            repo.Update<VCFConfigurationList>(oldVCF);
                            result.message = string.Format("VCF Configuration updated successfully");
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            VCFConfigurationList lst = new VCFConfigurationList();

                            lst.Name = list.Name;
                            lst.Description = list.Description;
                            repo.Create(lst, null);
                            repo.Save();

                            result.isActionPerformed = true;
                            result.message = string.Format("VCF List added Successfully");
                        }
                    }                    
                }

            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("operation Failed");
            }



            return result;
        }

        public AdminConfigActionModel GetList(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminConfigActionModel configmodel = new AdminConfigActionModel();

            List<VCFConfigurationList> lst = repo.GetAll<VCFConfigurationList>().ToList();

            configmodel.VCFDetails = Mapper.Map<IEnumerable<VCFConfigurationList>, List<VCFListModel>>(lst);

            return configmodel;
        }
    }
}
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
    public class AdminResourcePoolTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.resourcePool.Count > 0)
                {
                    foreach (ResourcePoolModel value in model.resourcePool)
                    {
                          if (value.ID > 0)
                        {
                            var oldResourcePool = repo.FindBy<ResourcePool>(x => x.ID == value.ID).FirstOrDefault();
                            oldResourcePool.Description = value.Description;
                            oldResourcePool.IsDeleted = value.IsDeleted;
                            repo.Update<ResourcePool>(oldResourcePool);
                            result.message = string.Format("ResourcePool updated successfully");
                            result.isActionPerformed = true;
                        }
                        else {
                            ResourcePool resourcePool = new ResourcePool();
                            resourcePool.Code = value.Code;
                            resourcePool.Name = value.Name;
                            resourcePool.Description = value.Description;
                            resourcePool.IsDeleted = value.IsDeleted;

                            repo.Create(resourcePool, null);
                            result.message = string.Format("ResourcePool added successfully");
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

            List<ResourcePool> list = repo.GetAll<ResourcePool>().ToList();
            configmodel.resourcePool = Mapper.Map<IEnumerable<ResourcePool>, List<ResourcePoolModel>>(list);
            return configmodel;
        }
    }
}

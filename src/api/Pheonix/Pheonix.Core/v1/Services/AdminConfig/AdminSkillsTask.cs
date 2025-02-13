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
    public class AdminSkillsTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                if (model.skills.Count > 0)
                {
                    foreach (SkillsModel value in model.skills)
                    {
                        if (value.ID > 0)
                        {
                            var oldskills = repo.FindBy<SkillMatrix>(x => x.ID == value.ID).FirstOrDefault();                           
                            oldskills.Name = value.Name;
                            oldskills.Active = value.Active;
                            repo.Update<SkillMatrix>(oldskills);
                            result.message = string.Format("Skills updated successfully");
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            SkillMatrix oldskills = new SkillMatrix();
                            oldskills.Name = value.Name;
                            oldskills.Active = value.Active;
                            oldskills.SkillCategory = value.SkillCategory;
                            oldskills.IsDeleted = value.IsDeleted;
                            repo.Create(oldskills, null);
                            result.message = string.Format("Skills added successfully");
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
            List<SkillMatrix> list = repo.GetAll<SkillMatrix>().ToList();
            configmodel.skills = Mapper.Map<IEnumerable<SkillMatrix>, List<SkillsModel>>(list);
            return configmodel;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.AdminConfig;
using Pheonix.DBContext;
using AutoMapper;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Validation;

namespace Pheonix.Core.v1.Services.AdminConfig
{
    public class AdminLeavesTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var oldList = repo.GetAll<PhoenixConfig>().Where(x => x.Year == model.HolidayYear && x.Location == model.Location);
            int i = oldList.Count();
            if (i > 0)
            {
                foreach (PhoenixConfig config in oldList)
                {
                    repo.HardRemove<PhoenixConfig>(config, x => x.Year == model.HolidayYear && x.Location == model.Location);
                }
            }

            try
            {
                if (model.leaves.Count > 0)
                {
                    foreach (AdminLeaveConfigModel value in model.leaves)
                    {

                        PhoenixConfig config = new PhoenixConfig();

                        config.ConfigKey = value.ConfigKey;
                        config.ConfigValue = value.ConfigValue;
                        config.Description = value.Description;
                        config.Location = value.Location;
                        config.Active = value.Active;
                        config.IsDeleted = value.IsDeleted;
                        config.Year = value.Year;
                        repo.Create(config, null);
                        repo.Save();
                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Config Added Successfully");
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

            List<PhoenixConfig> list = repo.GetAll<PhoenixConfig>().Where(x => x.Year == model.HolidayYear && x.Location == model.Location).ToList();

            configmodel.leaves = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(list);

            return configmodel;
        }
    }
}

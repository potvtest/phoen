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
    public class AdminLocationTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.locations.Count > 0)
                {
                    foreach (LocationListModel value in model.locations)
                    {
                        if (value.IsDeleted == true)
                        {
                            var oldLocation = repo.FindBy<WorkLocation>(x => x.ID == value.ID).FirstOrDefault();
                            int PersonEmployment = repo.FindBy<PersonEmployment>(x => x.OfficeLocation == value.ID || x.WorkLocation == value.ID).Count();
                            if (PersonEmployment > 0)
                            {
                                result.message = string.Format("Location is been linked with Employee profile");
                                result.isActionPerformed = false;
                            }
                            else
                            {
                                repo.SoftRemove<WorkLocation>(oldLocation, x => x.ID == value.ID);
                                result.message = string.Format("Location deleted successfully");
                                result.isActionPerformed = true;
                            }

                        }
                        else if (value.ID >= 0)
                        {
                            var oldLocation = repo.FindBy<WorkLocation>(x => x.ID == value.ID).FirstOrDefault();
                            oldLocation.LocationName = value.LocationName;
                            oldLocation.ParentLocation = value.ParentLocation;
                            repo.Update<WorkLocation>(oldLocation);
                            result.message = string.Format("Location updated successfully");
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            WorkLocation location = new WorkLocation();
                            location.LocationName = value.LocationName;
                            location.ParentLocation = value.ParentLocation;
                            location.IsDeleted = value.IsDeleted;
                            location.CreatedBy = value.CreatedBy;
                            location.CreatedDate = value.CreatedDate;
                            repo.Create(location, null);
                            result.message = string.Format("Location added successfully");
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

            List<WorkLocation> list = repo.GetAll<WorkLocation>().Where(x => x.IsDeleted == false).ToList();
            configmodel.locations = Mapper.Map<IEnumerable<WorkLocation>, List<LocationListModel>>(list);
            return configmodel;
        }
    }
}

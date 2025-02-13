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
    //public class AdminBGCTask : IAdminConfigTask
    //{
        //public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        //{
        //    AdminActionResult result = new AdminActionResult();

        //    try
        //    {
        //        if (model.bgParameterList.Count > 0)
        //        {
        //            foreach (AdminBGCConfigModel value in model.bgParameterList)
        //            {
        //                if (value.IsDeleted == true)
        //                {
        //                    var oldBGParameter = repo.FindBy<BGParameterList>(x => x.ID == value.ID).FirstOrDefault();
        //                    int PersonBGMapping = repo.FindBy<PersonBGMapping>(x => x.BGParameterID == value.ID).Count();
        //                    if (PersonBGMapping > 0)
        //                    {
        //                        result.message = string.Format("BGC Params has been linked with Employee profile");
        //                        result.isActionPerformed = false;
        //                    }
        //                    else
        //                    {
        //                        repo.SoftRemove<BGParameterList>(oldBGParameter, x => x.ID == value.ID);
        //                        result.message = string.Format("BGC Params deleted successfully");
        //                        result.isActionPerformed = true;
        //                    }

        //                }
        //                else if (value.ID >= 0)
        //                {
        //                    var oldBGParameterList = repo.FindBy<BGParameterList>(x => x.ID == value.ID).FirstOrDefault();
        //                    oldBGParameterList.Name = value.Name;
        //                    oldBGParameterList.Description = value.Description;
        //                    repo.Update<BGParameterList>(oldBGParameterList);
        //                    result.message = string.Format("BGParameterList updated successfully");
        //                    result.isActionPerformed = true;
        //                }
        //                else
        //                {
        //                    BGParameterList bgParameterList = new BGParameterList();
        //                    bgParameterList.Name = value.Name;
        //                    bgParameterList.Description = value.Description;
        //                    bgParameterList.IsDeleted = value.IsDeleted;
        //                    bgParameterList.Active = true;
        //                    repo.Create(bgParameterList, null);
        //                    result.message = string.Format("BGParameterList added successfully");
        //                    result.isActionPerformed = true;
        //                }
        //                repo.Save();
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        result.isActionPerformed = false;
        //        result.message = string.Format("operation Failed");
        //    }

        //    return result;
        //}

        //public AdminConfigActionModel GetList(IContextRepository repo, AdminConfigActionModel model)
        //{
        //    AdminConfigActionModel configmodel = new AdminConfigActionModel();

        //    List<BGParameterList> list = repo.GetAll<BGParameterList>().Where(x => x.IsDeleted == false).ToList();
        //    configmodel.bgParameterList = Mapper.Map<IEnumerable<BGParameterList>, List<AdminBGCConfigModel>>(list);
        //    return configmodel;
        //}
    //}
}

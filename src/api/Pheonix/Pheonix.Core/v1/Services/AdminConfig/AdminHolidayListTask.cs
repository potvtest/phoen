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
    public class AdminHolidayListTask : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var oldList = repo.GetAll<HolidayList>().Where(x => x.HolidayYear == model.HolidayYear && x.Location == model.Location);
            int i = oldList.Count();
            if (i > 0)
            {
                foreach (HolidayList holiday in oldList)
                {
                    repo.HardRemove<HolidayList>(holiday, x => x.HolidayYear == model.HolidayYear && x.Location == model.Location);
                }
            }

            try
            {
                if (model.Details.Count > 0)
                {
                    foreach (HolidayListModel list in model.Details)
                    {
                        //this condition to be removed
                        if (list.Description != null)
                        {
                            HolidayList lst = new HolidayList();
                            lst.Date = list.Date;
                            lst.HolidayType = list.HolidayType;
                            lst.Description = list.Description;
                            lst.HolidayYear = model.HolidayYear;
                            lst.IsDeleted = false;
                            lst.Location = model.Location;

                            repo.Create(lst, null);
                            repo.Save();
                        }
                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Holiday List added Successfully");
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

            List<HolidayList> lst = repo.GetAll<HolidayList>().Where(x => x.HolidayYear == model.HolidayYear && x.Location == model.Location).ToList();

            configmodel.Details = Mapper.Map<IEnumerable<HolidayList>, List<HolidayListModel>>(lst);

            return configmodel;
        }
    }
}

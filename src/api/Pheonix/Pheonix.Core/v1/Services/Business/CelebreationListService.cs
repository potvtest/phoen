using Pheonix.Core.Helpers;
using Pheonix.DBContext;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Pheonix.Core.v1.Services.Business
{
    public class CelebreationListService : ICelebreationListService
    {
        public async Task<IEnumerable<CelebrationListViewModel>> GetCelebrationList()
        {
            return await Task.Run(() =>
            {
                IEnumerable<CelebrationListViewModel> celebrationListData = new List<CelebrationListViewModel>();
                var _tempList = new List<CelebrationListViewModel>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var celebrationList = context.GetCelebrationList();

                        foreach (var item in celebrationList)
                        {
                            CelebrationListViewModel _CelebrationListViewModel = new CelebrationListViewModel();

                            _CelebrationListViewModel.EmployeeID = item.EmployeeID;
                            _CelebrationListViewModel.EmployeeName = item.EmployeeName;
                            _CelebrationListViewModel.SpouseName = item.SpouseName;
                            _CelebrationListViewModel.CelebrationDate = Convert.ToDateTime(item.Celebration_date);
                            _CelebrationListViewModel.OrganizationEmail = item.OrganizationEmail;
                            _CelebrationListViewModel.LocationName = item.LocationName;
                            _CelebrationListViewModel.OnNoticePeriod = item.OnNoticePeriod;
                            _CelebrationListViewModel.Category = item.Category;

                            _tempList.Add(_CelebrationListViewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                celebrationListData = _tempList;
                return celebrationListData;
            });

        }

    }
}

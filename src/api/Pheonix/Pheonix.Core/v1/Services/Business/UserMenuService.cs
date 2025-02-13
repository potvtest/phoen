using AutoMapper;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public class UserMenuService : IUserMenuService
    {
        private IBasicOperationsService service;

        public UserMenuService(IBasicOperationsService opsService)
        {
            service = opsService;
        }

        public async Task<IEnumerable<UserMenuViewModel>> GetUserMenu(int roleId, int userId)
        {
            return await Task.Run(() =>
                {
                    var userMenu = service.Top<Module>(10, x => x.ParentID == null);
                    if (userMenu != null)
                    {
                        var menuList = Mapper.Map<IEnumerable<Module>, IEnumerable<UserMenuViewModel>>(userMenu);
                        foreach (var menuitem in menuList)
                        {
                            var subModulesList = service.Top<Module>(10, x => x.ParentID == menuitem.ID);
                            menuitem.SubMenu = Mapper.Map<IEnumerable<Module>, IEnumerable<UserMenuViewModel>>(subModulesList);
                            foreach (var item in menuitem.SubMenu)
                            {
                                var pageList = service.Top<Module>(10, x => x.ParentID == item.ID);
                                item.SubMenu = Mapper.Map<IEnumerable<Module>, IEnumerable<UserMenuViewModel>>(pageList);
                                foreach (var subMenuItem in item.SubMenu)
                                {
                                    subMenuItem.SubMenu = new List<UserMenuViewModel>();
                                }
                            }
                        }
                        return menuList;
                    }
                    return null;
                });
        }

        public Task<IEnumerable<T>> List<T>(string filters = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Single<T>(string filters = null)
        {
            throw new NotImplementedException();
        }

        public int Add<T>(T model)
        {
            throw new NotImplementedException();
        }

        public int Update<T>(T model)
        {
            throw new NotImplementedException();
        }

        public int Delete<T>(int id)
        {
            throw new NotImplementedException();
        }
    }
}

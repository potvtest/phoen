using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.AdminConfig;

namespace Pheonix.Core.v1.Services.AdminConfig
{
    public class AdminConfigService : IAdminConfigService
    {

        private IContextRepository _repository;
        public AdminConfigService(IContextRepository repo)
        {
            _repository = repo;

        }

        public AdminActionResult TakeActionOn(AdminConfigActionModel model)
        {
            var adminTask = AdminConfigTaskFactory.InitAdminTask(model.ActionType);
            return adminTask.TakeActionOn(_repository, model);
        }       


        public AdminConfigActionModel GetList(AdminConfigActionModel model)
        {
            var adminTask = AdminConfigTaskFactory.InitAdminTask(model.ActionType);
            return adminTask.GetList(_repository, model);
        }
    }
}

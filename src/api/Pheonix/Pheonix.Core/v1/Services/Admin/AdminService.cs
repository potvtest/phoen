using AutoMapper;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Admin
{
    public class AdminService : IAdminService
    {
        private IContextRepository _repository;
        public AdminService(IContextRepository repo)
        {
            _repository = repo;

        }

        public AdminActionResult TakeActionOn(AdminActionModel model)
        {
            var adminTask = AdminTaskFactory.InitAdminTask(model.ActionType);
            return adminTask.TakeActionOn(_repository, model);
        }

        public AdminActionResult Delete(AdminActionModel model)
        {
            var adminTask = AdminTaskFactory.InitAdminTask(model.ActionType);

            return adminTask.Delete(_repository, model);
        }


    }
}

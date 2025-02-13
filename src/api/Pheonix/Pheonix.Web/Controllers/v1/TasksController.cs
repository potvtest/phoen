using Pheonix.Core.v1.Services.Business;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Web.Extensions;
using System.Security.Claims;
using Pheonix.Models.VM.Classes.Task;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/tasks"), Authorize]
    public class TasksController : ApiController
    {
        private ITaskService service;
        static string fileUrl = ConfigurationManager.AppSettings["UploadedFileUrl"].ToString();

        public TasksController(ITaskService service)
        {
            this.service = service;
        }

        [Route("save-update"), HttpPost]
        public async Task<bool> SaveOrUpdate(TasksViewModel model)
        {
            return await service.SaveOrUpdate(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("get-all-list/{id:int?}"), HttpGet]
        public async Task<IEnumerable<TasksListModel>> GetTaskList(int id)
        {
            return await service.GetTaskList(id);
        }

        [Route("get-task/{taskId:int?}"), HttpGet]
        public async Task<TasksViewModel> GetTask(int taskId = 0)
        {
            return await service.GetTask(taskId);
        }
    }
}

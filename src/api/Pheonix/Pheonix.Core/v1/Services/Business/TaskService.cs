                using AutoMapper;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public class TaskService : ITaskService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;

        public TaskService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
        }


        public Task<bool> SaveOrUpdate(TasksViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                var task = Mapper.Map<TasksViewModel, PMSTasks>(model);
                var oldModel = service.Top<PMSTasks>(1, x => x.ID == model.id);
                var person = service.Top<PMSTasks>(10, x => x.ID == id).ToList().FirstOrDefault();

                if (!oldModel.Any())
                {
                    task.SrNo = "test";
                    task.CreatedDate = DateTime.Now;
                    isTaskCreated = service.Create<PMSTasks>(task, x => x.ID == model.id);
                }
                else
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        isTaskCreated = service.Update<PMSTasks>(task, oldModel.First());  //// Update the task.
                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return isTaskCreated;
            });
        }
        
        public async Task<IEnumerable<TasksListModel>> GetTaskList(int id)
        {
            return await Task.Run(() =>
           {
               var taskList = service.All<PMSTasks>().Where(x =>  x.IsDeleted == false);
               var taskmodel = Mapper.Map<IEnumerable<PMSTasks>, IEnumerable<TasksListModel>>(taskList);
               //foreach (var model in taskmodel)
               //{
               //    //model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(taskList.Where(x => x.ID == model.id).First().Person);
               //}

               return taskmodel;
           });
        }


        public async Task<TasksViewModel> GetTask(int taskId)
        {
            return await Task.Run(() =>
           {
               var tasks = service.Top<PMSTasks>(0, x => (x.ID == taskId)).FirstOrDefault();
               var model = Mapper.Map<PMSTasks, TasksViewModel>(tasks);
               return model;
           });
        }





    }
}
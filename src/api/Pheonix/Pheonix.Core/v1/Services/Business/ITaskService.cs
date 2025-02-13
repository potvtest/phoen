
using Pheonix.Models.VM.Classes.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public  interface ITaskService
    {
        Task<bool> SaveOrUpdate(TasksViewModel model, int id);
                     

        Task<IEnumerable<TasksListModel>> GetTaskList(int id);

        Task<TasksViewModel> GetTask(int TaskId);

       
    }
}

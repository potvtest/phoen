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
    public class AdminTaskType : IAdminConfigTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            try
            {
                if (model.taskType.Count > 0)
                {
                    foreach (TaskTypeModel taskType in model.taskType)
                    {
                        if (taskType.id > 0)
                        {
                            var currentTaskType = repo.FindBy<PMSTaskTypes>(x => x.Id == taskType.id).FirstOrDefault();
                            currentTaskType.TypeName = taskType.typeName;
                            currentTaskType.ParentTaskId = taskType.parentTaskId;
                            repo.Update<PMSTaskTypes>(currentTaskType);
                            result.message = currentTaskType.ParentTaskId != 0 ? "SubTaskType updated successfully" : "TaskType updated successfully";
                            result.isActionPerformed = true;
                        }
                        else
                        {
                            PMSTaskTypes objTaskType = new PMSTaskTypes();
                            objTaskType.TypeName = taskType.typeName;
                            objTaskType.ParentTaskId = taskType.parentTaskId;
                            objTaskType.Active = true;
                            repo.Create(objTaskType, null);
                            result.message = objTaskType.ParentTaskId != 0 ? "SubTaskType added successfully" : "TaskType added successfully";
                            result.isActionPerformed = true;
                        }

                        repo.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = $"operation Failed {ex.StackTrace}";
            }

            return result;
        }

        public AdminConfigActionModel GetList(IContextRepository repo, AdminConfigActionModel model)
        {
            AdminConfigActionModel configmodel = new AdminConfigActionModel();

            List<PMSTaskTypes> list = repo.GetAll<PMSTaskTypes>().ToList();
            configmodel.taskType = Mapper.Map<IEnumerable<PMSTaskTypes>, List<TaskTypeModel>>(list);
            return configmodel;
        }
    }
}
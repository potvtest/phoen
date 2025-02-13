using System;
using System.Collections.Generic;
using Pheonix.Models;
using Pheonix.Models.VM;
using System.Threading.Tasks;

namespace Pheonix.Core.Services
{
    public interface IProjectService
    {
        IEnumerable<ProjectViewModel> GetList(string filters, int personID);
        ActionResult Add(ProjectViewModel model, int personid);
        ActionResult Update(ProjectViewModel model, int personid);
        void Delete(int id, int personid);
        ProjectViewModel GetProject(int id);
        IEnumerable<ProjectViewModel> GetSubProjectDetails(int projId);

        // PMSConfigurations
        ActionResult Add(PMSConfigurationViewModel model);
        ActionResult Update(PMSConfigurationViewModel model);
        IEnumerable<object> GetList(int id);
        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId);
        void Delete(int id);
        // PMSConfigurations ends

        // Project Skill
        ActionResult Add(ProjectSkillViewModel model);
        ActionResult Update(ProjectSkillViewModel model);
        IEnumerable<object> GetSkillList(int id);
        void DeleteSkill(int id);
        // Project Skill ends
    }
}

using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.Repository
{
    public interface IProjectRepository
    {
        IEnumerable<ProjectList> GetList(string filters,int personID);
        ActionResult Add(ProjectViewModel model, int personid);
        ActionResult Update(ProjectViewModel model, int personid);
        void Delete(int id, int personid);
        ProjectList GetProject(int id);
        IEnumerable<ProjectList> GetSubProjectDetails(int projId);
        bool CheckIfSubProjectPresent(int id);

        // PMSConfigurations
        ActionResult Add(PMSConfigurationViewModel model);
        ActionResult Update(PMSConfigurationViewModel model);
        IEnumerable<object> GetList(int id);
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

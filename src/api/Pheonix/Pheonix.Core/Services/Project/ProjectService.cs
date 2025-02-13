using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Pheonix.Core.Services
{
    public class ProjectService : IProjectService
    {
        private IProjectRepository _projectRepository;
        private IBasicOperationsService _service;

        public ProjectService(IProjectRepository projectRepository, IBasicOperationsService opsService)
        {
            _service = opsService;
            _projectRepository = projectRepository;
        }
        public IEnumerable<ProjectViewModel> GetList(string filters, int personID)
        {
            var projectList = Mapper.Map<IEnumerable<ProjectList>, IEnumerable<ProjectViewModel>>(_projectRepository.GetList(filters, personID));
            foreach (ProjectViewModel prj in projectList)
            {
                prj.IsChildPresent = _projectRepository.CheckIfSubProjectPresent(prj.ID);
            }

            return projectList;
        }
        public ActionResult Add(ProjectViewModel model, int personid)
        {
            var statusCode = _projectRepository.Add(model, personid);
            return statusCode;
        }
        public ActionResult Update(ProjectViewModel model, int personid)
        {
            var statusCode = _projectRepository.Update(model, personid);
            return statusCode;
        }
        public void Delete(int id, int personid)
        {
            _projectRepository.Delete(id, personid);
        }
        public ProjectViewModel GetProject(int id)
        {
            var projectList = Mapper.Map<ProjectList, ProjectViewModel>(_projectRepository.GetProject(id));
            return projectList;
        }
        public IEnumerable<ProjectViewModel> GetSubProjectDetails(int projId)
        {
            var sublist = Mapper.Map<IEnumerable<ProjectList>, IEnumerable<ProjectViewModel>>(_projectRepository.GetSubProjectDetails(projId));
            return sublist;
        }

        // PMSConfigurations
        public ActionResult Add(PMSConfigurationViewModel model)
        {
            var statusCode = _projectRepository.Add(model);
            return statusCode;
        }
        public ActionResult Update(PMSConfigurationViewModel model)
        {
            var statusCode = _projectRepository.Update(model);
            return statusCode;
        }
        public IEnumerable<object> GetList(int id)
        {
            var projectList = _projectRepository.GetList(id);

            return projectList;
        }

        public async Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId)
        {
            Dictionary<string, List<DropdownItems>> items = new Dictionary<string, List<DropdownItems>>();

            var projectNames = await GetDropdownItems<ProjectList>("ProjectName", "ID", x => x.ProjectName.Trim());
            items.Add(ProjectDropDownType.ProjectName.ToString(), projectNames);

            List<int> roles = new List<int> { 4, 5, 6 };
            var pmsRoles = await GetDropdownItems<PMSRoles>("PMSRoleDescription", "PMSRoleID", x => x.PMSRoleDescription.Trim(), x => roles.Contains(x.PMSRoleID));
            items.Add(ProjectDropDownType.PMSRoles.ToString(), pmsRoles);

            var customers = await GetDropdownItems<Customer>("CustomerName", "ID", x => x.Name.Trim());
            items.Add(ProjectDropDownType.CustomerName.ToString(), customers);

            var skills = await GetDropdownItems<SkillMatrix>("Skill", "ID", x => x.Name.Trim());
            items.Add(ProjectDropDownType.Skill.ToString(), skills);

            return items;
        }

        private Task<List<DropdownItems>> GetDropdownItems<T>(string propertyName, string idSelector, Func<T, string> textSelector, Func<T, bool> predicate = null)
    where T : class
        {
            var lstItems = new List<DropdownItems>();

            var query = _service.All<T>();

            if (predicate != null)
            {
                query = query.Where(predicate).AsQueryable();
            }

            var items = query.OrderBy(x => propertyName).ToList();

            foreach (var item in items)
            {
                lstItems.Add(new DropdownItems
                {
                    ID = typeof(T).GetProperty(idSelector)?.GetValue(item) as int? ?? default(int),
                    Text = textSelector(item)
                });
            }

            return Task.FromResult(lstItems);
        }

        public void Delete(int id)
        {
            _projectRepository.Delete(id);
        }

        // PMSConfigurations ends

        //Project Skill
        public ActionResult Add(ProjectSkillViewModel model)
        {
            var statusCode = _projectRepository.Add(model);
            return statusCode;
        }
        public ActionResult Update(ProjectSkillViewModel model)
        {
            var statusCode = _projectRepository.Update(model);
            return statusCode;
        }
        public IEnumerable<object> GetSkillList(int id)
        {
            var projectList = _projectRepository.GetSkillList(id);

            return projectList;
        }
        public void DeleteSkill(int id)
        {
            _projectRepository.DeleteSkill(id);
        }
        //Project Skill ends
    }

    public enum ProjectDropDownType
    {
        ProjectName,
        PMSRoles,
        CustomerName,
        Skill
    }
}

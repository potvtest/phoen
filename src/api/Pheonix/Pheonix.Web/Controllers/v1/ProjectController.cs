using Pheonix.Core.Services;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Pheonix.Web.Extensions;
using System.Security.Claims;
using Pheonix.Web.Authorization;
using System.Threading.Tasks;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/project"), Authorize]
    public class ProjectController : ApiController, IProjectCRUDService<ProjectViewModel>
    {
        private IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }
        [Route("list")]
        public IEnumerable<ProjectViewModel> GetList(string filters = null)
        {
            var projectList = _projectService.GetList(filters, Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));

            return projectList;
        }
        [Route("add"), HttpPost]
        public ActionResult Add(ProjectViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _projectService.Add(model, Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }
        [Route("update"), HttpPost]
        public ActionResult Update(ProjectViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _projectService.Update(model, Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }
        [Route("delete/{id:int}"), HttpPost]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");
            var personid = Convert.ToInt32(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            _projectService.Delete(id, personid);
        }
        [Route("getproject/{id:int}"), HttpGet]
        public ProjectViewModel GetProject(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            return _projectService.GetProject(id);
        }
        [Route("sublist/{projId:int}")]
        public IEnumerable<ProjectViewModel> GetSubProjectDetails(int projId)
        {
            var projectList = _projectService.GetSubProjectDetails(projId);

            return projectList;
        }

        // PMSConfigurations
        [Route("config/add"), HttpPost]
        public ActionResult Add(PMSConfigurationViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _projectService.Add(model);
        }
        [Route("config/update"), HttpPost]
        public ActionResult Update(PMSConfigurationViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _projectService.Update(model);
        }
        [Route("config/list/{id:int}")]
        public IEnumerable<object> GetList(int id = 0)
        {
            var projectList = _projectService.GetList(id);

            return projectList;
        }
        [Route("dropdowns"), HttpGet]
        public async Task<IHttpActionResult> GetDropDowns()
        {
            return Ok(await _projectService.GetDropdowns(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }
        [Route("config/delete/{id:int}"), HttpPost]
        public void DeleteConfig(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _projectService.Delete(id);
        }
        // PMSConfigurations ends

        //Project Skill
        [Route("projectSkill/add"), HttpPost]
        public ActionResult Add(ProjectSkillViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _projectService.Add(model);
        }
        [Route("projectSkill/update"), HttpPost]
        public ActionResult Update(ProjectSkillViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return _projectService.Update(model);
        }
        [Route("projectSkill/list/{id:int}")]
        public IEnumerable<object> GetSkillList(int id = 0)
        {
            var projectList = _projectService.GetSkillList(id);

            return projectList;
        }

        [Route("projectSkill/delete/{id:int}"), HttpPost]
        public void DeleteSkill(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _projectService.DeleteSkill(id);
        }
        //Project Skill ends
    }
}
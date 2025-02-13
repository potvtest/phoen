using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/skillmatrix")]
    public class SkillMatrixController : ApiController, ICRUDService<SkillMatrixViewModel>
    {
        private ISkillMatrixService _skillMatrixService;

        public SkillMatrixController(ISkillMatrixService skillMatrixService)
        {
            _skillMatrixService = skillMatrixService;
        }

        [Route("list")]
        public IEnumerable<SkillMatrixViewModel> GetList(string filters = null)
        {
            var skillMatrixList = _skillMatrixService.GetList(filters);

            return skillMatrixList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(SkillMatrixViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_skillMatrixService.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(SkillMatrixViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_skillMatrixService.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _skillMatrixService.Delete(id);
        }
    }
}
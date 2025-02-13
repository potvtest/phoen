using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/personskillmapping")]
    public class PersonSkillMatppingController : ApiController, ICRUDService<PersonSkillMappingViewModel>
    {
        private IPersonSkillMappingService _personSkillMappingService;

        public PersonSkillMatppingController(IPersonSkillMappingService personSkillMappingService)
        {
            _personSkillMappingService = personSkillMappingService;
        }

        [Route("list")]
        public IEnumerable<PersonSkillMappingViewModel> GetList(string filters = null)
        {
            var personSkillMappingList = _personSkillMappingService.GetList(filters);

            return personSkillMappingList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(PersonSkillMappingViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personSkillMappingService.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(PersonSkillMappingViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personSkillMappingService.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _personSkillMappingService.Delete(id);
        }
    }
}
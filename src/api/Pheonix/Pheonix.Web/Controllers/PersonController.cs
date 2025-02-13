using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/person")]
    public class PersonController : ApiController, ICRUDService<PersonViewModel>
    {
        private IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [Route("list")]
        public IEnumerable<PersonViewModel> GetList(string filters = null)
        {
            var employeeList = _personService.GetList(filters);

            return employeeList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(PersonViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personService.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(PersonViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personService.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _personService.Delete(id);
        }
    }
}
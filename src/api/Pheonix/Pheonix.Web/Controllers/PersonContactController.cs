using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/personcontact")]
    public class PersonContactController : ApiController, ICRUDService<PersonContactViewModel>
    {
        private IPersonContactService _personContactService;

        public PersonContactController(IPersonContactService personContactService)
        {
            _personContactService = personContactService;
        }

        [Route("list")]
        public IEnumerable<PersonContactViewModel> GetList(string filters = null)
        {
            var employeeContactList = _personContactService.GetList(filters);

            return employeeContactList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(PersonContactViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personContactService.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(PersonContactViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personContactService.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _personContactService.Delete(id);
        }
    }
}
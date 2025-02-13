using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/personpersonal")]
    public class PersonPersonalController : ApiController, ICRUDService<PersonPersonalViewModel>
    {
        private IPersonPersonalService _personPersonalService;

        public PersonPersonalController(IPersonPersonalService personPersonalService)
        {
            _personPersonalService = personPersonalService;
        }

        [Route("list")]
        public IEnumerable<PersonPersonalViewModel> GetList(string filters = null)
        {
            var employeeAddressList = _personPersonalService.GetList(filters);

            return employeeAddressList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(PersonPersonalViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personPersonalService.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(PersonPersonalViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personPersonalService.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _personPersonalService.Delete(id);
        }
    }
}
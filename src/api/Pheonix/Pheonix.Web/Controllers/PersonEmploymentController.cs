using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/personemployment")]
    public class PersonEmploymentController : ApiController, ICRUDService<PersonEmploymentViewModel>
    {
        private IPersonEmploymentService _PersonEmploymentervice;

        public PersonEmploymentController(IPersonEmploymentService PersonEmploymentervice)
        {
            _PersonEmploymentervice = PersonEmploymentervice;
        }

        [Route("list")]
        public IEnumerable<PersonEmploymentViewModel> GetList(string filters = null)
        {
            var employeeAddressList = _PersonEmploymentervice.GetList(filters);

            return employeeAddressList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(PersonEmploymentViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_PersonEmploymentervice.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(PersonEmploymentViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_PersonEmploymentervice.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _PersonEmploymentervice.Delete(id);
        }
    }
}
using Pheonix.Core.Services;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Pheonix.Web.Controllers
{
    //[Authorize]
    [RoutePrefix("api/personaddress")]
    public class PersonAddressController : ApiController, ICRUDService<PersonAddressViewModel>
    {
        private IPersonAddressService _personAddressService;

        public PersonAddressController(IPersonAddressService personAddressService)
        {
            _personAddressService = personAddressService;
        }

        [Route("list")]
        public IEnumerable<PersonAddressViewModel> GetList(string filters = null)
        {
            var employeeAddressList = _personAddressService.GetList(filters);

            return employeeAddressList;
        }

        [Route("add"), HttpPost]
        public HttpResponseMessage Add(PersonAddressViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personAddressService.Add(model));
        }

        [Route("update"), HttpPost]
        public HttpResponseMessage Update(PersonAddressViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            return Request.CreateResponse(_personAddressService.Update(model));
        }

        [Route("delete/{id:int}"), HttpGet]
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _personAddressService.Delete(id);
        }
    }
}
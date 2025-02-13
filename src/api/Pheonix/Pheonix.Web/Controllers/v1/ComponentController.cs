using Pheonix.Core.v1.Services.Business;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Pheonix.Web.Controllers.v1
{
    [Authorize]
    [RoutePrefix("v1/component")]
    public class ComponentController : ApiController
    {
        private IComponentService _service;

        public ComponentController(IComponentService service)
        {
            _service = service;
        }

        [HttpGet, Route("")]
        public async Task<Dictionary<string, bool>> GetComponents()
        {
            return await _service.GetComponents();
        }
    }
}
using Pheonix.Core.v1.Services.Business;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Web.Http.Cors;
using System.Security.Principal;
using System.Security.Claims;

namespace Pheonix.Web.Controllers.v1
{



    public class BaseApiController : ApiController
    {
        private IEmployeeService _service;
        private string _email;
        private int _id = 0;

        public BaseApiController(IEmployeeService service)
        {
            _service = service;
        }


        protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var id = (RequestContext.Principal.Identity as ClaimsIdentity).Claims.Where(x => x.Type == ClaimTypes.PrimarySid);

            _email = "nilesh.dalal@v2solutions.com";
            this.LoggedInUser.Email = "nilesh.dalal@v2solutions.com";

        }


        public LoggedInUser LoggedInUser
        {
            get
            {
                var loggedInUser = new LoggedInUser()
                {
                    Email = _email,
                    ID = _id
                };

                return loggedInUser;
            }
        }
    }

    public class LoggedInUser
    {
        public int ID { get; set; }

        public string Email { get; set; }
    }
}
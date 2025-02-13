using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;
using Pheonix.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/usermenu")]
    [Authorize]
    public class UserMenuController : ApiController, IUserMenuContract
    {
        private IUserMenuService _service;

        public UserMenuController(IUserMenuService service)
        {
            _service = service;
        }

        [HttpGet, Route("")]
        public async Task<IEnumerable<UserMenuViewModel>> UserMenu()
        {
            var menu = await _service.GetUserMenu(RequestContext.GetClaimInt(ClaimTypes.Role), RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return menu;
        }
    }
}
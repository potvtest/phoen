using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Cors;
using Pheonix.Web.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Pheonix.Models.ViewModels;
using Pheonix.Core.v1.Services.Business;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/sharepoint")]
    public class CelebrationListController: ApiController
    {
        private ICelebreationListService _service;


        public CelebrationListController(ICelebreationListService service)
        {
            _service = service;
        }
        //int GetUserId()
        //{
        //    return RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
        //}

        [Route("GetCelebrationListData"), HttpGet]
        public async Task<IEnumerable<CelebrationListViewModel>> GetCelebrationList()
        {
            //var roles = RequestContext.GetClaim(ClaimTypes.Role);
            //var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));            
            return await _service.GetCelebrationList();
        }

    }
}
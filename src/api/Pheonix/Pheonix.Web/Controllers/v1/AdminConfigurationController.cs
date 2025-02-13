using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Pheonix.Models.Models.Admin;
using Pheonix.Core.v1.Services.Admin;
using Pheonix.Core.v1.Services.AdminConfig;
using Pheonix.Models.Models.AdminConfig;
using Pheonix.Web.Models;
using Pheonix.Web.Extensions;
using System.Security.Claims;
using Pheonix.DBContext;
using System.Web;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/adminconfig"), Authorize]
    public class AdminConfigurationController : ApiController
    {
        private IAdminConfigService _service;
        public AdminConfigurationController(IAdminConfigService service)
        {
            _service = service;
        }

        [HttpPost, Route("task/{type:int}")]
        public AdminActionResult Task(AdminConfigTaskModel model, int type)
        {
            AdminConfigActionModel mainModel = null;

            if ((new int[11] { 0, 1, 2, 3, 4, 5, 6, 7, 8,9,10 }).Contains(type))
            {
                mainModel = new AdminConfigActionModel
                {
                    ActionType = (AdminConfigTaskType)type,
                    ID = model.ID,
                    HolidayYear = model.HolidayYear,
                    Location = model.Location,
                    Details = model.details,
                    leaves = model.leaves,
                    locations = model.locations,
                    //bgParameterList = model.bgParameterList,
                    VCFDetails = model.vCFDetails,
                    vcfApprover = model.vcfApprover,
                    deliveryTeam = model.deliveryTeam,
                    resourcePool = model.resourcePool,
                    vcfGlobalApproversList = model.vcfGlobalApproversList,
                    skills = model.skills,
                    taskType=model.taskType,
                };
                return _service.TakeActionOn(mainModel);
            }
            return null;
        }

        [HttpPost, Route("list/{type:int}")]
        public AdminConfigActionModel GetList(AdminConfigActionModel model, int type)
        {
            model.ActionType = (AdminConfigTaskType)type;
            return _service.GetList(model);
        }
    }
}

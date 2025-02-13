using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("email")]
    public class EmailActionController : Controller
    {
        #region Class level variables

        private IEmailActionsService service;

        #endregion Class level variables



        public EmailActionController(IEmailActionsService emailActionsService)
        {
            service = emailActionsService;

        }

        // GET: EmailAction
        [Route("action/{templateType}/{requestAction}/{base64Email}/{authKey}")]
        public async Task<ActionResult> Index(EnumHelpers.EmailTemplateType templateType, string requestAction, string base64Email, Guid? authKey = null)
        {
            if (authKey == null)
            {
                ViewBag.status = Status.InvalidAuthKey;
            }
            else
            {
                switch (templateType)
                {
                    case EnumHelpers.EmailTemplateType.Leave:
                        {
                            Status status = await service.LeaveActionViaEmail(authKey.Value, requestAction, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Email)));
                            ViewBag.status = status;
                            break;
                        }
                    case EnumHelpers.EmailTemplateType.ExpenseApproval:
                        {
                            ViewBag.requestAction = requestAction;
                            ViewBag.key = authKey;
                            ViewBag.email = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Email));
                            ViewBag.status = await service.ExpenseApprovalViaEmail(authKey.Value, requestAction, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Email)));
                            break;
                        }

                }
            }

            return View();
        }


    }
}
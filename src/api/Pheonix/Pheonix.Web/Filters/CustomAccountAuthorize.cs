using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Pheonix.Web.Filters
{
    public class CustomAccountAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var viewResult = new JsonResult();
                viewResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                viewResult.Data = (new { IsSuccess = "Unauthorized", description = "Sorry, you do not have the required permission to perform this action." });
                filterContext.Result = viewResult;
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
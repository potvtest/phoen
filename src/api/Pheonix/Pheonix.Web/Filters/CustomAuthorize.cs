using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Pheonix.Web.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            bool isAuthorized = base.IsAuthorized(actionContext);
            if (isAuthorized)
            {
                base.OnAuthorization(actionContext);
            }
        }

        //public override void OnAuthorization(HttpActionContext actionContext)
        //{
        //    base.OnAuthorization(actionContext);
        //}

        //protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        //{
        //    var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        //    challengeMessage.Headers.Add("WWW-Authenticate", "Basic");
        //    //throw new HttpResponseException(challengeMessage);
        //    actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Unauthorized);

        //    //if (filterContext.HttpContext.Request.IsAjaxRequest())
        //    //{

        //    //    var viewResult = new JsonResult();
        //    //    viewResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    //    viewResult.Data = (new { IsSuccess = "Unauthorized", description = "Sorry, you do not have the required permission to perform this action." });
        //    //    actionContext.Result = viewResult;

        //    //}
        //    //else
        //    //    base.HandleUnauthorizedRequest(actionContext);
        //}

        //protected override bool IsAuthorized(HttpActionContext actionContext)
        //{
        //    bool isAuthorized = base.IsAuthorized(actionContext);
        //    bool isRequestHeaderOk = false;
        //    return (isAuthorized && isRequestHeaderOk);
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;

namespace Pheonix.Web.Handlers
{
    public class PheonixExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            HttpRequestMessage requestMessage = context.Request;
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            responseMessage = requestMessage.CreateResponse(context.Exception.Message);
            context.Result = new ResponseMessageResult(responseMessage);
        }
    }
}
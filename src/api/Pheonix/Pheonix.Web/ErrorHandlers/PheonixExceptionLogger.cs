using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace Pheonix.Web.Handlers
{
    public class PheonixExceptionLogger : ExceptionLogger
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            return Task<string>.Run(() => Log4Net.Error(String.Format("Unhandled exception thrown in {0} for request {1}: {2}, Stack Trace : {3}", context.Request.Method, context.Request.RequestUri, context.Exception, context.Exception.StackTrace)));
        }
    }
}
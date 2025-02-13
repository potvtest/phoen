using log4net;
using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Pheonix.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            HttpConfiguration config = GlobalConfiguration.Configuration;
            MvcHandler.DisableMvcResponseHeader = true;


            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

        }

        private readonly Stopwatch stopWatch = new Stopwatch();
        private static readonly ILog Log4Net = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpContext.Current.Response.AddHeader("x-frame-options", "SAMEORIGIN");
            this.stopWatch.Start();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            this.stopWatch.Stop();
            Log4Net.ErrorFormat("Action : {0} : Time Utilized : {1} Seconds",
                    ((System.Web.HttpApplication)(sender)).Context.Request.FilePath,
                    this.stopWatch.ElapsedMilliseconds / 1000);
            this.stopWatch.Reset();
        }
    }
}
using Microsoft.Owin.Security.OAuth;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unity.WebApi;
using Microsoft.Practices.Unity;
using Pheonix.Web.Mapping;
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Web.Http.ExceptionHandling;
using Pheonix.Web.Handlers;
using Pheonix.Web.Authorization;
using Pheonix.Core.v1.Services.Approval;
using System.Configuration;

namespace Pheonix.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            //config.EnableCors();
            ApiAccess.Initialize();

            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //json.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
            json.SerializerSettings.DateFormatString = "MM/dd/yyyy";// "mmm-dd-yyyy";
            json.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            //json.SerializerSettings.Converters.Add(new IsoDateTimeConverter());

            GlobalConfiguration.Configuration.MessageHandlers.Insert(0, new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));


            var corsAttr = new EnableCorsAttribute(ConfigurationManager.AppSettings["WhitelistUrls"], "*", "*");
            config.EnableCors(corsAttr);

            // Configure Web API to use only bearer token authentication.
            // Commented as it was not making the Controllers Authorize.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Services.Add(typeof(IExceptionLogger), new PheonixExceptionLogger());
            config.Services.Replace(typeof(IExceptionHandler), new PheonixExceptionHandler());

            // Web API routes
            config.MapHttpAttributeRoutes();


            UnityConfig.RegisterComponents();
            MappingDTOModelToModel.Configure();

            // Or any other way to fetch your container.


            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

        }
    }
}
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Pheonix.Web.Startup))]

namespace Pheonix.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
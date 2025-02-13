using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;
using Pheonix.DBContext;
using Pheonix.Web.Mapping;
using Pheonix.Web.Models;
using Pheonix.Web.Providers;
using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Unity.WebApi;
using System.Linq;
using System.Globalization;
using System.Configuration;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Pheonix.Web
{
    public partial class Startup
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Metadata Address is used by the application to retrieve the signing keys used by Azure AD.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        // The Post Logout Redirect Uri is the URL where the user will be redirected after they sign out.
        //
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        public void ConfigureAuth(IAppBuilder app)
        {
            ConfigureDbContext(app);
            ConfigureCookieAuthentication(app);
            //ConfigureGoogleAuth(app);
            ConfigureOffice365Auth(app);
            ConfigureJwtOAuth(app);
        }

        private static void ConfigureDbContext(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
        }

        private static void ConfigureJwtOAuth(IAppBuilder app)
        {
            app.UseOAuthAuthorizationServer(new AppOAuthOptions());
            app.UseJwtBearerAuthentication(new AppJwtOptions());

            HttpConfiguration config = new HttpConfiguration();
            app.UseWebApi(config);
        }

        private static void ConfigureCookieAuthentication(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                CookieSecure = CookieSecureOption.Always,
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(500000),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager, DefaultAuthenticationTypes.ApplicationCookie))
                }

            });
        }

        private static void ConfigureGoogleAuth(IAppBuilder app)
        {
            
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "200899829118-64tcj5ofov7iofnkbojs83tjb9bpnlr0.apps.googleusercontent.com",
                ClientSecret = "IOMqL-RsFOGdYFniESnNgIdL"
            });

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
        }

        private static void ConfigureOffice365Auth(IAppBuilder app)
        {
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = postLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = context => 
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Error?message=" + context.Exception.Message);
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }


    public class AppOAuthOptions : OAuthAuthorizationServerOptions
    {
        public AppOAuthOptions()
        {
            TokenEndpointPath = new PathString("/token");
            AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(500000);
            AccessTokenFormat = new AppJwtFormat(TimeSpan.FromMinutes(500000));
            Provider = new AppOAuthProvider();
            AllowInsecureHttp = true;

        }
    }

    public class AppJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly TimeSpan _options;

        public AppJwtFormat(TimeSpan options)
        {
            _options = options;
        }

        public string SignatureAlgorithm
        {
            get { return "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256"; }
        }

        public string DigestAlgorithm
        {
            get { return "http://www.w3.org/2001/04/xmlenc#sha256"; }
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null) throw new ArgumentNullException("data");

            var issuer = "localhost";
            var audience = "all";
            var key = Convert.FromBase64String("bXlzdXBlcnN0cm9uZ2tleWZvckFwcFByb3RlY3Rpb24=");
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.TotalMinutes);
            var signingCredentials = new SigningCredentials(
                                        new InMemorySymmetricSecurityKey(key),
                                        SignatureAlgorithm,
                                        DigestAlgorithm);
            var token = new JwtSecurityToken(issuer, audience, data.Identity.Claims,
                                             now, expires, signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }

    public class AppOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity();
            var username = context.OwinContext.Get<string>("username");
            var employee_id = context.OwinContext.Get<string>("employee_id");
            var role = context.OwinContext.Get<string>("role");
            var country = context.OwinContext.Get<string>("country");
            identity.AddClaim(new Claim(ClaimTypes.Email, username));
            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, employee_id));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            identity.AddClaim(new Claim(ClaimTypes.Country, country));

            context.Validated(identity);
            return Task.FromResult(0);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            try
            {
                var username = context.Parameters["username"];
                var employee_id = context.Parameters["employee_id"];

                PhoenixEntities dbContext = new PhoenixEntities();
                var user = dbContext.PersonEmployment.Where(x => x.OrganizationEmail == username.ToLower() && x.OrganizationEmail.Length > 0).First();// && x.IsEnabled == true);

                if (user != null)
                {
                    context.OwinContext.Set("username", username);
                    context.OwinContext.Set("employee_id", user.PersonID.ToString());
                    context.OwinContext.Set("role", user.Person.PersonInRole.Any() ? string.Join(",", user.Person.PersonInRole.Select(t => t.RoleID).ToList()) :  "20" );// RS: update  role by adding EDMS
                    context.OwinContext.Set("country", user.OfficeLocation.Value.ToString());

                    context.Validated();
                }
                else
                {
                    context.SetError("Invalid credentials");
                    context.Rejected();
                }
            }
            catch
            {
                context.SetError("Server error");
                context.Rejected();
            }
            return Task.FromResult(0);
        }


    }

    public class AppJwtOptions : JwtBearerAuthenticationOptions
    {
        public AppJwtOptions()
        {
            var issuer = "localhost";
            var audience = "all";
            var key = Convert.FromBase64String("bXlzdXBlcnN0cm9uZ2tleWZvckFwcFByb3RlY3Rpb24="); ;

            AllowedAudiences = new[] { audience };
            IssuerSecurityTokenProviders = new[]
        {
            new SymmetricKeyIssuerSecurityTokenProvider(issuer, key)
        };
        }
    }

}
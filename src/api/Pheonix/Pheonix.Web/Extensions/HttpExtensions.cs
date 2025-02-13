using System;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Pheonix.Web.Extensions
{
    public static class Extension
    {
        public static string GetClaim(this System.Web.Http.Controllers.HttpRequestContext context, string key)
        {
            var claim = (context.Principal.Identity as ClaimsIdentity).Claims.Where(t => t.Type == key).FirstOrDefault();
            if (claim != null) return claim.Value;
            throw new ArgumentNullException(key + " is not a valid Claim");
        }

        public static int GetClaimInt(this System.Web.Http.Controllers.HttpRequestContext context, string key)
        {
            var claim = (context.Principal.Identity as ClaimsIdentity).Claims.Where(t => t.Type == key).FirstOrDefault();
            if (claim != null) return Convert.ToInt32(claim.Value);
            throw new ArgumentNullException(key + " is not a valid RoleID");
        }

        public static T[] GetClaimCsvToArray<T>(this System.Web.Http.Controllers.HttpRequestContext context, string key)
        {
            var claim = (context.Principal.Identity as ClaimsIdentity).Claims.Where(t => t.Type == key).FirstOrDefault();
            if (claim != null) return JsonConvert.DeserializeObject<T[]>("["+claim.Value+"]");
            throw new ArgumentNullException(key + " is not a valid RoleID");
        }
    }
}
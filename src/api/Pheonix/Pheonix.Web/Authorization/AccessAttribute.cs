using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace Pheonix.Web.Authorization
{
    public class AccessAttribute : AuthorizeAttribute
    {
        public string Api { get; set; }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            var role = ((System.Security.Claims.ClaimsIdentity)(actionContext.RequestContext.Principal.Identity))
                .Claims.Where(t => t.Type == roleType).FirstOrDefault();
           var result =  ApiAccess.AccessDictionary[Api].Any(s => role.Value.Contains(s.ToString()));
            if (role == null || !result)
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{'Error':'You are trying to access unautorized api'}",
                        Encoding.Default, "application/json")
                };
        }
    }
}
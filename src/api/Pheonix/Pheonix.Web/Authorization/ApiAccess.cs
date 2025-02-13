using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Pheonix.Web.Authorization
{
    public class ApiAccess
    {
        public static Dictionary<string, int[]> AccessDictionary;
        private ApiAccess()
        {
            
        }
        static ApiAccess()
        {
            AccessDictionary = new Dictionary<string, int[]>();
        }
        public static void Initialize()
        {
            var access = ConfigurationManager.AppSettings["api-access"].ToString();
            access = HttpUtility.UrlDecode(access);
            AccessDictionary = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(access);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pheonix.Web.Filters
{
    public class RequestValidator
    {
        
        public static bool IsValid(int roleId)
        {
            if (roleId <= 12 || roleId == 27 || roleId == 24 || roleId == 35 || roleId == 38 || roleId == 20 || roleId == 44)
                return true;
            return false;
        }

        public static bool IsValid(int[] roleIds)
        {
            bool pass = false;
            foreach (var role in roleIds)
            {
                pass=IsValid(role);
                if (pass )break;
            }
                return pass;

        }

    }
}
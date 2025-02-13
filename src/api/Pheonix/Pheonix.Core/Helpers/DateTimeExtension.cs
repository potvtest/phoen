using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pheonix.Core.Helpers
{
    public static class DateTimeExtension
    {
        public static DateTime ToThisTimeZone(this DateTime datetime, string timezone)
        {

            TimeZoneInfo timeZoneInfo;

            //Set the time zone information to US Mountain Standard Time 
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            //Get date and time in US Mountain Standard Time 
            datetime = TimeZoneInfo.ConvertTime(datetime, timeZoneInfo);

            return datetime;

        }

        public static string sentanceCase(this string sourcestring)
        {
            var lowerCase = sourcestring.ToLower();
            var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            return r.Replace(lowerCase, s => s.Value.ToUpper());
        }

        public static string ToStandardDate(this DateTime date)
        {
            return date.ToString("MMMM dd, yyyy");
        }
    }


}

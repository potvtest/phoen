using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Pheonix.Helpers
{
    public static class SearchParameterIdentifier
    {
        /// <summary>
        /// This method will identify column and its' type based on the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GetPredicate<T>(string query, KeyValuePair<string, Type> kvp, string compareMethod) where T : class
        {
            ParameterExpression pe = null;
            pe = Expression.Parameter(typeof(T), "obj");

            MemberExpression left;

            left = Expression.Property(pe, kvp.Key.ToLower());

            MethodInfo method = typeof(string).GetMethod(compareMethod, new[] { typeof(string) });

            Expression right = Expression.Constant(Convert.ChangeType(query.ToLower(), kvp.Value));

            Expression body = kvp.Value.Name.Contains("Int") ? Expression.Equal(left, right) as Expression : Expression.Call(left, method, right) as Expression;
            return Expression.Lambda<Func<T, bool>>(body, pe);

        }

        public static Expression<Func<T, bool>> GetDefaultPredicate<T>(string query, KeyValuePair<string, Type> kvp, string compareMethod)
        {
            ParameterExpression pe = null;
            pe = Expression.Parameter(typeof(T), "obj");

            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            MemberExpression firstLeft, lastleft;
            //MemberExpression firstLeft, midLeft, lastleft;

            firstLeft = Expression.Property(pe, "FirstName");
            //midLeft = Expression.Property(pe, "MiddleName");
            lastleft = Expression.Property(pe, "LastName");

            Expression right = Expression.Constant(query.ToLower());

            Expression eFirst = Expression.Call(firstLeft, method, right);
            //Expression eMid = Expression.Call(midLeft, method, right);
            Expression eLast = Expression.Call(lastleft, method, right);

            Expression preBody = Expression.OrElse(eFirst, eLast);
            //Expression preBody = Expression.OrElse(eFirst, eMid);
            //Expression body = Expression.OrElse(preBody, eLast);

            //return Expression.Lambda<Func<T, bool>>(body, pe);
            return Expression.Lambda<Func<T, bool>>(preBody, pe);
        }


        public static KeyValuePair<string, KeyValuePair<string, Type>> IdentifyEmployeeSearchOnPersonQuery(string query, List<Designation> collection)
        {
            KeyValuePair<string, KeyValuePair<string, Type>> type;
           // type = new KeyValuePair<string, KeyValuePair<string, Type>>("pn", new KeyValuePair<string, Type>("FirstName", typeof(string)));
            type = default(KeyValuePair<string, KeyValuePair<string,Type>>);

            //Identify Integert specifi to V2 emloyee
            if (IsDigitsOnly(query))
                type = new KeyValuePair<string, KeyValuePair<string, Type>>("p", new KeyValuePair<string, Type>("ID", typeof(int)));
            else if (TestV2EmployeeEmailID(query))
                type = new KeyValuePair<string, KeyValuePair<string, Type>>("pe", new KeyValuePair<string, Type>("OrganizationEmail", typeof(string)));
            else if (TestV2EmployeeDesignation(query, collection))
                type = new KeyValuePair<string, KeyValuePair<string, Type>>("d", new KeyValuePair<string, Type>("Name", typeof(string)));


            return type;
        }

        private static bool TestV2EmployeeID(string query)
        {
            int result = 0;
            bool pass = false;
            var test = int.TryParse(query, out result);

            if (test)
            {
                if (query.Length == 3 || query.Length == 4)
                {
                    if ((new char[] { '1', '2', '3', '4' }).Any(t => t == query[0]))
                        pass = true;
                }
            }
            return pass;
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private static bool TestV2EmployeeEmailID(string query)
        {
            //Regex regex = new Regex(@"^[a-zA-Z]+@v2solutions.com$");
            //if (regex.Match(query.ToLower()).Success)
            if (query.ToLower().Contains("@v2solutions.com"))
                return true;
            return false;
        }

        private static bool TestV2EmployeeDesignation(string query, List<Designation> collection)
        {
            return collection.Any(t => t.Name.ToLower().Equals(query.ToLower()));

        }

    }
}
/////^[a-zA-Z]+@yourdomain\.com$
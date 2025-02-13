using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.TransactionHooks
{
    public static  class Predicates<T>where T:class
    {
       
        public static Expression<Func<Person, bool>> Expression(Person model)
        {
            return x => x.FirstName == model.FirstName && x.LastName == model.LastName;
        }
    }
}

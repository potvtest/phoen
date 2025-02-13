using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pheonix.Core.v1.Services.ValuePortal//ValuePortal.Services
{
    public interface IMainOperationsService
    {
        bool Create<T>(T model, Expression<Func<T, bool>> predicate) where T : class;

        bool Update<T>(T model, T oldModel) where T : class;

        bool Update<T>(T model) where T : class;

        bool Remove<T>(T model, Expression<Func<T, bool>> predicate) where T : class;

        bool SoftRemove<T>(T model, Expression<Func<T, bool>> predicate) where T : class;

        IEnumerable<T> All<T>() where T : class;

        T First<T>(Expression<Func<T, bool>> predicate) where T : class;

        IEnumerable<T> Top<T>(int limit, Expression<Func<T, bool>> predicate) where T : class;

        int Finalize(bool isOk);
    }
}

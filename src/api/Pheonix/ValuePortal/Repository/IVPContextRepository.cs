using System;
using System.Linq;
using System.Linq.Expressions;

namespace ValuePortal.Repository
{
    public interface IVPContextRepository
    {
        IQueryable<T> GetAll<T>() where T : class;

        IQueryable<T> FindBy<T>(Expression<Func<T, bool>> predicate) where T : class;

        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;

        bool Create<T>(T entity, Expression<Func<T, bool>> predicate) where T : class;

        bool HardRemove<T>(T entity, Expression<Func<T, bool>> predicate) where T : class;

        bool SoftRemove<T>(T entity, Expression<Func<T, bool>> predicate) where T : class;

        bool Update<T>(T entity, T oldModel) where T : class;

        bool Update<T>(T entity) where T : class;

        int Save();
    }
}

using System;
using System.Collections;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Pheonix.DBContext.Repository.ValuePortal//ValuePortal.Repository
{
    public class VPContextRepository<C> :
    IVPContextRepository
         where C : DbContext, new()
    {
        private C _context;

        public VPContextRepository(C context)
        {
            _context = context;
            //_context.Database.Connection.Open();
        }

        public virtual IQueryable<T> GetAll<T>() where T : class
        {
            IQueryable<T> query = _context.Set<T>();
            return query;
        }

        public IQueryable<T> FindBy<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);
            return query;
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var resultset = FindBy(predicate);

            if (resultset != null)
                return resultset.FirstOrDefault();
            return null;
        }

        public virtual bool Create<T>(T entity, Expression<Func<T, bool>> predicate) where T : class
        {
            var marked = false;
            if (predicate == null)
            { marked = true; }
            else
            {
                var result = FirstOrDefault(predicate);
                marked = result == null;
            }
            if (marked)
                _context.Set<T>().Add(entity);
            return marked;
        }

        public bool HardRemove<T>(T entity, Expression<Func<T, bool>> predicate) where T : class
        {
            var marked = false;
            var result = FirstOrDefault(predicate);
            marked = result == null;
            if (!marked)
                _context.Set<T>().Remove(entity);
            return !marked;
        }

        public virtual bool SoftRemove<T>(T entity, Expression<Func<T, bool>> predicate) where T : class
        {
            var marked = false;
            var result = FirstOrDefault(predicate);
            marked = result == null;
            if (!marked)
            {
                DbEntityEntry entry = _context.Entry(entity);
                entry.Property("IsDeleted").CurrentValue = true;
            }
            //_context.Set<T>()(entity);
            return !marked;
        }

        public virtual bool Update<T>(T entity, T oldModel) where T : class
        {
            DbEntityEntry entry = _context.Entry(oldModel);
            foreach (string propertyName in entry.OriginalValues.PropertyNames)
            {
                // Get the old field value from the database.
                var original = entry.GetDatabaseValues().GetValue<object>(propertyName);
                // Get the current value from posted edit page.
                var current = entity.GetType().GetProperty(propertyName).GetValue(entity);
                if (!object.Equals(original, current) && current != null && propertyName != "ID" && propertyName != "SignInSignOutID" && propertyName != "PersonID")
                {
                    entry.Property(propertyName).IsModified = true;
                    entry.Property(propertyName).CurrentValue = current;
                }
            }
            return true;
        }

        public virtual bool Update<T>(T entity) where T : class
        {
            _context.Entry(entity).State = EntityState.Modified;
            return true;
        }



        public virtual int Save()
        {
            return _context.SaveChanges();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Pheonix.DBContext.Repository;

namespace ValuePortal.Services
{
    public class MainOperationsService:IMainOperationsService
    {
        private IVPContextRepository _repo;
        private bool result = false;

        public MainOperationsService(IVPContextRepository repository)
        {
            _repo = repository;
        }

        public virtual bool Create<T>(T model, Expression<Func<T, bool>> predicate) where T : class
        {
            result = _repo.Create(model, predicate);
            return result;
        }

        public virtual bool Update<T>(T model, T oldModel) where T : class
        {
            result = _repo.Update(model, oldModel);
            return result;
        }

        public virtual bool Update<T>(T model) where T : class
        {
            result = _repo.Update(model);
            return result;
        }


        public virtual bool Remove<T>(T model, Expression<Func<T, bool>> predicate) where T : class
        {
            result = _repo.HardRemove(model, predicate);
            return result;
        }

        public virtual bool SoftRemove<T>(T model, Expression<Func<T, bool>> predicate) where T : class
        {
            result = _repo.SoftRemove(model, predicate);
            return result;
        }

        public virtual IEnumerable<T> All<T>() where T : class
        {
            return _repo.GetAll<T>();
        }

        public virtual T First<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            return _repo.FirstOrDefault(predicate);
        }

        public virtual IEnumerable<T> Top<T>(int limit, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            return _repo.FindBy<T>(predicate);
        }

        public int Finalize(bool isOk)
        {
            if (isOk)
                return _repo.Save();
            return -999;//This code is to make explicit that context has nothing to save...
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Core.Repository.ValuePortal;
using Pheonix.DBContext;
//using Pheonix.DBContext.Repository.ValuePortal;
using ValuePortal;

namespace Pheonix.Core.Repository.ValuePortal//ValuePortal.Repository.ValuePortal
{
    public class ValuePortalRepository:IValuePortalRepository
    {
        private ValuePortalEntities _valuePortalEntities;
        public ValuePortalRepository()
        {
            _valuePortalEntities = new ValuePortalEntities();
        }
        public IEnumerable<object> GetList(string query, bool showInActive)
        {
            var result = new List<VPIdeaDetail>();
            try
            {
                using (var db = _valuePortalEntities)
                {
                    result= db.VPIdeaDetails.ToList();
                }
                
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}

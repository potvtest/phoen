using AutoMapper;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Repository.Utils
{
    public class DeletedRecordsLog : IDeletedRecordsLog
    {
        private PhoenixEntities _phoenixEntity;

        public DeletedRecordsLog()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        public bool AddLogs(DeletedRecordsLogDetailViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    DeletedRecordsLogDetails dbModel = Mapper.Map<DeletedRecordsLogDetailViewModel, DeletedRecordsLogDetails>(model);
                    db.DeletedRecordsLogDetails.Add(dbModel);
                    db.SaveChanges();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

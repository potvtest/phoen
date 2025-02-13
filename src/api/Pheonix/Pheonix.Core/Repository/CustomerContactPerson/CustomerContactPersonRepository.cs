using AutoMapper;
using Pheonix.Core.Repository.Utils;
using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Pheonix.Core.Repository
{
    public class CustomerContactPersonRepository : ICustomerContactPersonRepository
    {
        private PhoenixEntities _phoenixEntity;
        IDeletedRecordsLog recordLog;

        public CustomerContactPersonRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            recordLog = new DeletedRecordsLog();            
        }

        public IEnumerable<CustomerContactPerson> GetList(string filters)
        {
            var customerContactPerson = new List<CustomerContactPerson>();
            using (var db = _phoenixEntity)
            {
                int Id = Convert.ToInt32(filters);
                customerContactPerson = db.CustomerContactPerson.Where(t => t.CustomerID == Id && t.IsDeleted == false).ToList();
            }

            return customerContactPerson;
        }

        public ActionResult Add(CustomerContactPersonViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    CustomerContactPerson dbModel = Mapper.Map<CustomerContactPersonViewModel, CustomerContactPerson>(model);
                    dbModel.IsDeleted = false;
                    db.CustomerContactPerson.Add(dbModel);
                    db.SaveChanges();

                    Customer custModel = db.Customer.Where(x => x.ID == model.CustomerID).SingleOrDefault();
                    if (custModel != null)
                    {
                        if (custModel.Status == 2)
                        {
                            custModel.Status = 1;
                            db.Entry(custModel).CurrentValues.SetValues(custModel);
                            db.SaveChanges();
                        }
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Customer Contact Person added successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult Update(CustomerContactPersonViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    CustomerContactPerson dbModel = db.CustomerContactPerson.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {

                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<CustomerContactPersonViewModel, CustomerContactPerson>(model));
                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Customer Contact Person updated successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult Delete(int id, int personid)
        {
            try
            {
                ActionResult result = new ActionResult();
                DeletedRecordsLogDetailViewModel model;
                using (var db = _phoenixEntity)
                {
                    var customerContactPerson = db.CustomerContactPerson.Where(x => x.ID == id).FirstOrDefault();
                    model = new DeletedRecordsLogDetailViewModel
                    {
                        ModuleID = 1,
                        DeletedRecordID = id,
                        DeletedBy = personid,
                        DeletedOn = DateTime.Now
                    };
                    var isLogAdded = AddLog(model);
                    if (customerContactPerson != null && isLogAdded)
                    {
                        customerContactPerson.IsDeleted = true;
                        db.SaveChanges();
                        result.isActionPerformed = true;
                        result.message = string.Format("Customer Contact Deleted Successfully");
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public CustomerContactPerson GetCustomerContactPerson(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var customerContactPerson = db.CustomerContactPerson.Where(x => x.ID == id).FirstOrDefault();
                    return customerContactPerson;
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
        private bool AddLog(DeletedRecordsLogDetailViewModel model)
        {
            return recordLog.AddLogs(model);
        }
    }
}

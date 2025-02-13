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
    public class CustomerAddressRepository : ICustomerAddressRepository
    {
        private PhoenixEntities _phoenixEntity;
        IDeletedRecordsLog recordLog;

        public CustomerAddressRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            recordLog = new DeletedRecordsLog();            
        }

        public IEnumerable<CustomerAddress> GetList(string filters)
        {
            var customerAddressList = new List<CustomerAddress>();
            using (var db = _phoenixEntity)
            {
                int Id = Convert.ToInt32(filters);
                customerAddressList = db.CustomerAddress.Where(t => t.CustomerID == Id && t.Isdeleted == false).ToList();
            }

            return customerAddressList;
        }

        public ActionResult Add(CustomerAddressViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    CustomerAddress dbModel = Mapper.Map<CustomerAddressViewModel, CustomerAddress>(model);
                    dbModel.Isdeleted = false;
                    db.CustomerAddress.Add(dbModel);
                    db.SaveChanges();

                    Customer custModel = db.Customer.Where(x => x.ID == model.CustomerID).SingleOrDefault();
                    if (custModel != null)
                    {
                        if(custModel.Status == 2)
                        {
                            custModel.Status = 1;
                            db.Entry(custModel).CurrentValues.SetValues(custModel);
                            db.SaveChanges();
                        }
                    }
                }
                
                result.isActionPerformed = true;
                result.message = string.Format("CustomerAddress added successfully");
            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult Update(CustomerAddressViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {

                    CustomerAddress dbModel = db.CustomerAddress.Where(x => x.ID == model.ID).SingleOrDefault();

                    if (dbModel != null)
                    {

                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<CustomerAddressViewModel, CustomerAddress>(model));

                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("CustomerAddress updated successfully");
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
                    var customerAddress = db.CustomerAddress.Where(x => x.ID == id).FirstOrDefault();
                    model = new DeletedRecordsLogDetailViewModel
                    {
                        ModuleID = 1,
                        DeletedRecordID = id,
                        DeletedBy = personid,
                        DeletedOn = DateTime.Now
                    };
                    var isLogAdded = AddLog(model);
                    if (customerAddress != null && isLogAdded)
                    {
                        customerAddress.Isdeleted = true;
                        db.SaveChanges();
                        result.isActionPerformed = true;
                        result.message = string.Format("Customer Address Deleted Successfully");
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public CustomerAddress GetCustomerAddress(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var customerAddressList = db.CustomerAddress.Where(x => x.ID == id).FirstOrDefault();
                    return customerAddressList;
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

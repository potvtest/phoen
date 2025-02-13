using AutoMapper;
using Pheonix.Core.Repository.Utils;
using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Pheonix.Core.Repository
{
    public class CustomerContractRepository : ICustomerContractRepository
    {
        private PhoenixEntities _phoenixEntity;
        IDeletedRecordsLog recordLog;

        public CustomerContractRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            recordLog = new DeletedRecordsLog();
        }

        public IEnumerable<CustomerContractViewModel> GetList(string filters)
        {
            using (var db = _phoenixEntity)
            {
                int id = Convert.ToInt32(filters);
                var customerContract = db.CustomerContract.Where(t => t.CustomerID == id && t.IsDeleted == false).AsEnumerable();
                var contractModel = Mapper.Map<IEnumerable<CustomerContract>, IEnumerable<CustomerContractViewModel>>(customerContract).AsEnumerable();
                return contractModel;
            }
        }

        public ActionResult Add(CustomerContractViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    CustomerContract dbModel = Mapper.Map<CustomerContractViewModel, CustomerContract>(model);
                    dbModel.IsDeleted = false;
                    db.CustomerContract.Add(dbModel);
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
                result.message = string.Format("Customer Contract added successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult Update(CustomerContractViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    CustomerContract dbModel = db.CustomerContract.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<CustomerContractViewModel, CustomerContract>(model));
                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Customer Contract updated successfully");
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
                    var customerContract = db.CustomerContract.Where(x => x.ID == id).FirstOrDefault();
                    model = new DeletedRecordsLogDetailViewModel
                    {
                        ModuleID = 1,
                        DeletedRecordID = id,
                        DeletedBy = personid,
                        DeletedOn = DateTime.Now
                    };
                    var isLogAdded = AddLog(model);
                    if (customerContract != null && isLogAdded)
                    {
                        customerContract.IsDeleted = true;
                        db.SaveChanges();
                        result.isActionPerformed = true;
                        result.message = string.Format("Customer Contract Deleted Successfully");
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public CustomerContract GetCustomerContract(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var customerContract = db.CustomerContract.Where(x => x.ID == id).FirstOrDefault();
                    return customerContract;
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

        public ActionResult Add(ContractAttachmentViewModel model,int personID)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    ContractAttachments dbModel = Mapper.Map<ContractAttachmentViewModel, ContractAttachments>(model);
                    dbModel.AttachedBy = personID;
                    dbModel.AttachedOn = DateTime.Now;
                    db.ContractAttachments.Add(dbModel);
                    db.SaveChanges();
                }
                result.isActionPerformed = true;
                result.message = string.Format("Contract uploaded successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
    }
}

using AutoMapper;
using Pheonix.Core.Repository.Utils;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Data;
using Newtonsoft.Json.Linq;
using System;
using Pheonix.Core.v1.Services.Email;

namespace Pheonix.Core.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private PhoenixEntities _phoenixEntity;
        IDeletedRecordsLog recordLog;
        private IEmailService _emailService;

        public CustomerRepository(IEmailService emailService)
        {
            this._emailService = emailService;
            _phoenixEntity = new PhoenixEntities();
            recordLog = new DeletedRecordsLog();
        }

        public IEnumerable<object> GetList(string query, bool showInActive)
        {
            var customerList = new List<GetCustomerData_Result>();
            using (var db = _phoenixEntity)
            {
                if (query == null) { query = ""; }
                customerList = db.GetCustomerData(query, showInActive).OrderBy(x => x.Name).ToList();
            }

            return customerList;
        }

        public ActionResult Add(CustomerViewModel model, int userId)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    Customer dbModel = Mapper.Map<CustomerViewModel, Customer>(model);
                    dbModel.Status = 2; //Draft mode
                    dbModel.CreatedBy = Convert.ToString(userId);
                    dbModel.CreatedOn = DateTime.Now;
                    db.Customer.Add(dbModel);
                    db.SaveChanges();
                    //foreach (var item in model.BgcParameter)
                    //{
                    //    CustomerBGMapping bgModel = new CustomerBGMapping();
                    //    bgModel.CustomerID = dbModel.ID;
                    //    bgModel.BGParameterID = item;
                    //    db.CustomerBGMapping.Add(bgModel);
                    //}
                    //db.SaveChanges();
                    result.RecordID = dbModel.ID;
                    _emailService.SendCustomerMails(CustomerMailAction.Creation, dbModel, userId);
                }


                result.isActionPerformed = true;
                result.message = string.Format("Customer added successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult Update(CustomerViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    Customer dbModel = db.Customer.Where(x => x.ID == model.ID).SingleOrDefault();
                    model.CreatedBy = dbModel.CreatedBy;
                    model.CreatedOn = dbModel.CreatedOn;
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<CustomerViewModel, Customer>(model));
                        //db.SaveChanges();
                        List<CustomerBGMapping> dbCBMappingList = db.CustomerBGMapping.Where(x => x.CustomerID == dbModel.ID).ToList();
                        foreach (var itemTobeRemove in dbCBMappingList)
                        {
                            db.CustomerBGMapping.Remove(itemTobeRemove);
                        }
                        foreach (var item in model.BgcParameter)
                        {
                            CustomerBGMapping bgModel = new CustomerBGMapping();
                            bgModel.CustomerID = dbModel.ID;
                            bgModel.BGParameterID = item;
                            db.CustomerBGMapping.Add(bgModel);
                        }
                        db.SaveChanges();
                        result.RecordID = dbModel.ID;
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Customer updated successfully");
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
                using (var db = _phoenixEntity)
                {
                    var customer = db.Customer.Where(x => x.ID == id).FirstOrDefault();
                    if (customer != null)
                    {
                        if (customer.Status == 1)
                            customer.Status = 0;
                        else
                            customer.Status = 1;

                        db.SaveChanges();
                        result.isActionPerformed = true;
                        result.message = string.Format(" ");
                    }
                }
                return result;
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public Customer GetCustomer(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var customerList = db.Customer.Where(x => x.ID == id).FirstOrDefault();
                    return customerList;
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public List<int> getCustomerBGMapping(int id)
        {
            try
            {
                using (var db = new PhoenixEntities())
                {
                    var customerBGMappingList = db.CustomerBGMapping.Where(x => x.CustomerID == id);

                    return customerBGMappingList.Select(x => x.BGParameterID).ToList();
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

        public HttpResponseMessage GetDownload(List<object> reportQueryParams)
        {
            //List<Dictionary<string, string>> data, List<string> columns, 
            /**
             * 1. Fetch data from DB for given month and Year
             * 2. Create Columns   GetGenericColumnList returns List<string>
             * 3. Create data dictionary list CreateGenericDataDictionary returns List<Dictionary<string,string>>
             * 4. Create CSV inputs List<string> as column, List<Dictionary<string,string>> as data
             */
            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            var sw = new StringBuilder();

            List<string> columnsname = new List<string>();
            for (var i = 0; i < reportQueryParams.Count; i++)
            {
                JObject json = JObject.Parse(reportQueryParams[i].ToString());
                foreach (var property in json)
                {

                    var key = property.Key;
                    if (key == "jsonString")
                    {
                        if (property.Value.ToString() != "")
                        {
                            JObject json1 = JObject.Parse(property.Value.ToString());
                            foreach (var property1 in json1)
                            {
                                if (!columnsname.Contains(property1.Key))
                                {
                                    columnsname.Add(property1.Key);
                                    sw.Append("\"" + property1.Key + "\",");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!columnsname.Contains(key))
                        {
                            columnsname.Add(key);
                            sw.Append("\"" + key + "\",");
                        }
                    }
                }
            }
            sw.Append("\r\n");


            for (var i = 0; i < reportQueryParams.Count; i++)
            {
                JObject json = JObject.Parse(reportQueryParams[i].ToString());

                foreach (var property in json)
                {
                    if (property.Key == "jsonString")
                    {
                        if (property.Value.ToString() != "")
                        {
                            JObject json1 = JObject.Parse(property.Value.ToString());
                            foreach (var property1 in json1)
                            {
                                sw.AppendFormat("\"{0}\",", property1.Value);
                            }
                        }
                    }
                    else
                    {
                        sw.AppendFormat("\"{0}\",", property.Value);
                    }
                }
                sw.AppendFormat("\r\n");
            }
            Response.Content = new StringContent(sw.ToString());
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            Response.Content.Headers.ContentDisposition.FileName = "RecordExport.csv";
            return Response;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM;
using Pheonix.DBContext.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.ViewModels;
using AutoMapper;

namespace Pheonix.Core.v1.Services.Business
{
    public class CompOffExceptionService : ICompOffExceptionService
    {
        private PhoenixEntities _phoenixEntity;

        public CompOffExceptionService()
        {
            _phoenixEntity = new PhoenixEntities();
        }
        public Task<List<PersonViewModel>> GetManagerList()
        {
            return GetActiveManagerList();
        }
        async Task<List<PersonViewModel>> GetActiveManagerList()
        {
            return await Task.Run(() =>
            {
                List<PersonViewModel> employeeList = new List<PersonViewModel>();
                using (var db = _phoenixEntity)
                {
                    employeeList = (from p in db.PersonReporting
                                    join pr in db.People
                                    on p.ReportingTo equals pr.ID
                                    where pr.Active == true
                                    select new PersonViewModel { ID = p.ReportingTo, FirstName = pr.FirstName, LastName = pr.LastName }).Distinct().OrderBy(x => x.FirstName).ToList();
                }
                return employeeList;
            });
        }
        public Task<List<PersonViewModel>> GetExecptionList()
        {
            return GetActiveExceptionList();
        }
        async Task<List<PersonViewModel>> GetActiveExceptionList()
        {
            return await Task.Run(() =>
            {
                List<PersonViewModel> employeeList = new List<PersonViewModel>();
                using (var db = _phoenixEntity)
                {
                    employeeList = (from ce in db.CompensatoryOffException
                                    join pr in db.People
                                    on ce.PersonID equals pr.ID
                                    where pr.Active == true
                                    select new PersonViewModel { ID = pr.ID, FirstName = pr.FirstName, LastName = pr.LastName }).OrderBy(x => x.ID).Distinct().ToList();

                }
                return employeeList;
            });
        }
        public Task<ActiveEmpViewModel> GetActiveEmployeeList(string query, string managerID)
        {
            return GetList(query, managerID);
        }
        async Task<ActiveEmpViewModel> GetList(string query, string managerID)
        {
            return await Task.Run(() =>
            {
                int iD = Convert.ToInt32(query.Trim());
                int manageriD = Convert.ToInt32(managerID.Trim());
                ActiveEmpViewModel model = new ActiveEmpViewModel();
                
                using (var db = _phoenixEntity)
                {
                    var empList = ((from ce in db.CompensatoryOffException
                                    join pr in db.People
                                     on ce.PersonID equals pr.ID
                                    join pe in db.PersonEmployment
                                     on pr.ID equals pe.PersonID
                                    join de in db.Designations
                                    on pe.DesignationID equals de.ID
                                    where pr.ID == iD && pr.Active == true && de.Grade < 4
                                    select new ExceptedEmpViewModel { ID = pr.ID, FirstName = pr.FirstName, LastName = pr.LastName }).ToList()
                                     .Concat(from ce in db.CompensatoryOffException
                                             join p in db.People
                                             on ce.PersonID equals p.ID
                                             join pr in db.PersonReporting
                                             on p.ID equals pr.PersonID
                                             join pe in db.PersonEmployment
                                             on p.ID equals pe.PersonID
                                             join de in db.Designations
                                             on pe.DesignationID equals de.ID
                                             where pr.ReportingTo == manageriD && p.Active == true && de.Grade < 4
                                             select new ExceptedEmpViewModel { ID = p.ID, FirstName = p.FirstName, LastName = p.LastName }).ToList()
                                    ).OrderBy(f => f.ID).GroupBy(f => f.ID, f => f).Select(g => g.First());

                    model.exceptedEmpViewModel = empList.ToList();


                    var empList2 = ((from p in db.People
                                     join pe in db.PersonEmployment
                                     on p.ID equals pe.PersonID
                                     join de in db.Designations
                                     on pe.DesignationID equals de.ID
                                     where p.ID == iD && p.Active == true && de.Grade < 4
                                     select new PersonViewModel { ID = p.ID, FirstName = p.FirstName, LastName = p.LastName }).ToList()
                                     .Concat(from p in db.People
                                             join pr in db.PersonReporting
                                             on p.ID equals pr.PersonID
                                             join pe in db.PersonEmployment
                                             on p.ID equals pe.PersonID
                                             join de in db.Designations
                                             on pe.DesignationID equals de.ID
                                             where pr.ReportingTo == manageriD && p.Active == true && de.Grade < 4
                                             select new PersonViewModel { ID = p.ID, FirstName = p.FirstName, LastName = p.LastName }).ToList()
                                    ).OrderBy(f => f.ID).GroupBy(f => f.ID, f => f).Select(g => g.First());

                    
                    model.personviewmodel = empList2.Where(p => !empList.Any(p2 => p2.ID == p.ID)).ToList();

                }
                return model;
            });
        }
        public Task<AdminActionResult> Remove(List<CompOffExceptionViewModel> model)
        {
            return deleteData(model);
        }
        async Task<AdminActionResult> deleteData(List<CompOffExceptionViewModel> model)
        {
            AdminActionResult result = new AdminActionResult();
            return await Task.Run(() =>
            {
                using (var db = _phoenixEntity)
                {
                    try
                    {
                        if (model.Count > 0 && model != null)
                        {
                            foreach (CompOffExceptionViewModel exec in model)
                            {
                                CompensatoryOffException dbModel = (from p in db.CompensatoryOffException
                                                                    where p.PersonID == exec.ID
                                                                    select p).FirstOrDefault();
                                db.CompensatoryOffException.Remove(dbModel);
                                db.SaveChanges();
                            }
                            result.isActionPerformed = true;
                            result.message = string.Format("Data removed Successfully");
                        }

                    }
                    catch
                    {
                        result.isActionPerformed = false;
                        result.message = string.Format("Action Failed");
                    }
                    return result;
                }
            });
        }
        public Task<AdminActionResult> Add(List<CompOffExceptionViewModel> model)
        {
            return addData(model);
        }
        async Task<AdminActionResult> addData(List<CompOffExceptionViewModel> model)
        {
            AdminActionResult result = new AdminActionResult();
            return await Task.Run(() =>
            {
                using (var db = _phoenixEntity)
                {
                    try
                    {
                        if (model.Count > 0 && model != null)
                        {
                            foreach (CompOffExceptionViewModel exec in model)
                            {
                                CompensatoryOffException dbModel = Mapper.Map<CompOffExceptionViewModel, CompensatoryOffException>(exec);

                                db.CompensatoryOffException.Add(dbModel);
                                db.SaveChanges();
                            }
                            result.isActionPerformed = true;
                            result.message = string.Format("Data added Successfully");
                        }
                    }
                    catch
                    {
                        result.isActionPerformed = false;
                        result.message = string.Format("Action Failed");
                    }
                    return result;
                }
            });
        }
    }
}

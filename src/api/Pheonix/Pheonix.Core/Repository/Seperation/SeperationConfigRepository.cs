#region Edit Style
/// 1 - HR
/// 2 - Self
/// 3 - Deparmentwise
#endregion

#region Approval Status
/// 0 - Pending
/// 1 - Withdraw
/// 2 - Approved
/// 3 - Release Process Started
/// 4 - Release Process Completed
/// 5 - Resignation Rejected By Reporting Manager
#endregion

using AutoMapper;
using Newtonsoft.Json;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;


namespace Pheonix.Core.Repository
{
    public class SeperationConfigRepository : ISeperationConfigRepository
    {
        //private IBasicOperationsService _BasicService;
        //private IEmailService _EmailService;
        //private Pheonix.Core.v1.Services.Seperation.SeparationCardService _seperationCardService;

        //public SeperationConfigRepository(IBasicOperationsService basicService, IEmailService emailService)
        //    : this()
        //{
        //    _BasicService = basicService;
        //    _EmailService = emailService;
        //}

        private PhoenixEntities _phoenixEntity;

        int _HRRoleId = QueryHelper.GetConfigKeyValue<int>("HRRoleId");

        public SeperationConfigRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        public IEnumerable<SeperationConfigViewModel> GetList(string filters)
        {
            var seperationConfigList = new List<SeperationConfigViewModel>();
            var seperationConfigList1 = new List<SeperationConfigViewModel>();

            using (var db = _phoenixEntity)
            {
                seperationConfigList = (from sc in db.SeperationConfig
                                        join ro in db.Role on sc.RoleID equals ro.ID
                                        orderby sc.ID

                                        select new SeperationConfigViewModel
                                        {
                                            ID = sc.ID,
                                            ChecklistItem = sc.ChecklistItem,
                                            RoleID = sc.RoleID,
                                            IsActive = sc.IsActive,
                                            Name = ro.Name,
                                            ChecklistType = sc.ChecklistType.Value
                                        }).ToList();

                seperationConfigList1 = (from sc in db.SeperationConfig
                                         where sc.RoleID == 0
                                         orderby sc.ID

                                         select new SeperationConfigViewModel
                                         {
                                             ID = sc.ID,
                                             ChecklistItem = sc.ChecklistItem,
                                             RoleID = sc.RoleID,
                                             IsActive = sc.IsActive,
                                             Name = "Exit Process Manager",
                                             ChecklistType = sc.ChecklistType.Value
                                         }).ToList();

            }
            var seperationConfigList3 = seperationConfigList.Concat(seperationConfigList1);

            return seperationConfigList3;
        }

        public ActionResult Add(SeperationConfigViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    model.Sequence = GetDeptSequence(model.RoleID);

                    SeperationConfig dbModel = Mapper.Map<SeperationConfigViewModel, SeperationConfig>(model);

                    db.SeperationConfig.Add(dbModel);
                    db.SaveChanges();
                    model = Mapper.Map<SeperationConfig, SeperationConfigViewModel>(dbModel);
                }
                result.isActionPerformed = true;
                result.message = string.Format("Separation Config Added Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }

            return result;
        }

        public ActionResult Update(SeperationConfigViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    model.Sequence = GetDeptSequence(model.RoleID);

                    SeperationConfig dbModel = db.SeperationConfig.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<SeperationConfigViewModel, SeperationConfig>(model));
                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Separation Config Updated Successfully");
            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult Delete(int id)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    var seperationconfig = db.SeperationConfig.Where(x => x.ID == id).FirstOrDefault();
                    if (seperationconfig != null)
                    {
                        db.SeperationConfig.Remove(seperationconfig);
                        db.SaveChanges();

                        result.isActionPerformed = true;
                        result.message = string.Format("Separation Config Deleted Successfully");
                    }
                }
            }
            catch (SqlException ex)
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
                throw new HttpException(500, ex.ToString());
            }
            return result;
        }

        //public IEnumerable<Role> GetRoleList()
        //{
        //    var roleList = new List<Role>();
        //    var role1 = new List<Role>();
        //    using (var db = new PhoenixEntities())
        //    {
        //        roleList = (from d in db.Role
        //                    join
        //                        pr in db.PersonInRole on d.ID equals pr.RoleID
        //                    select d).Distinct().ToList();
        //    }

        //    Role ro = new Role();
        //    ro.ID = 0;
        //    ro.Name = "Exit Process Manager";
        //    ro.Description = "";
        //    ro.SliceFromRole = null;
        //    ro.From = null;
        //    ro.To = null;
        //    ro.IsTemporary = null;
        //    ro.IsDeleted = false;
        //    roleList.Add(ro);

        //    return roleList.OrderBy(x => x.Name);
        //}

        public Task<List<DropdownItems>> GetRoleList()
        {
            List<DropdownItems> lstItems = new List<DropdownItems>();
            using (var db = _phoenixEntity)
            {
                var categories = db.HelpDeskCategories.Where(hc => hc.IsDeleted == false);

                foreach (var item in categories)
                {
                    DropdownItems dropdownItem = new DropdownItems
                    {
                        ID = item.AssignedRole.Value,
                        Text = item.Name.Trim()
                    };
                    lstItems.Add(dropdownItem);
                }
            }

            DropdownItems dropdownItem1 = new DropdownItems
            {
                ID = 0,
                Text = "Exit Process Manager"
            };
            lstItems.Add(dropdownItem1);

            return Task.Run(() => { return lstItems; });
        }

        public SeperationViewModel GetSeperationById(int id)
        {
            try
            {
                var seperationModel = new SeperationViewModel();
                using (var _db = _phoenixEntity)
                {
                    var resultData = _db.Separation.Where(s => s.ID == id).ToList();
                    if (resultData != null && resultData.Count > 0)
                    {
                        seperationModel = Mapper.Map<Separation, SeperationViewModel>(resultData.FirstOrDefault());
                        seperationModel.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(resultData.FirstOrDefault().Person1);
                    }
                    return seperationModel;
                }
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        #region SeparationReasonMaster
        public ActionResult AddReason(SeparationReasonViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    SeparationReasons dbModel = Mapper.Map<SeparationReasonViewModel, SeparationReasons>(model);
                    dbModel.UpdatedOn = null;
                    db.SeparationReasons.Add(dbModel);
                    db.SaveChanges();
                    model = Mapper.Map<SeparationReasons, SeparationReasonViewModel>(dbModel);
                }
                result.isActionPerformed = true;
                result.message = string.Format("Separation Reason Added Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }

            return result;
        }

        public ActionResult UpdateReason(SeparationReasonViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    SeparationReasons dbModel = db.SeparationReasons.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        model.CreatedOn = Convert.ToDateTime(dbModel.CreatedOn);
                        model.CreatedBy = Convert.ToInt32(dbModel.CreatedBy);
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<SeparationReasonViewModel, SeparationReasons>(model));
                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Separation Reason Updated Successfully");
            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public ActionResult DeleteReason(int id)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    var seperationreason = db.SeparationReasons.Where(x => x.ID == id).FirstOrDefault();
                    if (seperationreason != null)
                    {
                        db.SeparationReasons.Remove(seperationreason);
                        db.SaveChanges();

                        result.isActionPerformed = true;
                        result.message = string.Format("Separation Reason Deleted Successfully");
                    }
                }
            }
            catch (SqlException ex)
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
                throw new HttpException(500, ex.ToString());
            }
            return result;
        }

        public IEnumerable<SeparationReasonViewModel> GetReasonList(string filters)
        {
            var separationReasonList = new List<SeparationReasonViewModel>();

            using (var db = _phoenixEntity)
            {
                separationReasonList = (from sc in db.SeparationReasons
                                        join ro in db.SeparationReasonMaster on sc.ReasonCode equals ro.ID
                                        orderby sc.ID

                                        select new SeparationReasonViewModel
                                        {
                                            ID = sc.ID,
                                            ReasonDescription = sc.ReasonDescription,
                                            ReasonCodeName = ro.ReasonCategoty,
                                            IsActive = sc.IsActive.Value,
                                            ReasonCode = sc.ReasonCode,
                                            ReasonCodeID=sc.ReasonCodeID
                                        }).ToList();



            }
            return separationReasonList;
        }
        #endregion


        public int GetDeptSequence(int roleID)
        {

            if (roleID == 12 || roleID == 24 || roleID == 38)
                return 50; //"HR"
            else if (roleID == 27 || roleID == 35)
                return 1; // "RMG";
            else if (roleID == 0)
                return 2; //"Exit Process Manager";
            else if (roleID == 25 || roleID == 37)
                return 3; //"IT";
            else if (roleID == 23 || roleID == 33)
                return 4; //"Finance";
            else if (roleID == 22 || roleID == 31 || roleID == 1)
                return 5; //"Admin";
            else if (roleID == 28 || roleID == 34)
                return 6; // "Internal";
            else if (roleID == 29 || roleID == 32)
                return 7; //"CQ";
            else if (roleID == 30 || roleID == 36)
                return 8; //"VWR";
            else if (roleID == 41 || roleID == 42)
                return 9; //"V2Hub";
            else
                return 0;
        }
    }
}

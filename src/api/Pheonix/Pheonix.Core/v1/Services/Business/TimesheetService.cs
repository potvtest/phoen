using AutoMapper;
using Newtonsoft.Json.Linq;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Timesheet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data;
using Pheonix.Helpers;
using System.Web.UI;
using System.Web;
using Newtonsoft.Json;
using Pheonix.Models;
using Microsoft.VisualBasic.FileIO;
using System.Security.Claims;
using System.Globalization;
using System.Data.Entity.Infrastructure;

namespace Pheonix.Core.v1.Services.Business
{
    public class TimesheetService : ITimesheetService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;

        public enum TimesheetDropDownType
        {
            ProjectName,
            SubProjectName,
            TaskType,
            SubTaskType,
            ProjectRole,
            ReportingMnager
        }

        public enum UploadTypeEnum
        {
            CreatedViaForm,
            UploadedViaExcel
        }

        public TimesheetService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
        }

        public Task<bool> SaveOrUpdate(TimesheetViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                var times = Mapper.Map<TimesheetViewModel, PMSTimesheet>(model);
                var oldModel = service.Top<PMSTimesheet>(1, x => x.ID == model.ID);
                if (!oldModel.Any())
                {
                    List<string> test = new List<string>();

                    if (model.ProjectName != "")
                    {
                        var regex = new Regex(@"#(\w*[0-9a-zA-Z-]+\w*[0-9a-zA-Z-])");
                        var matches = regex.Matches(model.Description);
                        string prjname = "";
                        foreach (Match m in matches)
                        {
                            string s = m.Value;
                            prjname = s.Replace('-', ' ').Replace('#', ' ').TrimStart();
                        }

                        times.ProjectID = service.First<ProjectList>(x => x.ProjectName == prjname).ID;
                    }
                    times.IsEmailSent = false;
                    times.CreatedDate = DateTime.Now;
                    isTaskCreated = service.Create<PMSTimesheet>(times, x => x.ID == model.ID);
                }
                else
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        if (model.ProjectName != "")
                        {
                            var regex = new Regex(@"#(\w*[0-9a-zA-Z-]+\w*[0-9a-zA-Z-])");
                            var matches = regex.Matches(model.Description);
                            string prjname = "";
                            foreach (Match m in matches)
                            {
                                string s = m.Value;
                                prjname = s.Replace('-', ' ').Replace('#', ' ').TrimStart();
                            }

                            times.ProjectID = service.First<ProjectList>(x => x.ProjectName == prjname).ID;
                        }
                        isTaskCreated = service.Update<PMSTimesheet>(times, oldModel.First());  //// Update the task.
                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return isTaskCreated;
            });
        }

        public Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId)
        {
            var db = new PhoenixEntities();
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();
            using (var connection = db.Database.Connection)
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[GetProjectAssignedWithOtherList]" + userId;
                using (var reader = command.ExecuteReader())
                {
                    var projectName =
                        ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<GetDropDownProjectDetails>(reader)
                            .ToList();
                    reader.NextResult();
                    foreach (var item in projectName)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = item.Id,
                            Text = item.Name.Trim()
                        };
                        if (IsSplitRequire(dropdownItem.Text))
                        {
                            dropdownItem.Text = SplitProjectName(item.Name);
                        }
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.ProjectName.ToString(), lstItems);

                    // Get the subproject names
                    lstItems = new List<DropdownItems>();
                    var subProjectName =
                        ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<GetDropDownSubProjectDetails>(reader)
                            .ToList();
                    reader.NextResult();
                    foreach (var subproject in subProjectName)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = subproject.Id,
                            Text = subproject.Name.Trim(),
                            AssignRole = subproject.ParentID
                        };
                        if (dropdownItem.Text.Contains(projectSubstring))
                        {
                            dropdownItem.Text = SplitProjectName(subproject.Name);
                        }
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.SubProjectName.ToString(), lstItems);
                    // Get the tasktypes name
                    var tasktypes = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<TaskTypeDetails>(reader)
                            .ToList();
                    reader.NextResult();

                    lstItems = new List<DropdownItems>();
                    foreach (var item in tasktypes)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = item.Id,
                            Text = item.Name.Trim()
                        };
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.TaskType.ToString(), lstItems);
                    lstItems = new List<DropdownItems>();
                    var subtasktypes = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<SubTaskTypeDetails>(reader)
                            .ToList();
                    reader.NextResult();
                    foreach (var subtask in subtasktypes)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = subtask.Id,
                            Text = subtask.Name.Trim(),
                            ProjectRole = subtask.ParentID
                        };
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.SubTaskType.ToString(), lstItems);
                    //Get project role From PMSProjectRole
                    var ProjectRole = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<PMSRoleDetails>(reader)
                            .ToList();
                    reader.NextResult();

                    lstItems = new List<DropdownItems>();
                    foreach (var item in ProjectRole)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = item.Id,
                            Text = item.Name.Trim()
                        };
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.ProjectRole.ToString(), lstItems);
                    var ReportingMgr = ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<ReportingToDetails>(reader)
                            .ToList();
                    lstItems = new List<DropdownItems>();
                    foreach (var item in ReportingMgr)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = item.Id,
                        };
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.ReportingMnager.ToString(), lstItems);

                }

            }


            return Task.Run(() => { return Items; });
        }

        public Task<Dictionary<string, List<DropdownItems>>> TimeSheetLedgerDropdown(int userId)
        {
            var db = new PhoenixEntities();
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();

            var subQuery1 = from pra in db.PMSResourceAllocation
                            where pra.PersonID == userId
                            select pra.ProjectID;
            var subquery2 = from rdc in db.ReportDropDownConfig
                            where rdc.PersonId == userId
                            select rdc.ProjectIds;
            var mainQuery = from pl in db.ProjectList
                            where subQuery1.Contains(pl.ID) || subquery2.Contains(pl.ID)
                            select new { pl };
            lstItems = new List<DropdownItems>();
            foreach (var item in mainQuery)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.pl.ID,
                    Text = item.pl.ProjectName
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TimesheetDropDownType.ProjectName.ToString(), lstItems);

            lstItems = new List<DropdownItems>();
            foreach (var project in mainQuery)
            {
                var subprojectname = service.All<ProjectList>()
                    .Where(x => x.Active == 1 && x.ParentProjId == project.pl.ID)
                    .OrderBy(x => x.ProjectName)
                    .ToList();

                foreach (var subproject in subprojectname)
                {
                    DropdownItems dropdownItem = new DropdownItems
                    {
                        ID = subproject.ID,
                        Text = subproject.ProjectName.Trim(),
                        AssignRole = subproject.ParentProjId
                    };
                    if (dropdownItem.Text.Contains(projectSubstring))
                    {
                        dropdownItem.Text = SplitProjectName(subproject.ProjectName);
                    }
                    lstItems.Add(dropdownItem);
                }
            }
            Items.Add(TimesheetDropDownType.SubProjectName.ToString(), lstItems);
            // Get the tasktypes name

            var tasktypes = service.All<PMSTaskTypes>()
            .Where(x => x.Active == true && x.ParentTaskId == null || x.ParentTaskId == 0)
            .OrderBy(x => x.TypeName);

            lstItems = new List<DropdownItems>();
            foreach (var item in tasktypes)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.TypeName.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TimesheetDropDownType.TaskType.ToString(), lstItems);

            // Get the subtasktypes name for Digital Experience (DU=26)

            lstItems = new List<DropdownItems>();
            foreach (var item in tasktypes)
            {
                var subtasktypes = service.All<PMSTaskTypes>()
                    .Where(x => x.Active == true && x.ParentTaskId == item.Id)
                    .OrderBy(x => x.TypeName);

                foreach (var subtask in subtasktypes)
                {
                    DropdownItems dropdownItem = new DropdownItems
                    {
                        ID = subtask.Id,
                        Text = subtask.TypeName.Trim(),
                        ProjectRole = subtask.ParentTaskId
                    };
                    lstItems.Add(dropdownItem);
                }
            }
            Items.Add(TimesheetDropDownType.SubTaskType.ToString(), lstItems);
            //Get project role From PMSProjectRole
            var ProjectRole = service.All<PMSRole>().OrderBy(x => x.Role);

            lstItems = new List<DropdownItems>();
            foreach (var item in ProjectRole)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.Role.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TimesheetDropDownType.ProjectRole.ToString(), lstItems);

            var ReportingMgr = service.All<PersonReporting>().OrderBy(x => x.ReportingTo);

            lstItems = new List<DropdownItems>();
            foreach (var item in ReportingMgr)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TimesheetDropDownType.ReportingMnager.ToString(), lstItems);

            return Task.Run(() => { return Items; });
        }

        public async Task<IEnumerable<EmployeeProfileViewModel>> GetEmpList()
        {
            return await Task.Run(() =>
            {
                var personlist = service.All<Person>().Where(x => x.Active == true).ToList();
                var taskmodel = Mapper.Map<IEnumerable<Person>, IEnumerable<EmployeeProfileViewModel>>(personlist);
                return taskmodel;
            });
        }

        public Task<bool> SaveOrUpdateTimesheetForm(TimesheetViewModel model, int id)
        {
            var deliveryUnit = service.First<Person>(x => x.ID == id).PersonEmployment.First().DeliveryUnit;
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                var times = Mapper.Map<TimesheetViewModel, PMSTimesheet>(model);
                var oldModel = service.Top<PMSTimesheet>(1, x => x.ID == model.ID);

                if (!oldModel.Any())
                {
                    times.IsEmailSent = false;
                    times.CreatedDate = DateTime.Now;
                    times.UploadType = UploadTypeEnum.CreatedViaForm.ToString();  //// Using enum here
                    isTaskCreated = service.Create<PMSTimesheet>(times, x => x.ID == model.ID);
                }

                else
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        isTaskCreated = service.Update<PMSTimesheet>(times, oldModel.First());  //// Update the task.
                    }
                }
                // Check for TotalHours
                if (!string.IsNullOrEmpty(model.TotalHours))
                {
                    int totalMinutes = ConvertTotalHoursToMinutes(model.TotalHours);
                    if (totalMinutes > (24 * 60))
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(model.NonBillableTime))
                    {
                        int nonBillableTime = ConvertTotalHoursToMinutes(model.NonBillableTime);
                        if (nonBillableTime > (24 * 60))
                        {
                            return false;
                        }

                        // Calculate the sum of totalMinutes and nonBillableTime
                        int totalSum = totalMinutes + nonBillableTime;
                        if (totalSum <= 0 || totalSum > (24 * 60))
                        {
                            return false;
                        }
                    }

                }

                if (isTaskCreated)
                    service.Finalize(true);
                return isTaskCreated;
            });
        }

        public async Task<bool> SaveMultipleEntryTimesheetForm(IEnumerable<TimesheetMultipleEntryViewModel> viewModelList, int id)
        {
            try
            {
                var deliveryUnit = service.First<Person>(x => x.ID == id).PersonEmployment.First().DeliveryUnit;
                bool isTaskCreated = false;
                var totalSumMinutesPerDayList = viewModelList.GroupBy(d => d.Date).Select(t => new { Date = t.Select(u => u.Date).FirstOrDefault(), TotalSumMinutes = t.Sum(u => ConvertTotalHoursToMinutes(u.TotalHours) + ConvertTotalHoursToMinutes(u.NonBillableTime)) }).ToList();
                var viewModelEmployeeId = viewModelList.Select(i => i.PersonID).FirstOrDefault();
                var viewModelDateList = viewModelList.Select(d => d.Date).Distinct().ToList();
                var existingTimesheetEntryOfEmployeeList = service.All<PMSTimesheet>().Where(v => (v.PersonID == viewModelEmployeeId && viewModelDateList.Contains(v.Date) && v.IsDeleted == false)).
                                                           Select(
                                                            g => new
                                                            {
                                                                ID = g.ID,
                                                                TotalHours = g.Time,
                                                                NonBillableTime = g.NonBillableTime,
                                                                Date = g.Date
                                                            }
                                                           ).ToList();

                var existingTotalSumMinutesPerDayList = existingTimesheetEntryOfEmployeeList.GroupBy(d => d.Date).Select(t => new { Date = t.Select(u => u.Date).FirstOrDefault(), TotalSumMinutes = t.Sum(u => ConvertTotalHoursToMinutes(u.TotalHours) + ConvertTotalHoursToMinutes(u.NonBillableTime)) }).ToList();

                var combinedTotalSumMinutesPerDayList = (from t in totalSumMinutesPerDayList
                                                         join e in existingTotalSumMinutesPerDayList on t.Date equals e.Date
                                                         select new
                                                         {
                                                             TotalSumMinutes = t.TotalSumMinutes + e.TotalSumMinutes
                                                         }
                                                        ).ToList();


                /* Comment: Validation for total hours in minutes for both existing and new multiple timesheet entries */
                foreach (var combinedTotalSumMinutesPerDay in combinedTotalSumMinutesPerDayList)
                {
                    if (combinedTotalSumMinutesPerDay.TotalSumMinutes <= 0 || combinedTotalSumMinutesPerDay.TotalSumMinutes > (24 * 60))
                    {
                        return false;
                    }
                }
                /* Comment:End */

                /* Comment: Validation for total hours in minutes for new multiple timesheet entries without previous timesheet entry */
                foreach (var totalSumMinutesPerDay in totalSumMinutesPerDayList)
                {
                    if (totalSumMinutesPerDay.TotalSumMinutes <= 0 || totalSumMinutesPerDay.TotalSumMinutes > (24 * 60))
                    {
                        return false;
                    }
                }
                /* Comment:End */

                foreach (var model in viewModelList)
                {
                    var times = Mapper.Map<TimesheetMultipleEntryViewModel, PMSTimesheet>(model);
                    var oldModel = service.Top<PMSTimesheet>(1, x => x.ID == model.ID);

                    if (!oldModel.Any())
                    {
                        times.JsonString = "";
                        times.SubTaskId = null;
                        times.IsDeleted = false;
                        times.IsEmailSent = false;
                        times.CreatedDate = DateTime.Now;
                        times.UploadType = UploadTypeEnum.CreatedViaForm.ToString();  //// Using enum here
                        isTaskCreated = service.Create<PMSTimesheet>(times, x => x.ID == model.ID);
                    }

                }

                if (isTaskCreated)
                    service.Finalize(true);

                return await Task.FromResult(isTaskCreated).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<GetTimesheetList_Result>> GetTimesheetList(int id)
            => await Task.FromResult(QueryHelper.GetTimesheetList(id)).ConfigureAwait(false);


        public async Task<IEnumerable<BillableVsNonBillableViewModel>> GetBillableVsNonBillableClientRep(int? duId, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<BillableVsNonBillableViewModel> viewModelList = new List<BillableVsNonBillableViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var billableVsNonBillableListRecords = context.BillableVsNonBillableByClientProjectReport(duId, startDate, endDate);
                    foreach (var item in billableVsNonBillableListRecords.ToList())
                    {
                        viewModelList.Add(
                            new BillableVsNonBillableViewModel
                            {
                                CustomerID = item.CustomerID,
                                CustomerName = item.CustomerName,
                                ProjectID = item.ProjectID,
                                ProjectName = item.ProjectName,
                                TotalHoursAvailable = item.TotalHoursAvailable,
                                TotalEnteredBillableHours = item.EnteredBillableHours,
                                TotalEnteredNonBillableHours = item.EnteredNonBillableHours,
                                TotalMissingHours = item.TotalMissingHours
                            }
                        );
                    }

                    return await Task.FromResult(viewModelList).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportDefault(TimesheetCustomReportObject customReportObject)
        {
            try
            {
                List<TimesheetCustomReportViewModel> viewModelList = new List<TimesheetCustomReportViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var customTimesheetReportListRecords = context.GetTimesheetCustomReport_Default(customReportObject.duID, customReportObject.projectID, customReportObject.subProjectID, customReportObject.employeeID, customReportObject.billable, customReportObject.startDate, customReportObject.endDate);
                    foreach (var item in customTimesheetReportListRecords)
                    {
                        viewModelList.Add(
                            new TimesheetCustomReportViewModel
                            {
                                DuId = item.DUId,
                                DuName = item.DUName,
                                EmployeeId = item.EmployeeId,
                                EmployeeName = item.EmployeeName,
                                ProjectCode = item.ProjectCode,
                                ProjectId = item.ProjectID,
                                SubProjectId = item.SubProjectID,
                                ProjectName = item.ProjectName,
                                SubProjectName = item.SubProjectName,
                                Billable = item.Billable,
                                TaskTypeId = item.TaskTypeID,
                                TaskType = item.TaskType,
                                TicketId = item.TicketID,
                                Date = item.Date,
                                BillableHours = item.BillableHours,
                                BillableDescription = item.BillableDescription,
                                NonBillableHours = item.NonBillableHours,
                                NonBillableDescription = item.NonBillableDescription,
                                ProjectManager = item.ProjectManager,
                                SubProjectManager = item.SubProjectManager,
                                DeliveryManager = item.DeliveryManager,
                                OrgReportingManager = item.OrgReportingManager,
                                RmgManager = item.RMGManager,
                                ProjectManagerName = item.ProjectManagerName,
                                SubProjectManagerName = item.SubProjectManagerName,
                                DeliveryManagerName = item.DeliveryManagerName,
                                OrgReportingManagerName = item.OrgReportingManagerName,
                                RmgManagerName = item.RMGManagerName
                            }
                        );
                    }

                    return await Task.FromResult(viewModelList).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportIndividualOverall(TimesheetCustomReportObject customReportObject)
        {
            try
            {
                List<TimesheetCustomReportViewModel> viewModelList = new List<TimesheetCustomReportViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var customTimesheetReportListRecords = context.GetTimesheetCustomReport_IndividualOverall(customReportObject.duID, customReportObject.projectID, customReportObject.subProjectID, customReportObject.employeeID, customReportObject.billable, customReportObject.startDate, customReportObject.endDate);
                    foreach (var item in customTimesheetReportListRecords)
                    {
                        viewModelList.Add(
                            new TimesheetCustomReportViewModel
                            {
                                DuId = item.DUId,
                                DuName = item.DUName,
                                EmployeeId = item.EmployeeId,
                                EmployeeName = item.EmployeeName,
                                OrgReportingManager = item.OrgReportingManager,
                                RmgManager = item.RMGManager,
                                EmployeeBillableHours = item.EmployeeBillableHours,
                                EmployeeNonBillableHours = item.EmployeeNonBillableHours,
                                OrgReportingManagerName = item.OrgReportingManagerName,
                                RmgManagerName = item.RMGManagerName
                            }
                        );
                    }
                }

                return await Task.FromResult(viewModelList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportIndividualWithProject(TimesheetCustomReportObject customReportObject)
        {
            try
            {
                List<TimesheetCustomReportViewModel> viewModelList = new List<TimesheetCustomReportViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var customTimesheetReportListRecords = context.GetTimesheetCustomReport_IndividualWithProject(customReportObject.duID, customReportObject.projectID, customReportObject.subProjectID, customReportObject.employeeID, customReportObject.billable, customReportObject.startDate, customReportObject.endDate);
                    foreach (var item in customTimesheetReportListRecords)
                    {
                        viewModelList.Add(
                            new TimesheetCustomReportViewModel
                            {
                                DuName = item.DUName,
                                EmployeeId = item.EmployeeId,
                                EmployeeName = item.EmployeeName,
                                ProjectCode = item.ProjectCode,
                                ProjectId = item.ProjectID,
                                SubProjectId = item.SubProjectID,
                                ProjectName = item.ProjectName,
                                SubProjectName = item.SubProjectName,
                                ProjectManager = item.ProjectManager,
                                SubProjectManager = item.SubProjectManager,
                                DeliveryManager = item.DeliveryManager,
                                OrgReportingManager = item.OrgReportingManager,
                                RmgManager = item.RMGManager,
                                EmployeeBillableHours = item.EmployeeBillableHours,
                                EmployeeNonBillableHours = item.EmployeeNonBillableHours,
                                ProjectManagerName = item.ProjectManagerName,
                                SubProjectManagerName = item.SubProjectManagerName,
                                DeliveryManagerName = item.DeliveryManagerName,
                                OrgReportingManagerName = item.OrgReportingManagerName,
                                RmgManagerName = item.RMGManagerName
                            }
                        );
                    }
                }

                return await Task.FromResult(viewModelList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportDULevel(TimesheetCustomReportObject customReportObject)
        {
            try
            {
                List<TimesheetCustomReportViewModel> viewModelList = new List<TimesheetCustomReportViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var customTimesheetReportListRecords = context.GetTimesheetCustomReport_DULevel(customReportObject.duID, customReportObject.projectID, customReportObject.subProjectID, customReportObject.employeeID, customReportObject.billable, customReportObject.startDate, customReportObject.endDate);
                    foreach (var item in customTimesheetReportListRecords)
                    {
                        viewModelList.Add(
                            new TimesheetCustomReportViewModel
                            {
                                DuId = item.DUId,
                                DuName = item.DUName,
                                Billable = item.Billable,
                                TotalBillableHours = item.TotalBillableHours,
                                TotalNonBillableHours = item.TotalNonBillableHours
                            }
                        );
                    }
                }

                return await Task.FromResult(viewModelList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportProjectLevel(TimesheetCustomReportObject customReportObject)
        {
            try
            {
                List<TimesheetCustomReportViewModel> viewModelList = new List<TimesheetCustomReportViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var customTimesheetReportListRecords = context.GetTimesheetCustomReport_ProjectLevel(customReportObject.duID, customReportObject.projectID, customReportObject.subProjectID, customReportObject.employeeID, customReportObject.billable, customReportObject.startDate, customReportObject.endDate);
                    foreach (var item in customTimesheetReportListRecords)
                    {
                        viewModelList.Add(
                            new TimesheetCustomReportViewModel
                            {
                                DuId = item.DUId,
                                DuName = item.DUName,
                                ProjectId = item.ProjectID,
                                SubProjectId = item.SubProjectID,
                                ProjectName = item.ProjectName,
                                SubProjectName = item.SubProjectName,
                                Billable = item.Billable,
                                ProjectManager = item.ProjectManager,
                                ProjectManagerName = item.ProjectManagerName,
                                SubProjectManager = item.SubProjectManager,
                                SubProjectManagerName = item.SubProjectManagerName,
                                TotalBillableHours = item.TotalBillableHours,
                                TotalNonBillableHours = item.TotalNonBillableHours
                            }
                        );
                    }
                }

                return await Task.FromResult(viewModelList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<TimesheetViewModel>> SearchTimesheetList(TimesheetViewModel model, int id)
        {
            var deliveryUnit = service.First<Person>(x => x.ID == id).PersonEmployment.First().DeliveryUnit;
            return await Task.Run(() =>
            {

                List<GetTimesheet_Result> taskList;

                taskList = QueryHelper.GetTimesheetResult(model.PersonID, model.startdate, model.enddate).ToList();
                var taskmodel = Mapper.Map<IEnumerable<GetTimesheet_Result>, IEnumerable<TimesheetViewModel>>(taskList);
                foreach (var model1 in taskmodel)
                {
                    model1.ProjectName = service.First<ProjectList>(x => x.ID == model1.ProjectID).ProjectName;
                    if (model1.ProjectName.Contains(projectSubstring))
                    {
                        model1.ProjectName = SplitProjectName(model1.ProjectName);
                    }

                    if (model1.SubProjectID != null && model1.SubProjectID > 0)
                    {
                        model1.SubProjectName = service.First<ProjectList>(x => x.ID == model1.SubProjectID).ProjectName;
                        if (model1.SubProjectName.Contains(projectSubstring))
                        {
                            model1.SubProjectName = SplitProjectName(model1.SubProjectName);
                        }
                    }

                    if (model1.TaskTypeId != null && model1.TaskTypeId > 0)
                    {
                        model1.TaskType = service.First<PMSTaskTypes>(x => x.Id == model1.TaskTypeId).TypeName;

                        if (model1.SubTaskId != null && model1.SubTaskId > 0)
                        {
                            model1.SubTaskType = service.First<PMSTaskTypes>(x => x.Id == model1.SubTaskId).TypeName;
                        }
                    }
                }
                return taskmodel;

            });
        }

        public async Task<TimesheetViewModel> GetTimesheet(int taskId)
        {
            return await Task.Run(() =>
            {
                var timesheet = service.Top<PMSTimesheet>(0, x => (x.ID == taskId)).FirstOrDefault();
                var model = Mapper.Map<PMSTimesheet, TimesheetViewModel>(timesheet);
                return model;
            });
        }

        public HttpResponseMessage GetDownload(List<object> reportQueryParams)
        {
            //List<Dictionary<string, string>> data, List<string> columns, 
            //  1. Fetch data from DB for given month and Year
            //  2. Create Columns   GetGenericColumnList returns List<string>
            //  3. Create data dictionary list CreateGenericDataDictionary returns List<Dictionary<string,string>>
            //  4. Create CSV inputs List<string> as column, List<Dictionary<string,string>> as data

            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            var sw = new StringBuilder();
            string id = "";

            List<string> columnsname = new List<string>();
            for (var i = 0; i < reportQueryParams.Count;)
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
                            key = (key == "PersonID") ? "EmployeeId" : key;
                            {
                                if (key == "EmployeeId")
                                    id = (string)property.Value;

                                columnsname.Add(key);
                                sw.Append("\"" + key + "\",");
                            }
                        }
                    }
                }
                break;
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
                        if (property.Key == "Date")
                        {
                            sw.AppendFormat("\"{0}\",", ((DateTime)property.Value).ToString("MM-dd-yyyy"));
                        }
                        else
                        {
                            sw.AppendFormat("\"{0}\",", property.Value);
                        }
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

        public HttpResponseMessage GetTemplate(List<object> reportQueryParams, int userId)
        {
            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            var sw = new StringBuilder();
            var deliveryUnit = service.First<Person>(x => x.ID == userId).PersonEmployment.First().DeliveryUnit;
            var grade = service.First<PersonEmployment>(x => x.PersonID == userId).Designation.Grade;

            List<string> columnsname = new List<string>();
            for (var i = 0; i < reportQueryParams.Count; i++)
            {
                JObject json = JObject.Parse(reportQueryParams[i].ToString());
                foreach (var property in json)
                {
                    var key = property.Key;
                    if (!columnsname.Contains(key))
                    {
                        columnsname.Add(key);
                        sw.Append("\"" + key + "\",");
                    }
                }
            }
            sw.Append("\r\n");

            foreach (var reportQueryParam in reportQueryParams)
            {
                JObject json = JObject.Parse(reportQueryParam.ToString());

                if (grade < 5)
                {
                    var userAllocations = service.All<PMSResourceAllocation>()
                    .Where(x => x.PersonID == userId)
                    .Select(x => x.ProjectID)
                    .ToList();

                    foreach (var allocation in userAllocations)
                    {
                        // Get the projects that are active and allocated to the user
                        var projects = service.All<ProjectList>()
                            .Where(x => x.Active == 1 && x.ID == allocation)
                            .OrderBy(x => x.ProjectName)
                            .ToList();

                        foreach (var project in projects)
                        {
                            var subprojects = service.All<ProjectList>()
                                .Where(x => x.Active == 1 && x.ParentProjId == project.ID)
                                .OrderBy(x => x.ProjectName)
                                .ToList();

                            if (subprojects.Count == 0)
                            {
                                // Append the projectName, empty subprojectName, and Date to the row
                                sw.AppendFormat("\"{0}\",", project.ProjectName);
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"{0}\",", ((DateTime)json["Date"]).ToString("MM-dd-yyyy"));
                                sw.AppendFormat("\r\n");

                            }
                            else
                            {
                                foreach (var subproject in subprojects)
                                {
                                    // Append the projectName, subprojectName, and Date to the row
                                    sw.AppendFormat("\"{0}\",", project.ProjectName);
                                    sw.AppendFormat("\"{0}\",", subproject.ProjectName);
                                    sw.AppendFormat("\"\",");
                                    sw.AppendFormat("\"\",");
                                    sw.AppendFormat("\"{0}\",", ((DateTime)json["Date"]).ToString("MM-dd-yyyy"));
                                    sw.AppendFormat("\r\n");
                                }
                            }
                        }
                    }
                    var OtherRetrieve = service.Top<ProjectList>(0, x => x.ID == 9999999).ToList();
                    foreach (var project in OtherRetrieve)
                    {
                        var subprojects = service.All<ProjectList>()
                            .Where(x => x.Active == 1 && x.ParentProjId == project.ID)
                            .OrderBy(x => x.ProjectName)
                            .ToList();

                        if (subprojects.Count == 0)
                        {
                            // Append the projectName, empty subprojectName, and Date to the row
                            sw.AppendFormat("\"{0}\",", project.ProjectName);
                            sw.AppendFormat("\"\",");
                            sw.AppendFormat("\"\",");
                            sw.AppendFormat("\"\",");
                            sw.AppendFormat("\"{0}\",", ((DateTime)json["Date"]).ToString("MM-dd-yyyy"));
                            sw.AppendFormat("\r\n");
                        }
                        else
                        {
                            foreach (var subproject in subprojects)
                            {
                                // Append the projectName, subprojectName, and Date to the row
                                sw.AppendFormat("\"{0}\",", project.ProjectName);
                                sw.AppendFormat("\"{0}\",", subproject.ProjectName);
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"{0}\",", ((DateTime)json["Date"]).ToString("MM-dd-yyyy"));
                                sw.AppendFormat("\r\n");
                            }
                        }
                    }
                }
                else
                {
                    // Get the projects that are active and allocated to the user
                    var projects = service.All<ProjectList>()
                        .Where(x => x.Active == 1 && (x.ParentProjId == 0 || x.ParentProjId == null))
                        .OrderBy(x => x.ProjectName)
                        .ToList();

                    foreach (var project in projects)
                    {
                        var subprojects = service.All<ProjectList>()
                            .Where(x => x.Active == 1 && x.ParentProjId == project.ID)
                            .OrderBy(x => x.ProjectName)
                            .ToList();

                        if (subprojects.Count == 0)
                        {
                            // Append the projectName, empty subprojectName, and Date to the row
                            sw.AppendFormat("\"{0}\",", project.ProjectName);
                            sw.AppendFormat("\"\",");
                            sw.AppendFormat("\"\",");
                            sw.AppendFormat("\"\",");
                            sw.AppendFormat("\"{0}\",", ((DateTime)json["Date"]).ToString("MM-dd-yyyy"));
                            sw.AppendFormat("\r\n");

                        }
                        else
                        {
                            foreach (var subproject in subprojects)
                            {
                                // Append the projectName, subprojectName, and Date to the row
                                sw.AppendFormat("\"{0}\",", project.ProjectName);
                                sw.AppendFormat("\"{0}\",", subproject.ProjectName);
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"\",");
                                sw.AppendFormat("\"{0}\",", ((DateTime)json["Date"]).ToString("MM-dd-yyyy"));
                                sw.AppendFormat("\r\n");
                            }
                        }
                    }
                }
            }


            Response.Content = new StringContent(sw.ToString());
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            Response.Content.Headers.ContentDisposition.FileName = "RecordExport.csv";
            return Response;
        }

        public async Task<string> Importdata(TimesheetViewModel timeSheetModel, int personId)
        {
            if (timeSheetModel == null)
                return "Payload is missing!!";
            string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"].ToString();

            return await Task.Run(async () =>
            {
                if (timeSheetModel.filename == null)
                    return "CSV file name not found, in Payload!!";

                string timeSheetUrl = uploadFolder + @"\timesheet\" + timeSheetModel.filename;
                FileInfo fileInfo = new FileInfo(timeSheetUrl);

                if (!fileInfo.Exists)
                    return "Uploaded CSV file does not exists!!";

                DataTable dt = GetDataTableFromCSVFile(timeSheetUrl);
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = dt.Rows[i];
                    bool isRowEmpty = true;
                    foreach (var cell in row.ItemArray)
                    {
                        if (cell != null && cell != DBNull.Value && !string.IsNullOrWhiteSpace(cell.ToString()))
                        {
                            isRowEmpty = false;
                            break;
                        }
                    }
                    if (isRowEmpty)
                    {
                        dt.Rows[i].Delete();
                    }
                }
                string hasImported = "";
                hasImported = await ImportDataDefault(timeSheetModel, dt, personId);

                if (hasImported == "true")
                    fileInfo.Delete();

                return hasImported ?? "";
            });
        }
        private static DataTable GetDataTableFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] excelColumns = csvReader.ReadFields();
                    foreach (string column in excelColumns)
                    {
                        DataColumn datecolumn = new DataColumn(column) { AllowDBNull = true };
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "") //Making empty value as null
                                fieldData[i] = null;
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch { }
            return csvData;
        }

        public async Task<string> ImportDataDefault(TimesheetViewModel tmmodel, DataTable dt, int personId)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string jsonStr = "";
                    bool isTaskCreated = false;
                    List<PMSTimesheet> newTimesheetEntries = new List<PMSTimesheet>();
                    int rowIndex = 0;

                    var dtTotalHours = dt.AsEnumerable()
                    .GroupBy(s => s.Field<string>("Date"))
                    .Select(sg => new
                    {
                        EntryDate = sg.First().Field<string>("Date"),
                        TotalHours = sg.Sum(s => ConvertTotalHoursToMinutes(s.Field<string>("BillableHours"))),
                        NonBillableTime = sg.Sum(s => ConvertTotalHoursToMinutes(s.Field<string>("NonBillableTime")))
                    }).ToList();

                    foreach (var entry in dtTotalHours)
                    {
                        if (entry.TotalHours > 24 * 60)
                        {
                            return $"Billable hours cannot exceed 24 hours for Date {entry.EntryDate}";
                        }

                        if (entry.NonBillableTime > 24 * 60)
                        {
                            return $"Non-billable time cannot exceed 24 hours for Date {entry.EntryDate}";
                        }
                    }

                    //Get extra columns in copyDataTable table and remaining delete
                    string[] columnList = { "ProjectName", "SubProjectName", "TaskType", "TicketId", "Date", "BillableHours", "BillableDescription", "NonBillableTime", "NonBillableDescription" };

                    DataTable copyDataTable = dt.Copy();
                    foreach (var columnName in columnList)
                        if (!copyDataTable.Columns.Contains(columnName))
                            return $"Column '{columnName}' does not exist in the DataTable.";

                    foreach (var columnName in columnList)
                        if (copyDataTable.Columns.Contains(columnName))
                            copyDataTable.Columns.Remove(columnName);

                    copyDataTable.AcceptChanges();

                    foreach (DataRow row in dt.Rows)
                    {
                        rowIndex++;
                        int iRowIndex = dt.Rows.IndexOf(row) + 1;
                        string projectName = row["ProjectName"].ToString();
                        string subProjectName = row["SubProjectName"].ToString();
                        var taskTypeName = row["TaskType"].ToString().ToLower();

                        var prjName = service.First<ProjectList>(x => x.ProjectName == projectName);
                        var taskType = service.All<PMSTaskTypes>().Where(x => x.TypeName.ToLower() == taskTypeName).ToList();
                        ProjectList subProjDetails = null;

                        if (prjName == null)
                        {
                            return $"Please enter a valid project name at row number {iRowIndex}";
                        }

                        if (!string.IsNullOrEmpty(subProjectName))
                        {
                            subProjDetails = service.First<ProjectList>(x => x.ParentProjId == prjName.ID && x.ProjectName.ToLower() == subProjectName.ToLower());

                            if (subProjDetails == null)
                            {
                                return $"Please enter a valid subproject name at row number {iRowIndex}";
                            }
                        }

                        using (var db = new PhoenixEntities())
                        {
                            var connection = db.Database.Connection;
                            connection.Open();
                            var command = connection.CreateCommand();
                            command.CommandText = "EXEC [dbo].[GetProjectAssignedWithOtherList]" + personId;

                            using (var reader = command.ExecuteReader())
                            {
                                var assignedProjectList = ((IObjectContextAdapter)db)
                                    .ObjectContext
                                    .Translate<GetDropDownProjectDetails>(reader)
                                    .ToList();

                                if (prjName == null || !assignedProjectList.Any(x => x.Id == prjName.ID))
                                {
                                    return $"Please enter a valid project name at row number {iRowIndex}";
                                }

                                if (reader.NextResult())
                                {
                                    var assignedSubProjectList = ((IObjectContextAdapter)db)
                                        .ObjectContext
                                        .Translate<GetDropDownSubProjectDetails>(reader)
                                        .ToList();

                                    if (!string.IsNullOrEmpty(subProjectName) && (subProjDetails == null || !assignedSubProjectList.Any(x => x.Id == subProjDetails.ID)))
                                    {
                                        return $"Please enter a valid subproject name at row number {iRowIndex}";
                                    }
                                }

                                if (reader.NextResult())
                                {
                                    var assignedTaskTypeList = ((IObjectContextAdapter)db)
                                        .ObjectContext
                                        .Translate<TaskTypeDetails>(reader)
                                        .ToList();

                                    if (!string.IsNullOrEmpty(taskTypeName) && (taskType == null || !taskType.Any(tt => assignedTaskTypeList.Any(at => at.Id == tt.Id))))
                                    {
                                        return $"Please enter a valid Task Type name at row number {iRowIndex}";
                                    }
                                }
                            }
                        }

                        string ticketId = row["TicketId"].ToString();

                        if (ticketId.Length > 20)
                        {
                            return $"TicketId Length should not be more than 20 characters. Error in row number {iRowIndex}.";
                        }
                       
                        if (string.IsNullOrEmpty(row["Date"].ToString()))
                        {
                            return $"The Date field cannot be empty. Error at row number {iRowIndex}.";
                        }

                        DateTime datet;
                        try
                        {
                            ParseDate(row["Date"].ToString(), out datet);
                        }
                        catch (Exception)
                        {
                            return $"Invalid date format. The date should be in 'mm/dd/yyyy' or 'dd/mm/yyyy' format. Error at row number {iRowIndex}.";
                        }

                        int billableMinutes = ConvertTotalHoursToMinutes(row["BillableHours"].ToString());
                        int nonBillableMinutes = ConvertTotalHoursToMinutes(row["NonBillableTime"].ToString());

                        if (billableMinutes == 0 && nonBillableMinutes == 0)
                        {
                            return $"At least one of BillableHours or NonBillableTime should have a value greater than 0. Error in row number {iRowIndex}.";
                        }

                        int totalMinutes = billableMinutes + nonBillableMinutes;
                        if (totalMinutes > 24 * 60)
                        {
                            return $"Total time (including billable and non-billable hours) should not exceed 24 hours. Error in row number {iRowIndex}.";
                        }

                        if (string.IsNullOrEmpty(row["BillableDescription"].ToString()) && string.IsNullOrEmpty(row["NonBillableDescription"].ToString()))
                        {
                            return $"At least one of BillableDescription or NonBillableDescription should have a value. Error in row number {iRowIndex}.";
                        }

                        if ((!string.IsNullOrEmpty(row["BillableHours"].ToString()) && string.IsNullOrEmpty(row["BillableDescription"].ToString())) ||
                            (string.IsNullOrEmpty(row["BillableHours"].ToString()) && !string.IsNullOrEmpty(row["BillableDescription"].ToString())))
                        {
                            return $"Please ensure that both Billable Hours field and Billable Description field are either filled or left empty. Error in row number {iRowIndex}.";
                        }

                        if ((!string.IsNullOrEmpty(row["NonBillableTime"].ToString()) && string.IsNullOrEmpty(row["NonBillableDescription"].ToString())) ||
                            (string.IsNullOrEmpty(row["NonBillableTime"].ToString()) && !string.IsNullOrEmpty(row["NonBillableDescription"].ToString())))
                        {
                            return $"Please ensure that both NonBillable Time field and Non-Billable Description field are either filled or left empty. Error in row number {iRowIndex}.";
                        }
                        
                        if (prjName != null)
                        {
                            tmmodel.ProjectName = prjName.ProjectName;
                            tmmodel.SubProjectName = subProjDetails == null ? "" : subProjDetails.ProjectName;
                            tmmodel.TaskType = taskType.Any() ? taskType.First().TypeName : string.Empty;
                            tmmodel.Date = datet;
                            tmmodel.TicketID = row["TicketId"].ToString();
                            tmmodel.BillableHours = row["BillableHours"].ToString();
                            tmmodel.BillableDescription = row["BillableDescription"].ToString();
                            tmmodel.TotalHours = row["BillableHours"].ToString();
                            tmmodel.Description = row["BillableDescription"].ToString();
                            tmmodel.NonBillableTime = row["NonBillableTime"].ToString();
                            tmmodel.NonBillableDescription = row["NonBillableDescription"].ToString();
                            tmmodel.PersonID = personId;
                            tmmodel.ProjectID = prjName.ID;
                            tmmodel.SubProjectID = subProjDetails?.ID ?? 0;

                            var timesheetRecords = Mapper.Map<TimesheetViewModel, PMSTimesheet>(tmmodel);

                            jsonStr = GetJSON(copyDataTable, iRowIndex);
                            if (!string.IsNullOrEmpty(jsonStr)) timesheetRecords.JsonString = jsonStr;

                            timesheetRecords.IsEmailSent = false;
                            timesheetRecords.CreatedDate = DateTime.Now;
                            timesheetRecords.UploadType = UploadTypeEnum.UploadedViaExcel.ToString();

                            if (!string.IsNullOrEmpty(tmmodel.TaskType))
                            {
                                timesheetRecords.TaskTypeId = service.First<PMSTaskTypes>(x => x.TypeName == tmmodel.TaskType).Id;
                            }
                            if (!string.IsNullOrEmpty(tmmodel.ProjectName))
                            {
                                timesheetRecords.ProjectID = service.First<ProjectList>(x => x.ProjectName == tmmodel.ProjectName).ID;
                            }

                            newTimesheetEntries.Add(timesheetRecords);
                        }
                    }

                    var timesheetDate = tmmodel.Date;
                    var EmployeeId = personId;

                    var newTotalMinutesPerDay = dtTotalHours
                        .Where(d => DateTime.TryParse(d.EntryDate, out _))
                                .GroupBy(d => DateTime.Parse(d.EntryDate).Date)
                                .Select(g => new
                                {
                                    Date = g.Key,
                                    NewTotalSumMinutes = g.Sum(e => e.TotalHours + e.NonBillableTime)
                                }).ToList();

                    var existingTimesheetEntryOfEmployeeList = service.All<PMSTimesheet>()
                                                                          .Where(v => v.PersonID == EmployeeId && v.Date == timesheetDate && !v.IsDeleted)
                                                                          .Select(g => new
                                                                          {
                                                                              ID = g.ID,
                                                                              TotalHours = g.Time,
                                                                              NonBillableTime = g.NonBillableTime,
                                                                              Date = g.Date
                                                                          })
                                                                          .ToList();

                    var existingTotalSumMinutesPerDayList = existingTimesheetEntryOfEmployeeList
                                                            .GroupBy(d => d.Date)
                                                            .Select(g => new
                                                            {
                                                                Date = g.Key,
                                                                TotalSumMinutes = g.Sum(e => ConvertTotalHoursToMinutes(e.TotalHours) + ConvertTotalHoursToMinutes(e.NonBillableTime))
                                                            })
                                                            .ToList();

                    var combinedTotalSumMinutesPerDayList = (from n in newTotalMinutesPerDay
                                                             join e in existingTotalSumMinutesPerDayList on n.Date equals e.Date

                                                             select new
                                                             {
                                                                 Date = n.Date,
                                                                 CombineTotalSumMinutes = n.NewTotalSumMinutes + e.TotalSumMinutes
                                                             })
                                                             .ToList();

                    foreach (var day in newTotalMinutesPerDay)
                    {
                        if (day.NewTotalSumMinutes > 24 * 60)
                        {
                            return $"Total hours exceed 24 hours for Date {day.Date:MM/dd/yyyy}";
                        }
                    }

                    foreach (var combinedTotalMinutes in combinedTotalSumMinutesPerDayList)
                    {
                        if (combinedTotalMinutes.CombineTotalSumMinutes > 24 * 60)
                        {
                            return $"Including existing records total hours exceed 24 hours for Date {combinedTotalMinutes.Date:MM/dd/yyyy}.";
                        }
                    }

                    foreach (var timesheetEntry in newTimesheetEntries)
                    {                   
                        isTaskCreated = service.Create<PMSTimesheet>(timesheetEntry, x => x.ID == tmmodel.ID);
                    }

                    if (isTaskCreated)
                    {
                        service.Finalize(true);
                    }

                    return "true";
                });
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
                     
        private string GetJSON(DataTable dtExcludedColumns, int rowIndex)
        {
            if (dtExcludedColumns.Columns.Count > 0)
            {
                DataTable newDataTable = dtExcludedColumns.Clone();
                newDataTable.ImportRow(dtExcludedColumns.Rows[rowIndex]);
                return JsonConvert.SerializeObject(newDataTable).Replace("[", "").Replace("]", "");
            }
            return "";
        }

        public static string ConvertIntoJson(DataTable dt)
        {
            var jsonString = new StringBuilder();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    jsonString.Append("{");

                    for (int j = 0; j < dt.Columns.Count; j++)
                        jsonString.Append("\"" + dt.Columns[j].ColumnName + "\":\""
                            + dt.Rows[i][j].ToString().Replace('"', '\"') + (j < dt.Columns.Count - 1 ? "\"," : "\""));

                    jsonString.Append(i < dt.Rows.Count - 1 ? "}," : "}");
                }
                return jsonString.ToString();
            }
            else
            {
                return "[]";
            }
        }

        private int ConvertTotalHoursToMinutes(string totalHours)
        {
            if (string.IsNullOrEmpty(totalHours)) return 0;
            if (totalHours.IndexOf(":") > -1)
            {
                var ttl = totalHours.Split(':');
                return int.Parse(ttl[0]) * 60 + int.Parse(ttl[1]);
            }
            return int.Parse(totalHours) * 60;
        }

        public static bool ParseDate(string dateString, out DateTime dateValue)
        {
            CultureInfo enUS = new CultureInfo("en-US", true);

            var formatStrings = new string[] { "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };
            if (DateTime.TryParseExact(dateString, formatStrings, enUS, DateTimeStyles.None, out dateValue))
                return true;

            dateValue = DateTime.Parse(dateString, new CultureInfo("en-US", true));
            if (DateTime.TryParse(dateValue.ToString(), out dateValue))
                return true;

            return false;
        }

        private readonly string projectSubstring = "Rev";

        private bool IsSplitRequire(string projectName)
        {
            return projectName.Contains(projectSubstring);
        }

        private string SplitProjectName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                return name = name.Split('-').Skip(2).FirstOrDefault();

            return "";
        }

        public async Task<IEnumerable<GetLedgerReport_Result>> GetLedgerReport(DateTime reportStartDate, DateTime reportEndDate, int projectId, int? personId = null, int? subProjectId = null)
        {
            return await Task.Run(() => QueryHelper.GetLedgerReport(reportStartDate, reportEndDate, projectId, personId, subProjectId)).ConfigureAwait(false);
        }

        public async Task<IEnumerable<GetSummaryReport_Result>> GetSummaryReport(DateTime reportStartDate, DateTime reportEndDate, int personId)
        {
            return await Task.Run(() => QueryHelper.GetSummaryReport(reportStartDate, reportEndDate, personId)).ConfigureAwait(false);
        }
        public async Task<bool> ShowMyTeamTab(int id)
        {
            return await Task.Run(() =>
            {
                var isMyTeamTabVisible = service.Top<ProjectList>(0, x => x.ProjectManager == id || x.DeliveryManager == id).Any();
                return isMyTeamTabVisible;
            });
        }
        private IEnumerable<EmployeeListForMyTeam> EmpListFilterByProjManager(int id)
        {
            var myEmpListData = new List<EmployeeListForMyTeam>();
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var MyEmpList = context.GetEmpListByProjManager(id);

                    foreach (var item in MyEmpList)
                    {
                        myEmpListData.Add(new EmployeeListForMyTeam()
                        {
                            ID = item.ID,
                            FirstName = item.FirstName,
                            MiddleName = item.MiddleName,
                            LastName = item.LastName,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
            return myEmpListData;
        }

        public async Task<IEnumerable<EmployeeListForMyTeam>> GetEmpListByProjectManager(int id)
        {
            return await Task.Run(() =>
            {
                return EmpListFilterByProjManager(id);
            });
        }

        public async Task<IEnumerable<TimesheetViewModel>> SearchTimesheetListByManagerProjectID(TimesheetMyTeamTabViewModel model, int id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    List<GetTimesheetByProjectID_Result> tasklist;
                    tasklist = QueryHelper.GetTimesheetByProjectID(model.PersonID, id, model.StartDate, model.EndDate).ToList();
                    IEnumerable<TimesheetViewModel> taskmodel = Mapper.Map<IEnumerable<GetTimesheetByProjectID_Result>, IEnumerable<TimesheetViewModel>>(tasklist);
                    return taskmodel;
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
            });
        }
        public Task<Dictionary<string, List<DropdownItems>>> TimeSheetLedgerReportDropdown(int userId)
        {
            var db = new PhoenixEntities();
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();
            using (var connection = db.Database.Connection)
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[GetProjDetailsByPMandDM]" + userId;

                using (var reader = command.ExecuteReader())
                {
                    var projectName =
                        ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<ProjectDetails>(reader)
                            .ToList();
                    reader.NextResult();
                    lstItems = new List<DropdownItems>();
                    foreach (var item in projectName)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = item.Id,
                            Text = item.ProjectName
                        };
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.ProjectName.ToString(), lstItems);

                    lstItems = new List<DropdownItems>();
                    var subProjectName =
                        ((IObjectContextAdapter)db)
                            .ObjectContext
                            .Translate<SubPRojectDetails>(reader)
                            .ToList();
                    foreach (var subproject in subProjectName)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = subproject.Id,
                            Text = subproject.ProjectName.Trim(),
                            AssignRole = subproject.ParentProjID
                        };
                        if (dropdownItem.Text.Contains(projectSubstring))
                        {
                            dropdownItem.Text = SplitProjectName(subproject.ProjectName);
                        }
                        lstItems.Add(dropdownItem);
                    }
                    Items.Add(TimesheetDropDownType.SubProjectName.ToString(), lstItems);
                }
            }
            return Task.Run(() => { return Items; });
        }
        public async Task<IEnumerable<GetTotalHours_Available_Reported_Result>> TotalHoursAvailableReported(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var totalData = new List<GetTotalHours_Available_Reported_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetTotalHours_Available_Reported(deliveryUnitId, startDate,endDate).ToList();
                        foreach (var item in result)
                        {
                            totalData.Add(new GetTotalHours_Available_Reported_Result()
                            {
                                TotalHoursAvailable = item.TotalHoursAvailable,
                                TotalHoursReported = item.TotalHoursReported,
                                TotalHrsAvailable_Percentage=item.TotalHrsAvailable_Percentage,
                                TotalHrsReported_Percentage=item.TotalHrsReported_Percentage
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return totalData;
            });
        }

        public async Task<IEnumerable<GetHoursForClientCompetency_Result>> GetHoursForClientCompetency(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var data = new List<GetHoursForClientCompetency_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetHoursForClientCompetency(deliveryUnitId, startDate, endDate).ToList();
                        foreach (var item in result)
                        {
                            data.Add(new GetHoursForClientCompetency_Result()
                            {
                                ProjectType = item.ProjectType,
                                TotalHours = item.TotalHours,
                                Percentage = item.Percentage,
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return data;
            });
        }

        public async Task<IEnumerable<GetHoursForOrgActivity_Result>> GetHoursForOrgActivity(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var data = new List<GetHoursForOrgActivity_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetHoursForOrgActivity(deliveryUnitId, startDate, endDate).ToList();
                        foreach (var item in result)
                        {
                            data.Add(new GetHoursForOrgActivity_Result()
                            {
                                OrgActivity = item.OrgActivity,
                                TotalHours = item.TotalHours,
                                Percentage = item.Percentage,
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return data;
            });
        }

        public async Task<IEnumerable<GetHoursForInvProject_Result>> GetHoursForInvProject(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var data = new List<GetHoursForInvProject_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetHoursForInvProject(deliveryUnitId, startDate, endDate).ToList();
                        foreach (var item in result)
                        {
                            data.Add(new GetHoursForInvProject_Result()
                            {
                                ProjectName = item.ProjectName,
                                TotalHours = item.TotalHours,
                                Percentage = item.Percentage,
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return data;
            });
        }

        public async Task<IEnumerable<GetHoursForLearning_Result>> GetHoursForLearning(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var data = new List<GetHoursForLearning_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetHoursForLearning(deliveryUnitId, startDate, endDate).ToList();
                        foreach (var item in result)
                        {
                            data.Add(new GetHoursForLearning_Result()
                            {
                                TaskTypeName = item.TaskTypeName,
                                TotalHours = item.TotalHours,
                                Percentage = item.Percentage,
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return data;
            });
        }

        public async Task<IEnumerable<GetCompliance_Hours_Percentage_Result>> TotalHoursAvailableReported_Compliance_Percentage(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var totalData = new List<GetCompliance_Hours_Percentage_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetCompliance_Hours_Percentage(deliveryUnitId, startDate, endDate).ToList();
                        foreach (var item in result)
                        {
                            totalData.Add(new GetCompliance_Hours_Percentage_Result()
                            {
                                TotalHoursAvailable = item.TotalHoursAvailable,
                                TotalHoursReported = item.TotalHoursReported,
                                Compliance_Percentage = item.Compliance_Percentage
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return totalData;
            });
        }
        public async Task<IEnumerable<TimesheetLeaveCount>> TotalLeaveCount(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var totalData = new List<TimesheetLeaveCount>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var result = context.GetTotalLeave_Count(deliveryUnitId, startDate, endDate).ToList();
                        foreach (var item in result)
                        {
                            totalData.Add(new TimesheetLeaveCount()
                            {
                                Count = item.Value
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpException(500, ex.ToString());
                }
                return totalData;
            });
        }
        public class ProjectDetails
        {
            public int Id { get; set; }
            public string ProjectName { get; set; }
        }

        public class SubPRojectDetails
        {
            public int Id { get; set; }
            public string ProjectName { get; set; }
            public int ParentProjID { get; set; }
        }
        public class GetDropDownProjectDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class GetDropDownSubProjectDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentID { get; set; }
        }
        public class TaskTypeDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class SubTaskTypeDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentID { get; set; }
        }
        public class PMSRoleDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class ReportingToDetails
        {
            public int Id { get; set; }
        }
    }
}

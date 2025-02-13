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
using Pheonix.Models.VM.Classes.ResourceAllocation;
using Pheonix.Models.VM.Classes;
using Pheonix.Models.ViewModels;

using log4net;

namespace Pheonix.Core.v1.Services.Business
{
    public class ResourceAllocationService : IResourceAllocationService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;
        private readonly PhoenixEntities _phoenixEntity;

        public ResourceAllocationService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
            _phoenixEntity = new PhoenixEntities();
        }

        #region RA Get Project List
        public async Task<IEnumerable<ResourceAllocationProjectDetails>> GetProjectList(int userId, bool rmg)
        {
            return await Task.Run(() =>
            {
                IEnumerable<ResourceAllocationProjectDetails> projList;
                IEnumerable<ResourceAllocationProjectDetails> projConfigList;
                IEnumerable<ResourceAllocationProjectDetails> projCardList;
                try
                {
                    PhoenixEntities dbContext = new PhoenixEntities();
                    if (rmg)
                    {
                        projList = (from pl in dbContext.ProjectList
                                    where pl.Active == 1
                                    select new ResourceAllocationProjectDetails
                                    {
                                        ProjectID = pl.ID,
                                        ProjectCode = pl.ProjectCode,
                                        ProjectName = pl.ProjectName,
                                        ActualStartDate = pl.ActualStartDate,
                                        ActualEndDate = pl.ActualEndDate,
                                        Status = pl.Active,
                                        BGCParameters = (from m in dbContext.CustomerBGMapping
                                                         where m.CustomerID == pl.CustomerID
                                                         select m.BGParameterID).ToList(),

                                        Resources = (from r in dbContext.PMSResourceAllocation
                                                     join p in dbContext.People on r.PersonID equals p.ID
                                                     where r.ProjectID == pl.ID && p.Active == true
                                                     select new RAResource
                                                     {
                                                         AllocationID = r.ID,
                                                         Id = r.PersonID,
                                                         FullName = dbContext.People.Where(x => x.ID == r.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                         ProjectRole = r.ProjectRole,
                                                         RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == r.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                         Billability = r.BillbleType,
                                                         BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == r.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                         Allocation = r.percentage,
                                                         StartDate = r.FromDate,
                                                         EndDate = r.ToDate,
                                                         ProjectReporting = r.ReportingTo,
                                                         EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == r.PersonID).Select(x => new RAEmploymentDetails()
                                                         {
                                                             DeliveryTeam = dbContext.ProjectList.Where(dt => dt.ID == r.ProjectID).Select(dt => dt.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                             DeliveryUnit = dbContext.ProjectList.Where(du => du.ID == r.ProjectID).Select(du => du.DeliveryUnit).FirstOrDefault(), //x.DeliveryUnit,
                                                             ResourcePool = x.ResourcePool,
                                                             WorkLocation = x.WorkLocation
                                                         }).FirstOrDefault(),
                                                         ProjectReportingName = dbContext.People.Where(x => x.ID == r.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                         ReleaseDate = r.ReleaseDate
                                                     }).ToList(),
                                        SubProjects = (from proj in dbContext.ProjectList
                                                       where proj.ParentProjId == pl.ID && proj.Active == 1
                                                       select new RASubProject
                                                       {
                                                           SubProjectID = proj.ID,
                                                           SubProjectName = proj.ProjectName
                                                       }).ToList()
                                    });
                    }
                    else
                    {
                        projList = (from pl in dbContext.ProjectList
                                    join psa in dbContext.PMSResourceAllocation on pl.ID equals psa.ProjectID
                                    where psa.PersonID == userId && pl.Active == 1
                                    select new ResourceAllocationProjectDetails
                                    {
                                        ProjectID = pl.ID,
                                        ProjectCode = pl.ProjectCode,
                                        ProjectName = pl.ProjectName,
                                        ActualStartDate = pl.ActualStartDate,
                                        ActualEndDate = pl.ActualEndDate,
                                        Status = pl.Active,
                                        Resources = (from r in dbContext.PMSResourceAllocation
                                                     where r.ProjectID == pl.ID
                                                     select new RAResource
                                                     {
                                                         AllocationID = r.ID,
                                                         Id = r.PersonID,
                                                         FullName = dbContext.People.Where(x => x.ID == r.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                         ProjectRole = r.ProjectRole,
                                                         RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == r.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                         Billability = r.BillbleType,
                                                         BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == r.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                         Allocation = r.percentage,
                                                         StartDate = r.FromDate,
                                                         EndDate = r.ToDate,
                                                         ProjectReporting = r.ReportingTo,
                                                         EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == r.PersonID).Select(x => new RAEmploymentDetails()
                                                         {
                                                             DeliveryTeam = dbContext.ProjectList.Where(dt => dt.ID == r.ProjectID).Select(dt => dt.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                             DeliveryUnit = dbContext.ProjectList.Where(du => du.ID == r.ProjectID).Select(du => du.DeliveryUnit).FirstOrDefault(), //x.DeliveryUnit,
                                                             ResourcePool = x.ResourcePool,
                                                             WorkLocation = x.WorkLocation
                                                         }
                                                         ).FirstOrDefault(),
                                                         //PMSProjectAction = dbContext.getPMSActions(r.PersonID, pl.ID).Select(x => x).ToList(),
                                                         ProjectReportingName = dbContext.People.Where(x => x.ID == r.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                         ReleaseDate = r.ReleaseDate
                                                     }).ToList(),
                                        SubProjects = (from proj in dbContext.ProjectList
                                                       where proj.ParentProjId == pl.ID && proj.Active == 1
                                                       select new RASubProject
                                                       {
                                                           SubProjectID = proj.ID,
                                                           SubProjectName = proj.ProjectName
                                                       }).ToList()
                                    });

                        projCardList = (from pl in dbContext.ProjectList
                                        where pl.Active == 1 && (pl.ProjectManager == userId || pl.DeliveryManager == userId)
                                        select new ResourceAllocationProjectDetails
                                        {
                                            ProjectID = pl.ID,
                                            ProjectCode = pl.ProjectCode,
                                            ProjectName = pl.ProjectName,
                                            ActualStartDate = pl.ActualStartDate,
                                            ActualEndDate = pl.ActualEndDate,
                                            Status = pl.Active,
                                            Resources = (from r in dbContext.PMSResourceAllocation
                                                         where r.ProjectID == pl.ID
                                                         select new RAResource
                                                         {
                                                             AllocationID = r.ID,
                                                             Id = r.PersonID,
                                                             FullName = dbContext.People.Where(x => x.ID == r.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                             ProjectRole = r.ProjectRole,
                                                             SubProjectID = null,
                                                             SubProjectName = "",
                                                             RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == r.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                             Billability = r.BillbleType,
                                                             BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == r.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                             Allocation = r.percentage,
                                                             StartDate = r.FromDate,
                                                             EndDate = r.ToDate,
                                                             ProjectReporting = r.ReportingTo,
                                                             EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == r.PersonID).Select(x => new RAEmploymentDetails()
                                                             {
                                                                 DeliveryTeam = dbContext.ProjectList.Where(dt => dt.ID == r.ProjectID).Select(dt => dt.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                 DeliveryUnit = dbContext.ProjectList.Where(du => du.ID == r.ProjectID).Select(du => du.DeliveryUnit).FirstOrDefault(), //x.DeliveryUnit,
                                                                 ResourcePool = x.ResourcePool,
                                                                 WorkLocation = x.WorkLocation
                                                             }
                                                             ).FirstOrDefault(),
                                                             ProjectReportingName = dbContext.People.Where(x => x.ID == r.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                             ReleaseDate = r.ReleaseDate
                                                         }).Union(
                                                            (from ab in dbContext.ProjectList
                                                             join
                            r in dbContext.PMSResourceAllocation on ab.ID equals r.ProjectID
                                                             where ab.ParentProjId == pl.ID
                                                             select new RAResource
                                                             {
                                                                 AllocationID = r.ID,
                                                                 Id = r.PersonID,
                                                                 FullName = dbContext.People.Where(x => x.ID == r.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                 ProjectRole = r.ProjectRole,
                                                                 SubProjectID = r.ProjectID,
                                                                 SubProjectName = dbContext.ProjectList.Where(x => x.ID == r.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                                 RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == r.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                 Billability = r.BillbleType,
                                                                 BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == r.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                                 Allocation = r.percentage,
                                                                 StartDate = r.FromDate,
                                                                 EndDate = r.ToDate,
                                                                 ProjectReporting = r.ReportingTo,
                                                                 EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == r.PersonID).Select(x => new RAEmploymentDetails()
                                                                 {
                                                                     DeliveryTeam = dbContext.ProjectList.Where(dt => dt.ID == r.ProjectID).Select(dt => dt.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                     DeliveryUnit = dbContext.ProjectList.Where(du => du.ID == r.ProjectID).Select(du => du.DeliveryUnit).FirstOrDefault(), //x.DeliveryUnit,
                                                                     ResourcePool = x.ResourcePool,
                                                                     WorkLocation = x.WorkLocation
                                                                 }
                                                                 ).FirstOrDefault(),
                                                                 //PMSProjectAction = dbContext.getPMSActions(r.PersonID, pl.ID).Select(x => x).ToList(),
                                                                 ProjectReportingName = dbContext.People.Where(x => x.ID == r.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                 ReleaseDate = r.ReleaseDate
                                                             })
                                                ).ToList(),
                                            SubProjects = (from proj in dbContext.ProjectList
                                                           where proj.ParentProjId == pl.ID && proj.Active == 1
                                                           select new RASubProject
                                                           {
                                                               SubProjectID = proj.ID,
                                                               SubProjectName = proj.ProjectName
                                                           }).ToList()
                                        });

                        projConfigList = (from pl in dbContext.ProjectList
                                          join psa in dbContext.PMSConfiguration on pl.ID equals psa.Project
                                          where psa.PersonID == userId && psa.IsDeleted == false && pl.Active == 1
                                          select new ResourceAllocationProjectDetails
                                          {
                                              ProjectID = pl.ID,
                                              ProjectCode = pl.ProjectCode,
                                              ProjectName = pl.ProjectName,
                                              ActualStartDate = pl.ActualStartDate,
                                              ActualEndDate = pl.ActualEndDate,
                                              Status = pl.Active,
                                              Resources = (from r in dbContext.PMSResourceAllocation
                                                           where r.ProjectID == pl.ID
                                                           select new RAResource
                                                           {
                                                               AllocationID = r.ID,
                                                               Id = r.PersonID,
                                                               FullName = dbContext.People.Where(x => x.ID == r.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                               ProjectRole = r.ProjectRole,
                                                               RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == r.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                               Billability = r.BillbleType,
                                                               BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == r.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                               Allocation = r.percentage,
                                                               StartDate = r.FromDate,
                                                               EndDate = r.ToDate,
                                                               ProjectReporting = r.ReportingTo,
                                                               EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == r.PersonID).Select(x => new RAEmploymentDetails()
                                                               {
                                                                   DeliveryTeam = dbContext.ProjectList.Where(dt => dt.ID == r.ProjectID).Select(dt => dt.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                   DeliveryUnit = dbContext.ProjectList.Where(du => du.ID == r.ProjectID).Select(du => du.DeliveryUnit).FirstOrDefault(), //x.DeliveryUnit,
                                                                   ResourcePool = x.ResourcePool,
                                                                   WorkLocation = x.WorkLocation
                                                               }
                                                               ).FirstOrDefault(),
                                                               //PMSProjectAction = dbContext.getPMSActions(r.PersonID, pl.ID).Select(x => x).ToList(),
                                                               ProjectReportingName = dbContext.People.Where(x => x.ID == r.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                               ReleaseDate = r.ReleaseDate
                                                           }).ToList(),
                                              SubProjects = (from proj in dbContext.ProjectList
                                                             where proj.ParentProjId == pl.ID && proj.Active == 1
                                                             select new RASubProject
                                                             {
                                                                 SubProjectID = proj.ID,
                                                                 SubProjectName = proj.ProjectName
                                                             }).ToList()
                                          });

                        List<RAProjectAccess> raProjectAccess = (from pa in dbContext.getPMSUserActions(userId).Where(p => p.ActionID == 9)
                                                                 select new RAProjectAccess
                                                                 {
                                                                     ActionId = pa.ActionID,
                                                                     ProjectID = pa.ProjectID
                                                                 }).ToList();

                        projList = (from activeItem in projList
                                    join item in raProjectAccess on activeItem.ProjectID equals item.ProjectID
                                    where item.ActionId == 9
                                    select activeItem);

                        projList = projList.Union(projCardList).Union(projConfigList).GroupBy(x => x.ProjectID).Select(x => x.First());
                    }
                    return projList;
                }
                catch
                {
                    return projList = null;
                }

            });
        }
        #endregion

        //public List<int?> GetPMSActionsResult(int personID, int project)
        //{
        //    using (var db = _phoenixEntity)
        //    {
        //        int?[] result = null;
        //        result = db.getPMSActions(personID, project).ToArray();
        //        return result;
        //    }
        //}
        //Get ResurceAllocationHistory list

        #region RA Get Current Allocation
        public async Task<IEnumerable<CurrentResourceAllocationModel>> GetResourceAllocationHistoryDetail(int PersonID)
        {
            return await Task.Run(() =>
            {
                //var taskList = service.All<PMSResourceAllocation>().Where(x => x.IsDeleted == false && x.ProjectID == ProjectID);

                PhoenixEntities dbContext = new PhoenixEntities();
                IEnumerable<CurrentResourceAllocationModel> taskList;
                IEnumerable<CurrentResourceAllocationModel> taskListprojected;
                IEnumerable<CurrentResourceAllocationModel> taskListCurrent = (from psa in dbContext.PMSResourceAllocation
                                                                               join p in dbContext.People on psa.PersonID equals p.ID
                                                                               where psa.PersonID == PersonID && psa.IsDeleted == false
                                                                               select new CurrentResourceAllocationModel
                                                                               {
                                                                                   ProjectID = psa.ProjectID,
                                                                                   PersonID = psa.PersonID,
                                                                                   ProjectName = dbContext.ProjectList.Where(x => x.ID == psa.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                                                   ResourceName = p.FirstName + " " + p.LastName,
                                                                                   percentage = psa.percentage,
                                                                                   FromDate = psa.FromDate,
                                                                                   ToDate = psa.ToDate,
                                                                                   BillbleType = psa.BillbleType,
                                                                                   BillbleTypeName = dbContext.PMSAllocationBillableType.Where(r => r.ID == psa.BillbleType).Select(r => r.Discription).FirstOrDefault(),
                                                                                   ProjectRole = psa.ProjectRole,
                                                                                   RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == psa.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                                                                   ReportingTo = psa.ReportingTo,
                                                                                   ReportingToName = dbContext.People.Where(x => x.ID == psa.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                   EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == psa.PersonID).Select(x => new RAEmploymentDetails()
                                                                                   {
                                                                                       DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == psa.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                                       DeliveryUnit = dbContext.ProjectList.Where(p => p.ID == psa.ProjectID).Select(p => p.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                                                                       ResourcePool = x.ResourcePool,
                                                                                       WorkLocation = x.WorkLocation
                                                                                   }).FirstOrDefault(),
                                                                                   ReleaseDate = psa.ReleaseDate,
                                                                                   IsProjected = false
                                                                               });
                taskListprojected = RAGetProjectedAllocationCurrent(PersonID);
                taskList = taskListCurrent.Concat(taskListprojected);
                return taskList;
            });
        }
        #endregion

        #region RA Get Other Project Employee
        public async Task<IEnumerable<ResourceViewModel>> GetOtherProjectEmployee(int projectId)
        {
            return await Task.Run(() =>
            {
                List<ResourceViewModel> lstResourceAllocationResponse = new List<ResourceViewModel>();
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {
                        //var alreadyInProject = (from psa in dbContext.PMSResourceAllocation where psa.ProjectID == projectId select psa.PersonID).ToList();
                        lstResourceAllocationResponse = (from p in dbContext.People
                                                         where p.Active == true
                                                         select new ResourceViewModel
                                                         {
                                                             PersonId = p.ID,
                                                             FirstName = p.FirstName,
                                                             LastName = p.LastName,
                                                             BGCParameters = (from b in dbContext.PersonBGMapping
                                                                              where b.BGStatus == 2
                                                                              select b.BGParameterID).ToList()
                                                         }).ToList();
                        //lstResourceAllocationResponse = lstResourceAllocationResponse.Where(e => !alreadyInProject.Contains(e.PersonId)).ToList();
                        return lstResourceAllocationResponse;

                    }
                    catch
                    {
                        return lstResourceAllocationResponse;
                    }
                }
            });
        }
        #endregion

        #region RA Raised Request
        public Task<List<ResourceAllocationResponse>> RARaisedRequest(RARaisedRequest model, int userId)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                //bool ResourceAvailability = true;
                List<ResourceAllocationResponse> lstResourceAllocationResponse = new List<ResourceAllocationResponse>();
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {
                        //insert a record in PMSAllocationRequest
                        bool isToDateValid = checkExtendToDateValid(dbContext, model.Request.ProjectID, model.Resource[0].EndDate, model.Request.SubProjectID);
                        if (isToDateValid || model.Request.RequestType == 4)
                        {

                            model.Request.ID = RARequest(dbContext, model.Request, model.Resource[0].AllocationID);
                            if (model.Request.ID > 0)
                            {
                                if (model.Request.RequestType == 1) // New Request
                                {
                                    //insert a record in PMSAllocationNewRequest
                                    List<ResourceAllocationResponse> lstnewResponse = new List<ResourceAllocationResponse>();
                                    if (model.Request.IsRmg)
                                    {
                                        RAGetRaisedRequest objRAGetRaisedRequest = new RAGetRaisedRequest();
                                        objRAGetRaisedRequest.ProjectID = model.Request.ProjectID;
                                        objRAGetRaisedRequest.SubProjectID = model.Request.SubProjectID;
                                        objRAGetRaisedRequest.RequestType = 1;
                                        List<RANewRequest> lstRANewRequest = new List<RANewRequest>();
                                        for (int i = 0; i < model.RequestDetail.Count; i++)
                                        {
                                            RANewRequest objRANewRequest = new RANewRequest();
                                            objRANewRequest.EmpID = model.RequestDetail[i].EmpID;
                                            objRANewRequest.Allocation = model.Resource[i].Allocation;
                                            objRANewRequest.Billability = model.Resource[i].Billability;
                                            objRANewRequest.StartDate = model.Resource[i].StartDate;
                                            objRANewRequest.EndDate = model.Resource[i].EndDate;
                                            objRANewRequest.ProjectReporting = model.Resource[i].ProjectReporting;
                                            objRANewRequest.ProjectRole = model.Resource[i].ProjectRole;
                                            lstRANewRequest.Add(objRANewRequest);
                                        }
                                        objRAGetRaisedRequest.RANewRequest = lstRANewRequest;
                                        lstnewResponse = RAPercentageAvailability(objRAGetRaisedRequest, dbContext);
                                        if (lstnewResponse.Any(x => x.IsSuccess == false))
                                        {
                                            return lstnewResponse;
                                        }
                                        else
                                        {
                                            lstResourceAllocationResponse = RANewRequest(dbContext, model);
                                            if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                            {
                                                return lstResourceAllocationResponse;
                                            }
                                            else
                                            {
                                                // this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lstResourceAllocationResponse = RANewRequest(dbContext, model);
                                        if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                        {
                                            return lstResourceAllocationResponse;
                                        }
                                        else
                                        {
                                            this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                        }
                                    }
                                }
                                else if (model.Request.RequestType == 2) // Update Request
                                {
                                    //insert a record in PMSAllocationUpdateRequest
                                    List<ResourceAllocationResponse> lstUpdateResponse = new List<ResourceAllocationResponse>();
                                    if (model.Request.IsRmg)
                                    {
                                        RAGetRaisedRequest objRAGetRaisedRequest = new RAGetRaisedRequest();
                                        objRAGetRaisedRequest.ProjectID = model.Request.ProjectID;
                                        objRAGetRaisedRequest.SubProjectID = model.Request.SubProjectID;
                                        objRAGetRaisedRequest.RequestType = 2;
                                        List<RAUpdateRequest> lstRAUpdateRequest = new List<RAUpdateRequest>();
                                        for (int i = 0; i < model.RequestDetail.Count; i++)
                                        {
                                            RAUpdateRequest objRAUpdateRequest = new RAUpdateRequest();
                                            objRAUpdateRequest.EmpID = model.RequestDetail[i].EmpID;
                                            objRAUpdateRequest.AllocationID = model.Resource[i].AllocationID;
                                            objRAUpdateRequest.Allocation = model.Resource[i].Allocation;
                                            objRAUpdateRequest.Billability = model.Resource[i].Billability;
                                            objRAUpdateRequest.StartDate = model.Resource[i].StartDate;
                                            objRAUpdateRequest.EndDate = model.Resource[i].EndDate;
                                            objRAUpdateRequest.ProjectReporting = model.Resource[i].ProjectReporting;
                                            objRAUpdateRequest.ProjectRole = model.Resource[i].ProjectRole;
                                            objRAUpdateRequest.ActionDate = model.Resource[i].ActionDate;
                                            lstRAUpdateRequest.Add(objRAUpdateRequest);
                                        }
                                        objRAGetRaisedRequest.RAUpdateRequest = lstRAUpdateRequest;
                                        lstUpdateResponse = RAPercentageAvailability(objRAGetRaisedRequest, dbContext);
                                        if (lstUpdateResponse.Any(x => x.IsSuccess == false))
                                        {
                                            return lstUpdateResponse;
                                        }
                                        else
                                        {
                                            lstResourceAllocationResponse = RAUpdateRequest(dbContext, model);
                                            if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                            {
                                                return lstResourceAllocationResponse;
                                            }
                                            else
                                            {
                                                // this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        lstResourceAllocationResponse = RAUpdateRequest(dbContext, model);
                                        if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                        {
                                            return lstResourceAllocationResponse;
                                        }
                                        else
                                        {
                                            this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                        }
                                    }
                                }
                                else if (model.Request.RequestType == 3) // Extention Request
                                {
                                    //insert a record in PMSAllocationExtensionRequest
                                    List<ResourceAllocationResponse> lstExtendResponse = new List<ResourceAllocationResponse>();
                                    //bool isExtendToDateValid = checkExtendToDateValid(dbContext, model.Request.ProjectID, model.Resource[0].EndDate);
                                    //if (isExtendToDateValid)
                                    //{
                                    if (model.Request.IsRmg)
                                    {
                                        RAGetRaisedRequest objRAGetRaisedRequest = new RAGetRaisedRequest();
                                        objRAGetRaisedRequest.ProjectID = model.Request.ProjectID;
                                        objRAGetRaisedRequest.SubProjectID = model.Request.SubProjectID;
                                        objRAGetRaisedRequest.RequestType = 3;
                                        List<RAExtentionRequest> lstRAExtentionRequest = new List<RAExtentionRequest>();
                                        for (int i = 0; i < model.RequestDetail.Count; i++)
                                        {
                                            RAExtentionRequest objRAExtentionRequest = new RAExtentionRequest();
                                            objRAExtentionRequest.EmpID = model.RequestDetail[i].EmpID;
                                            objRAExtentionRequest.Allocation = model.Resource[i].Allocation;
                                            objRAExtentionRequest.Billability = model.Resource[i].Billability;
                                            objRAExtentionRequest.StartDate = model.Resource[i].StartDate;
                                            objRAExtentionRequest.EndDate = model.Resource[i].EndDate;
                                            objRAExtentionRequest.ProjectReporting = model.Resource[i].ProjectReporting;
                                            objRAExtentionRequest.ProjectRole = model.Resource[i].ProjectRole;
                                            objRAExtentionRequest.ActionDate = model.Resource[i].ActionDate;
                                            lstRAExtentionRequest.Add(objRAExtentionRequest);
                                        }
                                        objRAGetRaisedRequest.RAExtentionRequest = lstRAExtentionRequest;
                                        lstExtendResponse = RAPercentageAvailability(objRAGetRaisedRequest, dbContext);
                                        if (lstExtendResponse.Any(x => x.IsSuccess == false))
                                        {
                                            return lstExtendResponse;
                                        }
                                        else
                                        {
                                            lstResourceAllocationResponse = RAExtentionRequest(dbContext, model);
                                            if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                            {
                                                return lstResourceAllocationResponse;
                                            }
                                            else
                                            {
                                                // this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        lstResourceAllocationResponse = RAExtentionRequest(dbContext, model);
                                        if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                        {
                                            return lstResourceAllocationResponse;
                                        }
                                        else
                                        {
                                            this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                        }
                                    }
                                    //}
                                    //else
                                    //{
                                    //    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                                    //    objResourceAllocationResponse.IsSuccess = false;
                                    //    objResourceAllocationResponse.Response = "Resource end date should not exceed project or customer end date";
                                    //    lstResourceAllocationResponse.Add(objResourceAllocationResponse);
                                    //}

                                }
                                else if (model.Request.RequestType == 4) // Release Request
                                {
                                    lstResourceAllocationResponse = RAReleaseRequest(dbContext, model);
                                    if (lstResourceAllocationResponse.Any(x => x.IsSuccess == false))
                                    {
                                        return lstResourceAllocationResponse;
                                    }
                                    else
                                    {
                                        if (!model.Request.IsRmg)
                                        {
                                            this.emailService.SendResourceAllocationRaisedEmail(model, userId);
                                        }
                                    }
                                }

                                isTaskCreated = true;
                            }
                        }
                        else
                        {
                            ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                            objResourceAllocationResponse.IsSuccess = false;
                            objResourceAllocationResponse.Response = "Resource end date should not exceed project or customer end date";
                            lstResourceAllocationResponse.Add(objResourceAllocationResponse);
                        }
                    }
                    catch (Exception e)
                    {
                        isTaskCreated = false;
                    }

                }

                if (isTaskCreated)
                    service.Finalize(true);

                return lstResourceAllocationResponse;
            });
        }
        private int RARequest(PhoenixEntities dbContext, RARequest objRARequest, int allocationId)
        {
            int RequestId;
            try
            {
                PMSAllocationRequest objPMSAllocationRequest = new PMSAllocationRequest();
                objPMSAllocationRequest.ProjectID = objRARequest.ProjectID;
                objPMSAllocationRequest.RequestedBy = objRARequest.RequestedBy;
                objPMSAllocationRequest.RequestType = objRARequest.RequestType;
                if (objRARequest.IsRmg)
                {
                    objPMSAllocationRequest.Status = 1; // Approve 
                }
                else
                {
                    objPMSAllocationRequest.Status = 0; // Pending
                }
                objPMSAllocationRequest.IsDeleted = false;
                objPMSAllocationRequest.StatusBy = objRARequest.RequestedBy;
                objPMSAllocationRequest.StatusDate = DateTime.Now;
                objPMSAllocationRequest.RequestDate = DateTime.Now;
                if (objRARequest.RequestType == 3) // Extension Request
                {
                    objPMSAllocationRequest.IsActionPerformed = true;
                }
                else
                {
                    objPMSAllocationRequest.IsActionPerformed = false;
                    if(objRARequest.RequestType == 2 || objRARequest.RequestType == 4)
                    {
                        objPMSAllocationRequest.PMSResourceAllocationId = allocationId;
                    }
                }
                dbContext.PMSAllocationRequest.Add(objPMSAllocationRequest);
                dbContext.SaveChanges();
                RequestId = objPMSAllocationRequest.RequestID;
            }
            catch
            {
                RequestId = 0;
            }
            return RequestId;
        }
        private List<ResourceAllocationResponse> RANewRequest(PhoenixEntities dbContext, RARaisedRequest model)
        {
            List<ResourceAllocationResponse> lstResourceAllocationResponse = new List<ResourceAllocationResponse>();
            try
            {     //insert another record
                for (int i = 0; i < model.Resource.Count; i++)
                {
                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                    PMSAllocationNewRequest objPMSAllocationNewRequest = new PMSAllocationNewRequest();
                    objPMSAllocationNewRequest.EmpID = model.RequestDetail[i].EmpID;
                    objPMSAllocationNewRequest.Percentage = model.Resource[i].Allocation;
                    objPMSAllocationNewRequest.FromDate = model.Resource[i].StartDate;
                    objPMSAllocationNewRequest.ToDate = model.Resource[i].EndDate;
                    objPMSAllocationNewRequest.BillableType = model.Resource[i].Billability;
                    objPMSAllocationNewRequest.ProjectRole = model.Resource[i].ProjectRole;
                    objPMSAllocationNewRequest.ReportingTo = model.Resource[i].ProjectReporting;
                    objPMSAllocationNewRequest.Comments = model.RequestDetail[i].Comments;
                    objPMSAllocationNewRequest.RequestID = model.Request.ID;
                    if (model.RequestDetail[i].IsRmg)
                    {
                        if (model.RequestDetail[i].RMGComments == null || model.RequestDetail[i].RMGComments == "")
                        {
                            objPMSAllocationNewRequest.RMGComments = "Approved By RMG";
                        }
                        else
                        {
                            objPMSAllocationNewRequest.RMGComments = model.RequestDetail[i].RMGComments;
                        }
                        objPMSAllocationNewRequest.Status = 1; // Approve
                        if (model.Resource[i].ActionDate.HasValue && model.Resource[i].ActionDate != DateTime.MinValue)
                        {
                            objPMSAllocationNewRequest.ActionDate = model.Resource[i].ActionDate;
                        }
                        else
                        {
                            objPMSAllocationNewRequest.ActionDate = model.Resource[i].StartDate;
                        }
                        //RAApproveDirectNewRequest(dbContext, model);
                    }
                    else
                    {
                        objPMSAllocationNewRequest.Status = 0; // Pending
                    }
                    if (model.Resource[i].IsBGCRequired)
                    {
                        objPMSAllocationNewRequest.BGStatus = 1;
                    }
                    else
                    {
                        objPMSAllocationNewRequest.BGStatus = 0;
                    }
                    objPMSAllocationNewRequest.CreatedBy = model.RequestDetail[i].CreatedBy;
                    objPMSAllocationNewRequest.ModifyBy = model.RequestDetail[i].ModifyBy;
                    objPMSAllocationNewRequest.CreatedDate = DateTime.Now;
                    objPMSAllocationNewRequest.ModifyDate = DateTime.Now;
                    objPMSAllocationNewRequest.IsDeleted = false;
                    dbContext.PMSAllocationNewRequest.Add(objPMSAllocationNewRequest);
                    dbContext.SaveChanges();
                    objResourceAllocationResponse.IsSuccess = true;
                    objResourceAllocationResponse.Response = "Request submitted successfully";
                    lstResourceAllocationResponse.Add(objResourceAllocationResponse);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstResourceAllocationResponse;
        }
        public List<ResourceAllocationResponse> RAUpdateRequest(PhoenixEntities dbContext, RARaisedRequest model)
        {
            List<ResourceAllocationResponse> lstResourceAllocationResponse = new List<ResourceAllocationResponse>();
            try
            {     //insert another record
                for (int i = 0; i < model.Resource.Count; i++)
                {
                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                    PMSAllocationUpdateRequest objPMSAllocationUpdateRequest = new PMSAllocationUpdateRequest();
                    objPMSAllocationUpdateRequest.EmpID = model.RequestDetail[i].EmpID;
                    objPMSAllocationUpdateRequest.Percentage = model.Resource[i].Allocation;
                    objPMSAllocationUpdateRequest.ToDate = model.Resource[i].EndDate;
                    objPMSAllocationUpdateRequest.BillableType = model.Resource[i].Billability;
                    objPMSAllocationUpdateRequest.ProjectRole = model.Resource[i].ProjectRole;
                    objPMSAllocationUpdateRequest.ReportingTo = model.Resource[i].ProjectReporting;
                    objPMSAllocationUpdateRequest.RequestID = model.Request.ID;
                    if (model.RequestDetail[0].IsRmg)
                    {
                        if (model.Resource[i].ActionDate.HasValue && model.Resource[i].ActionDate != DateTime.MinValue)
                        {
                            objPMSAllocationUpdateRequest.ActionDate = model.Resource[i].ActionDate.Value.AddDays(1);
                            objPMSAllocationUpdateRequest.FromDate = model.Resource[i].ActionDate.Value.AddDays(1);
                        }
                        else
                        {
                            objPMSAllocationUpdateRequest.ActionDate = model.Resource[i].EndDate.AddDays(1);
                            objPMSAllocationUpdateRequest.FromDate = model.Resource[i].EndDate.AddDays(1);
                        }
                        objPMSAllocationUpdateRequest.Status = 1; // Approve
                        if (model.RequestDetail[i].RMGComments == null || model.RequestDetail[i].RMGComments == "")
                        {
                            objPMSAllocationUpdateRequest.RMGComments = "Approved By RMG";
                        }
                        else
                        {
                            objPMSAllocationUpdateRequest.RMGComments = model.RequestDetail[i].RMGComments;
                        }
                        int projectID = model.Request.ProjectID;
                        int subProjectId = model.Request.SubProjectID;
                        int personID = model.RequestDetail[i].EmpID;
                        int allocationID = model.Resource[i].AllocationID;
                        PMSResourceAllocation ca;
                        if (allocationID > 0)
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.IsDeleted == false && x.ID == allocationID
                                  select x).FirstOrDefault();
                        }
                        else
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                                        where x.ProjectID == projectID && x.PersonID == personID && x.IsDeleted == false
                                                        select x).FirstOrDefault();
                        }

                        if (model.Resource[i].ActionDate.HasValue && model.Resource[i].ActionDate != DateTime.MinValue)
                        {
                            ca.ReleaseDate = model.Resource[i].ActionDate.Value;
                        }
                        else
                        {
                            ca.ReleaseDate = model.Resource[i].EndDate;
                        }
                        ca.ModifyBy = model.RequestDetail[i].CreatedBy;
                        ca.ModifyDate = DateTime.Now;
                        dbContext.Entry(ca).State = EntityState.Modified;
                        //RAApproveDirectUpdateRequest(dbContext, model);
                    }
                    else
                    {
                        objPMSAllocationUpdateRequest.Status = 0; // Pending
                        objPMSAllocationUpdateRequest.FromDate = model.Resource[i].StartDate;
                    }
                    objPMSAllocationUpdateRequest.Comments = model.RequestDetail[i].Comments;
                    objPMSAllocationUpdateRequest.CreatedBy = model.RequestDetail[i].CreatedBy;
                    objPMSAllocationUpdateRequest.ModifyBy = model.RequestDetail[i].ModifyBy;
                    objPMSAllocationUpdateRequest.CreatedDate = DateTime.Now;
                    objPMSAllocationUpdateRequest.ModifyDate = DateTime.Now;
                    objPMSAllocationUpdateRequest.IsDeleted = false;
                    dbContext.PMSAllocationUpdateRequest.Add(objPMSAllocationUpdateRequest);
                    dbContext.SaveChanges();
                    objResourceAllocationResponse.IsSuccess = true;
                    objResourceAllocationResponse.Response = "Request submitted successfully";
                    lstResourceAllocationResponse.Add(objResourceAllocationResponse);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstResourceAllocationResponse;
        }

        private Boolean checkExtendToDateValid(PhoenixEntities dbContext, int ProjectId, DateTime extendTodate, int subProjectId = 0)
        {
            bool isExtendToDateValid = false;
            if (subProjectId > 0)
            {
                isExtendToDateValid = (from p in dbContext.ProjectList
                                       join c in dbContext.Customer on p.CustomerID equals c.ID
                                       where p.ID == subProjectId && p.ParentProjId == ProjectId && p.Active == 1 && p.ActualEndDate >= extendTodate && c.ValidTill >= extendTodate
                                       select p).Any();
            }
            else
            {
                isExtendToDateValid = (from p in dbContext.ProjectList
                                       join c in dbContext.Customer on p.CustomerID equals c.ID
                                       where p.ID == ProjectId && p.Active == 1 && p.ActualEndDate >= extendTodate && c.ValidTill >= extendTodate
                                       select p).Any();
            }

            return isExtendToDateValid;
        }
        private List<ResourceAllocationResponse> RAExtentionRequest(PhoenixEntities dbContext, RARaisedRequest model)
        {
            List<ResourceAllocationResponse> lstResourceAllocationResponse = new List<ResourceAllocationResponse>();
            try
            {
                int projectID = model.Request.ProjectID;
                int subProjectId = model.Request.SubProjectID;
                //insert another record
                for (int i = 0; i < model.Resource.Count; i++)
                {
                    int empID = model.RequestDetail[i].EmpID;
                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                    PMSAllocationExtentionRequest objPMSAllocationExtentionRequest = new PMSAllocationExtentionRequest();
                    objPMSAllocationExtentionRequest.EmpID = model.RequestDetail[i].EmpID;
                    objPMSAllocationExtentionRequest.Percentage = model.Resource[i].Allocation;
                    objPMSAllocationExtentionRequest.FromDate = model.Resource[i].StartDate;
                    objPMSAllocationExtentionRequest.ToDate = model.Resource[i].EndDate;
                    objPMSAllocationExtentionRequest.BillableType = model.Resource[i].Billability;
                    objPMSAllocationExtentionRequest.ProjectRole = model.Resource[i].ProjectRole;
                    objPMSAllocationExtentionRequest.ReportingTo = model.Resource[i].ProjectReporting;
                    objPMSAllocationExtentionRequest.Comments = model.RequestDetail[i].Comments;
                    objPMSAllocationExtentionRequest.RequestID = model.Request.ID;
                    if (model.RequestDetail[0].IsRmg)
                    {
                        int requestID = model.Request.ID;
                        int rrID = (from r in dbContext.PMSAllocationRequest
                                    join rr in dbContext.PMSAllocationReleaseRequest on r.RequestID equals rr.RequestID
                                    where rr.EmpID == empID && r.ProjectID == projectID && r.IsActionPerformed != true
                                    select rr.ID).FirstOrDefault();
                        if (rrID > 0)
                        {
                            PMSAllocationReleaseRequest rrq = (from x in dbContext.PMSAllocationReleaseRequest
                                                               where x.ID == rrID
                                                               select x).FirstOrDefault();
                            rrq.Status = 2; // 2 is Reject
                            rrq.RMGComments = "Due to resource extension, release request has been rejected";
                            rrq.ModifyBy = model.RequestDetail[i].CreatedBy;
                            rrq.ModifyDate = DateTime.Now;
                            rrq.ActionDate = DateTime.Now;
                            dbContext.Entry(rrq).State = EntityState.Modified;

                            PMSAllocationRequest r = (from x in dbContext.PMSAllocationRequest
                                                      where x.RequestID == requestID
                                                      select x).FirstOrDefault();
                            r.Status = 2; // 2 is Reject
                            r.StatusBy = model.RequestDetail[i].CreatedBy;
                            r.StatusDate = DateTime.Now;
                            r.IsActionPerformed = true;
                            dbContext.Entry(r).State = EntityState.Modified;

                            // Send email
                            this.emailService.SendResourceAllocationActionStatusEmail(model.RequestDetail[i].CreatedBy, model.Request.RequestedBy, requestID, 4, 2, "Due to resource extension, release request has been rejected");
                        }

                        objPMSAllocationExtentionRequest.ActionDate = DateTime.Now;
                        // }
                        objPMSAllocationExtentionRequest.Status = 1; // Approve                       
                        int allocationID = model.Resource[i].AllocationID;
                        PMSResourceAllocation ca;
                        if (allocationID > 0)
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.IsDeleted == false && x.ID == allocationID
                                  select x).FirstOrDefault();
                        }
                        else
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                  select x).FirstOrDefault();
                        }

                        //if (model.Resource[i].ActionDate.HasValue && model.Resource[i].ActionDate != DateTime.MinValue)
                        //{
                        //    ca.ReleaseDate = model.Resource[i].ActionDate;
                        //}
                        //else
                        //{
                        ca.ToDate = model.Resource[i].EndDate;
                        ca.ReleaseDate = null;
                        // }
                        ca.ModifyBy = model.RequestDetail[i].CreatedBy;
                        ca.ModifyDate = DateTime.Now;
                        dbContext.Entry(ca).State = EntityState.Modified;
                        //RAApproveDirectExtentionRequest(dbContext, model);
                    }
                    else
                    {
                        objPMSAllocationExtentionRequest.Status = 0; // Pending
                    }
                    objPMSAllocationExtentionRequest.RMGComments = model.RequestDetail[i].RMGComments;
                    objPMSAllocationExtentionRequest.CreatedBy = model.RequestDetail[i].CreatedBy;
                    objPMSAllocationExtentionRequest.ModifyBy = model.RequestDetail[i].ModifyBy;
                    objPMSAllocationExtentionRequest.CreatedDate = DateTime.Now;
                    objPMSAllocationExtentionRequest.ModifyDate = DateTime.Now;
                    objPMSAllocationExtentionRequest.IsDeleted = false;
                    dbContext.PMSAllocationExtentionRequest.Add(objPMSAllocationExtentionRequest);
                    dbContext.SaveChanges();
                    if (model.RequestDetail[0].IsRmg)
                    {
                        model.Resource[i].ProjectID = projectID;
                        model.Resource[i].EmpID = empID;
                        model.Resource[i].StatusBy = model.RequestDetail[i].CreatedBy;
                        model.Resource[i].Comments = model.RequestDetail[i].Comments;
                        emailService.EmployeeUpdateEmail(model.Resource[i]);
                    }
                    objResourceAllocationResponse.IsSuccess = true;
                    objResourceAllocationResponse.Response = "Request submitted successfully";
                    lstResourceAllocationResponse.Add(objResourceAllocationResponse);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstResourceAllocationResponse;
        }
        private List<ResourceAllocationResponse> RAReleaseRequest(PhoenixEntities dbContext, RARaisedRequest model)
        {
            List<ResourceAllocationResponse> lstResourceAllocationResponse = new List<ResourceAllocationResponse>();
            try
            {     //insert another record
                for (int i = 0; i < model.Resource.Count; i++)
                {
                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                    PMSAllocationReleaseRequest objPMSAllocationReleaseRequest = new PMSAllocationReleaseRequest();
                    objPMSAllocationReleaseRequest.EmpID = model.RequestDetail[i].EmpID;
                    objPMSAllocationReleaseRequest.Percentage = model.Resource[i].Allocation;
                    objPMSAllocationReleaseRequest.FromDate = model.Resource[i].StartDate;
                    objPMSAllocationReleaseRequest.ToDate = model.Resource[i].EndDate;
                    objPMSAllocationReleaseRequest.BillableType = model.Resource[i].Billability;
                    objPMSAllocationReleaseRequest.ProjectRole = model.Resource[i].ProjectRole;
                    objPMSAllocationReleaseRequest.ReportingTo = model.Resource[i].ProjectReporting;
                    if (model.Resource[i].ActionDate.HasValue && model.Resource[i].ActionDate != DateTime.MinValue)
                    {
                        objPMSAllocationReleaseRequest.ActionDate = model.Resource[i].ActionDate.Value.AddDays(1);
                    }
                    else
                    {
                        objPMSAllocationReleaseRequest.ActionDate = model.Resource[i].EndDate.AddDays(1);
                    }
                    objPMSAllocationReleaseRequest.Comments = model.RequestDetail[i].Comments;
                    objPMSAllocationReleaseRequest.RequestID = model.Request.ID;
                    if (model.RequestDetail[0].IsRmg)
                    {
                        if (model.RequestDetail[i].RMGComments == null || model.RequestDetail[i].RMGComments == "")
                        {
                            objPMSAllocationReleaseRequest.RMGComments = "Approved By RMG";
                        }
                        else
                        {
                            objPMSAllocationReleaseRequest.RMGComments = model.RequestDetail[i].RMGComments;
                        }
                        objPMSAllocationReleaseRequest.Status = 1; // Approve
                        int projectID = model.Request.ProjectID;
                        int subProjectID = model.Request.SubProjectID;
                        int personID = model.RequestDetail[i].EmpID;
                        int allocationID = model.Resource[i].AllocationID;
                        PMSResourceAllocation ca;
                        if (allocationID > 0)
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.IsDeleted == false && x.ID == allocationID
                                  select x).FirstOrDefault();
                        }
                        else
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.ProjectID == projectID && x.PersonID == personID && x.IsDeleted == false
                                  select x).FirstOrDefault();
                        }

                        if (model.Resource[i].ActionDate.HasValue && model.Resource[i].ActionDate != DateTime.MinValue)
                        {
                            ca.ReleaseDate = model.Resource[i].ActionDate;
                        }
                        else
                        {
                            ca.ReleaseDate = model.Resource[i].EndDate;
                        }
                        ca.ModifyBy = model.RequestDetail[i].CreatedBy;
                        ca.ModifyDate = DateTime.Now;
                        dbContext.Entry(ca).State = EntityState.Modified;
                        //RAApproveDirectReleaseRequest(dbContext, model);
                    }
                    else
                    {
                        objPMSAllocationReleaseRequest.Status = 0; // Pending
                    }

                    objPMSAllocationReleaseRequest.CreatedBy = model.RequestDetail[i].CreatedBy;
                    objPMSAllocationReleaseRequest.ModifyBy = model.RequestDetail[i].ModifyBy;
                    objPMSAllocationReleaseRequest.CreatedDate = DateTime.Now;
                    objPMSAllocationReleaseRequest.ModifyDate = DateTime.Now;
                    objPMSAllocationReleaseRequest.IsDeleted = false;

                    ///////////////// Insert Record In PMSAllocationReleaseRequest ////////////////////////////////////
                    dbContext.PMSAllocationReleaseRequest.Add(objPMSAllocationReleaseRequest);
                    dbContext.SaveChanges();
                    if (model.Resource[i].Ratings.Count() > 0)
                    {
                        foreach (RARatings raRatings in model.Resource[i].Ratings)
                        {
                            PMSAllocationRatings objPMSAllocationRatings = new PMSAllocationRatings();
                            objPMSAllocationRatings.Technical = raRatings.Technical;
                            objPMSAllocationRatings.Process = raRatings.Process;
                            objPMSAllocationRatings.Discipline = raRatings.Discipline;
                            objPMSAllocationRatings.Communication = raRatings.Communication;
                            objPMSAllocationRatings.Quality = raRatings.Quality;
                            objPMSAllocationRatings.Timelines = raRatings.Timelines;
                            objPMSAllocationRatings.IsDeleted = false;
                            objPMSAllocationRatings.AllocationID = raRatings.AllocationID;
                            objPMSAllocationRatings.ReleaseRequestID = objPMSAllocationReleaseRequest.ID;
                            ///////////////// Insert Record In PMSAllocationRatings ////////////////////////////////////
                            dbContext.PMSAllocationRatings.Add(objPMSAllocationRatings);
                            foreach (RAProjectSkillRatings skillRating in raRatings.ProjectSkillRatings)
                            {
                                PMSAllocationSkillRatings objPMSAllocationSkillRatings = new PMSAllocationSkillRatings();
                                objPMSAllocationSkillRatings.ProjectSkillID = skillRating.SkillId;
                                objPMSAllocationSkillRatings.Rating = skillRating.Rating;
                                objPMSAllocationSkillRatings.IsDeleted = false;
                                objPMSAllocationSkillRatings.AllocationID = raRatings.AllocationID;
                                objPMSAllocationSkillRatings.ReleaseRequestID = objPMSAllocationReleaseRequest.ID;
                                ///////////////// Insert Record In PMSAllocationSkillRatings ////////////////////////////////////
                                dbContext.PMSAllocationSkillRatings.Add(objPMSAllocationSkillRatings);
                            }
                        }
                        dbContext.SaveChanges();
                    }
                    objResourceAllocationResponse.IsSuccess = true;
                    objResourceAllocationResponse.Response = "Request submitted successfully";
                    lstResourceAllocationResponse.Add(objResourceAllocationResponse);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstResourceAllocationResponse;
        }
        #endregion

        #region RA Get Project Skill
        public Task<List<ProjectSkillDetails>> RAGetProjectSkill(int projectID)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                //bool ResourceAvailability = true;
                List<ProjectSkillDetails> lstProjectSkill = new List<ProjectSkillDetails>();
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {
                        lstProjectSkill = (from psl in dbContext.ProjectSkill
                                           where psl.ProjectID == projectID
                                           select new ProjectSkillDetails
                                           {
                                               ID = psl.ID,
                                               ProjectID = psl.ProjectID,
                                               SkillID = psl.SkillID,
                                               SkillName = dbContext.SkillMatrices.Where(x => x.ID == psl.SkillID).Select(x => x.Name).FirstOrDefault(),
                                               IsDeleted = psl.IsDeleted
                                           }).ToList();

                        isTaskCreated = true;
                    }
                    catch
                    {

                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return lstProjectSkill;
            });
        }
        #endregion

        #region RA Get Request By Project ID
        public async Task<List<RAGetRaisedRequest>> RAGetRequestByProjectID(int userID, bool rmg)
        {
            List<RAGetRaisedRequest> raGetRaisedRequestViewModelList = new List<RAGetRaisedRequest>();
            // return await Task.Run(() =>
            // {
            try
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                List<int> rmgRoles = new List<int> { 27, 35 };
                if (rmg)
                {
                    raGetRaisedRequestViewModelList = (from r in dbContext.PMSAllocationRequest
                                                       where !dbContext.PersonInRole.Where(p => p.PersonID == r.RequestedBy).Select(p => p.RoleID).Any(x => rmgRoles.Contains(x))
                                                       //where !rmgRoles.Contains(dbContext.PersonInRole.Where(p => p.PersonID == r.RequestedBy).Select(p => p.RoleID).FirstOrDefault())
                                                       orderby r.RequestID descending
                                                       select new RAGetRaisedRequest
                                                       {
                                                           ID = r.RequestID,
                                                           ProjectID = r.ProjectID,
                                                           ProjectName = dbContext.ProjectList.Where(x => x.ID == r.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                           RequestedBy = r.RequestedBy,
                                                           RequestedByName = dbContext.People.Where(x => x.ID == r.RequestedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                           RequestDate = r.RequestDate,
                                                           Status = r.Status,
                                                           RequestType = r.RequestType,
                                                           StatusDate = r.StatusDate,
                                                           RANewRequest = (from n in dbContext.PMSAllocationNewRequest
                                                                           where n.RequestID == r.RequestID
                                                                           select new RANewRequest
                                                                           {
                                                                               Billability = n.BillableType,
                                                                               BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == n.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                                                               Comments = n.Comments,
                                                                               StartDate = n.FromDate,
                                                                               EndDate = n.ToDate,
                                                                               Allocation = n.Percentage,
                                                                               EmpID = n.EmpID,
                                                                               FullName = dbContext.People.Where(x => x.ID == n.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                               ProjectReporting = n.ReportingTo,
                                                                               ProjectReportingName = dbContext.People.Where(x => x.ID == n.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                               RequestID = n.RequestID,
                                                                               ProjectRole = n.ProjectRole,
                                                                               RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == n.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                               Status = n.Status,
                                                                               ModifyDate = n.ModifyDate,
                                                                               RMGComments = n.RMGComments,
                                                                               EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == n.EmpID).Select(x => new RAEmploymentDetails()
                                                                               {
                                                                                   DeliveryTeam = x.DeliveryTeam,
                                                                                   DeliveryUnit = x.DeliveryUnit,
                                                                                   ResourcePool = x.ResourcePool,
                                                                                   WorkLocation = x.WorkLocation
                                                                               }).FirstOrDefault(),
                                                                           }).ToList(),

                                                           RAUpdateRequest = (from u in dbContext.PMSAllocationUpdateRequest
                                                                              where u.RequestID == r.RequestID
                                                                              select new RAUpdateRequest
                                                                              {
                                                                                  Billability = u.BillableType,
                                                                                  BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == u.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                                                                  Comments = u.Comments,
                                                                                  StartDate = u.FromDate,
                                                                                  EndDate = u.ToDate,
                                                                                  Allocation = u.Percentage,
                                                                                  AllocationID = (int?)dbContext.PMSAllocationRequest
                                                                                  .Where(a => a.RequestID == u.RequestID)
                                                                                  .Select(a => a.PMSResourceAllocationId)
                                                                                  .FirstOrDefault() ??
                                                                                  (int?)dbContext.PMSResourceAllocation
                                                                                  .Where(pa => pa.ProjectID == r.ProjectID &&
                                                                                  pa.BillbleType == u.BillableType && pa.PersonID == u.EmpID)
                                                                                  .Select(pa => pa.ID).FirstOrDefault() ?? 0,
                                                                                  EmpID = u.EmpID,
                                                                                  FullName = dbContext.People.Where(x => x.ID == u.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                  ProjectReporting = u.ReportingTo,
                                                                                  ProjectReportingName = dbContext.People.Where(x => x.ID == u.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                  RequestID = u.RequestID,
                                                                                  ProjectRole = u.ProjectRole,
                                                                                  RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == u.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                                  Status = u.Status,
                                                                                  CreatedBy = u.CreatedBy,
                                                                                  CreatedDate = u.CreatedDate,
                                                                                  ModifyDate = u.ModifyDate,
                                                                                  RMGComments = u.RMGComments,
                                                                                  EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == u.EmpID).Select(x => new RAEmploymentDetails()
                                                                                  {
                                                                                      DeliveryTeam = x.DeliveryTeam,
                                                                                      DeliveryUnit = x.DeliveryUnit,
                                                                                      ResourcePool = x.ResourcePool,
                                                                                      WorkLocation = x.WorkLocation
                                                                                  }).FirstOrDefault(),
                                                                              }).ToList(),

                                                           RAExtentionRequest = (from e in dbContext.PMSAllocationExtentionRequest
                                                                                 where e.RequestID == r.RequestID
                                                                                 select new RAExtentionRequest
                                                                                 {
                                                                                     Comments = e.Comments,
                                                                                     Billability = e.BillableType,
                                                                                     BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == e.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                                                                     StartDate = e.FromDate,
                                                                                     ProjectRole = e.ProjectRole,
                                                                                     RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == e.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                                     Allocation = e.Percentage,
                                                                                     ProjectReporting = e.ReportingTo,
                                                                                     ProjectReportingName = dbContext.People.Where(x => x.ID == e.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                     EndDate = e.ToDate,
                                                                                     EmpID = e.EmpID,
                                                                                     FullName = dbContext.People.Where(x => x.ID == e.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                     RequestID = e.RequestID,
                                                                                     Status = e.Status,
                                                                                     CreatedBy = e.CreatedBy,
                                                                                     CreatedDate = e.CreatedDate,
                                                                                     ModifyBy = e.ModifyBy,
                                                                                     ModifyDate = e.ModifyDate,
                                                                                     RMGComments = e.RMGComments,
                                                                                     EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == e.EmpID).Select(x => new RAEmploymentDetails()
                                                                                     {
                                                                                         DeliveryTeam = x.DeliveryTeam,
                                                                                         DeliveryUnit = x.DeliveryUnit,
                                                                                         ResourcePool = x.ResourcePool,
                                                                                         WorkLocation = x.WorkLocation
                                                                                     }).FirstOrDefault(),
                                                                                 }).ToList(),
                                                           RAReleaseRequest = (from rr in dbContext.PMSAllocationReleaseRequest
                                                                               where rr.RequestID == r.RequestID
                                                                               select new RAReleaseRequest
                                                                               {
                                                                                   Comments = rr.Comments,
                                                                                   Billability = rr.BillableType,
                                                                                   BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == rr.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                                                                   StartDate = rr.FromDate,
                                                                                   ProjectRole = rr.ProjectRole,
                                                                                   RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == rr.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                                   Allocation = rr.Percentage,
                                                                                   ProjectReporting = rr.ReportingTo,
                                                                                   ProjectReportingName = dbContext.People.Where(x => x.ID == rr.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                   EndDate = rr.ToDate,
                                                                                   EmpID = rr.EmpID,
                                                                                   FullName = dbContext.People.Where(x => x.ID == rr.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                   RequestID = r.RequestID,
                                                                                   Status = rr.Status,
                                                                                   CreatedBy = rr.CreatedBy,
                                                                                   CreatedDate = rr.CreatedDate,
                                                                                   ModifyBy = rr.ModifyBy,
                                                                                   ModifyDate = rr.ModifyDate,
                                                                                   RMGComments = rr.RMGComments,
                                                                                   ActionDate = DbFunctions.AddDays(rr.ActionDate, -1),
                                                                                   EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == rr.EmpID).Select(x => new RAEmploymentDetails()
                                                                                   {
                                                                                       DeliveryTeam = x.DeliveryTeam,
                                                                                       DeliveryUnit = x.DeliveryUnit,
                                                                                       ResourcePool = x.ResourcePool,
                                                                                       WorkLocation = x.WorkLocation
                                                                                   }).FirstOrDefault(),
                                                                                   Ratings = (from ar in dbContext.PMSAllocationRatings
                                                                                              where ar.ReleaseRequestID == rr.ID
                                                                                              select new RARatings
                                                                                              {
                                                                                                  Communication = ar.Communication,
                                                                                                  Discipline = ar.Discipline,
                                                                                                  Process = ar.Process,
                                                                                                  Quality = ar.Quality,
                                                                                                  Technical = ar.Technical,
                                                                                                  Timelines = ar.Timelines,
                                                                                                  ProjectSkillRatings = (from sr in dbContext.PMSAllocationSkillRatings
                                                                                                                         where sr.ReleaseRequestID == rr.ID
                                                                                                                         select new RAProjectSkillRatings
                                                                                                                         {
                                                                                                                             SkillId = sr.ProjectSkillID,
                                                                                                                             Rating = sr.Rating,
                                                                                                                         }).ToList()
                                                                                              }).ToList(),
                                                                               }).ToList()

                                                       }).ToList();


                }
                else
                {

                    raGetRaisedRequestViewModelList = (from r in dbContext.PMSAllocationRequest
                                                       where r.RequestedBy == userID && !rmgRoles.Contains(dbContext.PersonInRole.Where(p => p.PersonID == r.RequestedBy).Select(p => p.RoleID).FirstOrDefault())
                                                       orderby r.RequestID descending
                                                       select new RAGetRaisedRequest
                                                       {
                                                           ID = r.RequestID,
                                                           ProjectID = r.ProjectID,
                                                           ProjectName = dbContext.ProjectList.Where(x => x.ID == r.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                           RequestedBy = r.RequestedBy,
                                                           RequestedByName = dbContext.People.Where(x => x.ID == r.RequestedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                           RequestDate = r.RequestDate,
                                                           Status = r.Status,
                                                           RequestType = r.RequestType,
                                                           StatusDate = r.StatusDate,
                                                           RANewRequest = (from n in dbContext.PMSAllocationNewRequest
                                                                           where n.RequestID == r.RequestID
                                                                           select new RANewRequest
                                                                           {
                                                                               Billability = n.BillableType,
                                                                               Comments = n.Comments,
                                                                               StartDate = n.FromDate,
                                                                               EndDate = n.ToDate,
                                                                               Allocation = n.Percentage,
                                                                               EmpID = n.EmpID,
                                                                               FullName = dbContext.People.Where(x => x.ID == n.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                               ProjectReporting = n.ReportingTo,
                                                                               ProjectReportingName = dbContext.People.Where(x => x.ID == n.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                               RequestID = n.RequestID,
                                                                               ProjectRole = n.ProjectRole,
                                                                               RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == n.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                               Status = n.Status,
                                                                               ModifyDate = n.ModifyDate,
                                                                               RMGComments = n.RMGComments,
                                                                               EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == n.EmpID).Select(x => new RAEmploymentDetails()
                                                                               {
                                                                                   DeliveryTeam = x.DeliveryTeam,
                                                                                   DeliveryUnit = x.DeliveryUnit,
                                                                                   ResourcePool = x.ResourcePool,
                                                                                   WorkLocation = x.WorkLocation
                                                                               }).FirstOrDefault(),
                                                                           }).ToList(),

                                                           RAUpdateRequest = (from u in dbContext.PMSAllocationUpdateRequest
                                                                              where u.RequestID == r.RequestID
                                                                              select new RAUpdateRequest
                                                                              {
                                                                                  Billability = u.BillableType,
                                                                                  Comments = u.Comments,
                                                                                  StartDate = u.FromDate,
                                                                                  EndDate = u.ToDate,
                                                                                  Allocation = u.Percentage,
                                                                                  AllocationID = (int?)dbContext.PMSAllocationRequest
                                                                                  .Where(a => a.RequestID == u.RequestID)
                                                                                  .Select(a => a.PMSResourceAllocationId)
                                                                                  .FirstOrDefault() ??
                                                                                  (int?)dbContext.PMSResourceAllocation
                                                                                  .Where(pa => pa.ProjectID == r.ProjectID &&
                                                                                  pa.BillbleType == u.BillableType && pa.PersonID == u.EmpID)
                                                                                  .Select(pa => pa.ID).FirstOrDefault() ?? 0,
                                                                                  EmpID = u.EmpID,
                                                                                  FullName = dbContext.People.Where(x => x.ID == u.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                  ProjectReporting = u.ReportingTo,
                                                                                  ProjectReportingName = dbContext.People.Where(x => x.ID == u.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                  RequestID = u.RequestID,
                                                                                  ProjectRole = u.ProjectRole,
                                                                                  RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == u.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                                  Status = u.Status,
                                                                                  CreatedBy = u.CreatedBy,
                                                                                  CreatedDate = u.CreatedDate,
                                                                                  ModifyDate = u.ModifyDate,
                                                                                  RMGComments = u.RMGComments,
                                                                                  EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == u.EmpID).Select(x => new RAEmploymentDetails()
                                                                                  {
                                                                                      DeliveryTeam = x.DeliveryTeam,
                                                                                      DeliveryUnit = x.DeliveryUnit,
                                                                                      ResourcePool = x.ResourcePool,
                                                                                      WorkLocation = x.WorkLocation
                                                                                  }).FirstOrDefault(),
                                                                              }).ToList(),

                                                           RAExtentionRequest = (from e in dbContext.PMSAllocationExtentionRequest
                                                                                 where e.RequestID == r.RequestID
                                                                                 select new RAExtentionRequest
                                                                                 {
                                                                                     Comments = e.Comments,
                                                                                     Billability = e.BillableType,
                                                                                     BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == e.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                                                                     StartDate = e.FromDate,
                                                                                     ProjectRole = e.ProjectRole,
                                                                                     RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == e.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                                     Allocation = e.Percentage,
                                                                                     ProjectReporting = e.ReportingTo,
                                                                                     ProjectReportingName = dbContext.People.Where(x => x.ID == e.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                     EndDate = e.ToDate,
                                                                                     EmpID = e.EmpID,
                                                                                     FullName = dbContext.People.Where(x => x.ID == e.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                     RequestID = e.RequestID,
                                                                                     Status = e.Status,
                                                                                     CreatedBy = e.CreatedBy,
                                                                                     CreatedDate = e.CreatedDate,
                                                                                     ModifyBy = e.ModifyBy,
                                                                                     ModifyDate = e.ModifyDate,
                                                                                     RMGComments = e.RMGComments,
                                                                                     EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == e.EmpID).Select(x => new RAEmploymentDetails()
                                                                                     {
                                                                                         DeliveryTeam = x.DeliveryTeam,
                                                                                         DeliveryUnit = x.DeliveryUnit,
                                                                                         ResourcePool = x.ResourcePool,
                                                                                         WorkLocation = x.WorkLocation
                                                                                     }).FirstOrDefault(),
                                                                                 }).ToList(),
                                                           RAReleaseRequest = (from rr in dbContext.PMSAllocationReleaseRequest
                                                                               where rr.RequestID == r.RequestID
                                                                               select new RAReleaseRequest
                                                                               {
                                                                                   Comments = rr.Comments,
                                                                                   Billability = rr.BillableType,
                                                                                   BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == rr.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                                                                   StartDate = rr.FromDate,
                                                                                   ProjectRole = rr.ProjectRole,
                                                                                   RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == rr.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                                   Allocation = rr.Percentage,
                                                                                   ProjectReporting = rr.ReportingTo,
                                                                                   ProjectReportingName = dbContext.People.Where(x => x.ID == rr.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                   EndDate = rr.ToDate,
                                                                                   EmpID = rr.EmpID,
                                                                                   FullName = dbContext.People.Where(x => x.ID == rr.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                                   RequestID = r.RequestID,
                                                                                   Status = rr.Status,
                                                                                   CreatedBy = rr.CreatedBy,
                                                                                   CreatedDate = rr.CreatedDate,
                                                                                   ModifyBy = rr.ModifyBy,
                                                                                   ModifyDate = rr.ModifyDate,
                                                                                   RMGComments = rr.RMGComments,
                                                                                   ActionDate = DbFunctions.AddDays(rr.ActionDate, -1),
                                                                                   EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == rr.EmpID).Select(x => new RAEmploymentDetails()
                                                                                   {
                                                                                       DeliveryTeam = x.DeliveryTeam,
                                                                                       DeliveryUnit = x.DeliveryUnit,
                                                                                       ResourcePool = x.ResourcePool,
                                                                                       WorkLocation = x.WorkLocation
                                                                                   }).FirstOrDefault(),
                                                                                   Ratings = (from ar in dbContext.PMSAllocationRatings
                                                                                              where ar.ReleaseRequestID == rr.ID
                                                                                              select new RARatings
                                                                                              {
                                                                                                  Communication = ar.Communication,
                                                                                                  Discipline = ar.Discipline,
                                                                                                  Process = ar.Process,
                                                                                                  Quality = ar.Quality,
                                                                                                  Technical = ar.Technical,
                                                                                                  Timelines = ar.Timelines,
                                                                                                  ProjectSkillRatings = (from sr in dbContext.PMSAllocationSkillRatings
                                                                                                                         where sr.ReleaseRequestID == rr.ID
                                                                                                                         select new RAProjectSkillRatings
                                                                                                                         {
                                                                                                                             SkillId = sr.ProjectSkillID,
                                                                                                                             Rating = sr.Rating,
                                                                                                                         }).ToList()
                                                                                              }).ToList(),
                                                                               }).ToList()

                                                       }).ToList();
                }

                return await Task.Run(() => { return raGetRaisedRequestViewModelList; });
            }
            catch
            {
                return raGetRaisedRequestViewModelList;
            }
        }
        #endregion

        #region RA Delete Request
        public Task<bool> RADeleteFullRaisedRequest(int requestId, int requestType)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {

                        //Remove First record
                        switch (requestType)
                        {
                            case 1:
                                var n = from x in dbContext.PMSAllocationNewRequest
                                        where x.RequestID == requestId && x.IsDeleted == false
                                        select x;

                                foreach (PMSAllocationNewRequest requestDetails in n.ToList())
                                {
                                    dbContext.Entry(requestDetails).State = EntityState.Deleted;
                                }
                                break;
                            case 2:
                                var u = from x in dbContext.PMSAllocationUpdateRequest
                                        where x.RequestID == requestId && x.IsDeleted == false
                                        select x;

                                foreach (PMSAllocationUpdateRequest requestDetails in u.ToList())
                                {
                                    dbContext.Entry(requestDetails).State = EntityState.Deleted;
                                }
                                break;
                            case 3:
                                var e = from x in dbContext.PMSAllocationExtentionRequest
                                        where x.RequestID == requestId && x.IsDeleted == false
                                        select x;

                                foreach (PMSAllocationExtentionRequest requestDetails in e.ToList())
                                {
                                    dbContext.Entry(requestDetails).State = EntityState.Deleted;
                                }
                                break;
                            case 4:
                                var rr = from x in dbContext.PMSAllocationReleaseRequest
                                         where x.RequestID == requestId && x.IsDeleted == false
                                         select x;
                                foreach (PMSAllocationReleaseRequest releaseRequest in rr.ToList())
                                {
                                    var sr = from x in dbContext.PMSAllocationSkillRatings
                                             where x.ReleaseRequestID == releaseRequest.ID && x.IsDeleted == false
                                             select x;
                                    foreach (PMSAllocationSkillRatings skillRatingsDetails in sr.ToList())
                                    {
                                        dbContext.Entry(skillRatingsDetails).State = EntityState.Deleted;
                                    }

                                    var r = from x in dbContext.PMSAllocationRatings
                                            where x.ReleaseRequestID == releaseRequest.ID && x.IsDeleted == false
                                            select x;
                                    foreach (PMSAllocationRatings ratingsDetails in r.ToList())
                                    {
                                        dbContext.Entry(ratingsDetails).State = EntityState.Deleted;
                                    }

                                    dbContext.Entry(releaseRequest).State = EntityState.Deleted;
                                }
                                break;
                            default:
                                Console.WriteLine("Invalid Request Type");
                                break;
                        }

                        //Remove another record          
                        PMSAllocationRequest par = (from x in dbContext.PMSAllocationRequest
                                                    where x.RequestID == requestId
                                                    select x).First();

                        dbContext.Entry(par).State = EntityState.Deleted;
                        dbContext.SaveChanges();

                        isTaskCreated = true;
                    }
                    catch
                    {

                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return isTaskCreated;
            });
        }
        #endregion

        #region RA Edit request
        public Task<bool> RAEditFullRaisedRequest(RAGetRaisedRequest model)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {
                        switch (model.RequestType)
                        {
                            case 1:
                                isTaskCreated = RAEditNewRequest(dbContext, model);
                                if (isTaskCreated)
                                {
                                    this.emailService.SendResourceAllocationUpdatedEmail(model, 0);
                                }
                                break;
                            case 2:
                                isTaskCreated = RAEditUpdateRequest(dbContext, model);
                                if (isTaskCreated)
                                {
                                    this.emailService.SendResourceAllocationUpdatedEmail(model, 0);
                                }
                                break;
                            case 3:
                                isTaskCreated = RAEditExtentionRequest(dbContext, model);
                                if (isTaskCreated)
                                {
                                    this.emailService.SendResourceAllocationUpdatedEmail(model, 0);
                                }
                                break;
                            case 4:
                                isTaskCreated = RAEditReleaseRequest(dbContext, model);
                                if (isTaskCreated)
                                {
                                    this.emailService.SendResourceAllocationUpdatedEmail(model, 0);
                                }
                                break;
                            default:
                                Console.WriteLine("Invalid Request Type");
                                break;
                        }
                        dbContext.SaveChanges();
                        //isTaskCreated = true;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return isTaskCreated;
            });
        }
        private bool RAEditNewRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            IEnumerable<PMSAllocationNewRequest> newRequestDB = from x in dbContext.PMSAllocationNewRequest
                                                                where x.RequestID == model.ID && x.IsDeleted == false
                                                                select x;

            List<PMSAllocationNewRequest> lstModel = new List<PMSAllocationNewRequest>();
            foreach (RANewRequest newRequest in model.RANewRequest.ToList())
            {
                PMSAllocationNewRequest objModel = new PMSAllocationNewRequest();
                objModel.EmpID = newRequest.EmpID;
                objModel.FromDate = newRequest.StartDate;
                objModel.ToDate = newRequest.EndDate;
                objModel.Percentage = newRequest.Allocation;
                objModel.ProjectRole = newRequest.ProjectRole;
                objModel.ReportingTo = newRequest.ProjectReporting;
                objModel.BillableType = newRequest.Billability;
                objModel.Comments = newRequest.Comments;
                objModel.RequestID = newRequest.RequestID;
                if (newRequest.RMGComments == null || newRequest.RMGComments == "")
                {
                    objModel.RMGComments = "Approved By RMG";
                }
                else
                {
                    objModel.RMGComments = newRequest.RMGComments;
                }
                objModel.Status = newRequest.Status;
                objModel.CreatedBy = newRequest.CreatedBy;
                objModel.CreatedDate = newRequest.CreatedDate;
                objModel.ModifyBy = newRequest.ModifyBy;
                objModel.ModifyDate = newRequest.ModifyDate;
                if (newRequest.ActionDate == DateTime.MinValue || newRequest.ActionDate == null)
                {
                    objModel.ActionDate = newRequest.StartDate;
                }
                else
                {
                    objModel.ActionDate = newRequest.ActionDate;
                }
                lstModel.Add(objModel);
            }
            //var comparer = new NewRequestEqualityComparer();
            var itemsToDelete = newRequestDB.Except(lstModel, new NewRequestEqualityComparer()).AsEnumerable();
            foreach (PMSAllocationNewRequest item in itemsToDelete)
            {
                dbContext.Entry(item).State = EntityState.Deleted;
            }

            var itemsToUpdate = from ndb in newRequestDB
                                join nlm in lstModel
                                on ndb.EmpID equals nlm.EmpID
                                select new
                                {
                                    ndb,
                                    nlm
                                };

            foreach (var item in itemsToUpdate)
            {
                if (!item.ndb.Percentage.Equals(item.nlm.Percentage))
                {
                    item.ndb.Percentage = item.nlm.Percentage;
                }
                if (!item.ndb.FromDate.Equals(item.nlm.FromDate))
                {
                    item.ndb.FromDate = item.nlm.FromDate;
                }
                if (!item.ndb.ToDate.Equals(item.nlm.ToDate))
                {
                    item.ndb.ToDate = item.nlm.ToDate;
                }
                if (!item.ndb.BillableType.Equals(item.nlm.BillableType))
                {
                    item.ndb.BillableType = item.nlm.BillableType;
                }
                if (!item.ndb.ProjectRole.Equals(item.nlm.ProjectRole))
                {
                    item.ndb.ProjectRole = item.nlm.ProjectRole;
                }
                if (!item.ndb.ReportingTo.Equals(item.nlm.ReportingTo))
                {
                    item.ndb.ReportingTo = item.nlm.ReportingTo;
                }
                if (model.IsRmg)
                {
                    item.ndb.Status = 1;
                }
                else
                {
                    item.ndb.Status = 0;
                }
                item.ndb.ModifyBy = model.RequestedBy;
                item.ndb.ModifyDate = DateTime.Now;
                item.ndb.Comments = model.RANewRequest[0].Comments;
                if (model.RANewRequest[0].RMGComments == null || model.RANewRequest[0].RMGComments == "")
                {
                    item.ndb.RMGComments = "Updated By RMG";
                }
                else
                {
                    item.ndb.RMGComments = model.RANewRequest[0].RMGComments;
                }
                if (model.RANewRequest[0].ActionDate == DateTime.MinValue || model.RANewRequest[0].ActionDate == null)
                {
                    item.ndb.ActionDate = model.RANewRequest[0].StartDate;
                }
                else
                {
                    item.ndb.ActionDate = model.RANewRequest[0].ActionDate;
                }
                dbContext.Entry(item.ndb).State = EntityState.Modified;
            }

            var itemsToAdd = lstModel.Except(newRequestDB, new NewRequestEqualityComparer()).ToList();
            foreach (PMSAllocationNewRequest item in itemsToAdd)
            {
                if (model.IsRmg)
                {
                    item.Status = 1;
                }
                else
                {
                    item.Status = 0;
                }
                item.CreatedDate = DateTime.Now;
                item.CreatedBy = model.RequestedBy;
                item.RequestID = model.ID;
                item.ModifyDate = DateTime.Now;
                item.ModifyBy = model.RequestedBy;
                item.Comments = model.RANewRequest[0].Comments;
                if (model.RANewRequest[0].ActionDate == DateTime.MinValue || model.RANewRequest[0].ActionDate == null)
                {
                    item.ActionDate = model.RANewRequest[0].StartDate;
                }
                else
                {
                    item.ActionDate = model.RANewRequest[0].ActionDate;
                }

                dbContext.PMSAllocationNewRequest.Add(item);
            }

            dbContext.SaveChanges();
            isEdit = true;
            return isEdit;
        }
        private class NewRequestEqualityComparer : IEqualityComparer<PMSAllocationNewRequest>
        {
            public int GetHashCode(PMSAllocationNewRequest obj)
            {
                return (obj == null) ? 0 : obj.EmpID.GetHashCode();
            }
            public bool Equals(PMSAllocationNewRequest x, PMSAllocationNewRequest y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x == null || y == null) return false;
                return x.EmpID == y.EmpID;
            }
        }
        private bool RAEditUpdateRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            var u = from x in dbContext.PMSAllocationUpdateRequest
                    where x.RequestID == model.ID && x.IsDeleted == false
                    select x;
            foreach (PMSAllocationUpdateRequest updateRequest in u.ToList())
            {
                // Since for all the elements value should be same take zero index for all items
                if (!updateRequest.Percentage.Equals(model.RAUpdateRequest[0].Allocation))
                {
                    updateRequest.Percentage = model.RAUpdateRequest[0].Allocation;
                }
                if (!updateRequest.FromDate.Equals(model.RAUpdateRequest[0].StartDate))
                {
                    updateRequest.FromDate = model.RAUpdateRequest[0].StartDate;
                }
                if (!updateRequest.ToDate.Equals(model.RAUpdateRequest[0].EndDate))
                {
                    updateRequest.ToDate = model.RAUpdateRequest[0].EndDate;
                }
                if (!updateRequest.BillableType.Equals(model.RAUpdateRequest[0].Billability))
                {
                    updateRequest.BillableType = model.RAUpdateRequest[0].Billability;
                }
                if (!updateRequest.ProjectRole.Equals(model.RAUpdateRequest[0].ProjectRole))
                {
                    updateRequest.ProjectRole = model.RAUpdateRequest[0].ProjectRole;
                }
                if (!updateRequest.ReportingTo.Equals(model.RAUpdateRequest[0].ProjectReporting))
                {
                    updateRequest.ReportingTo = model.RAUpdateRequest[0].ProjectReporting;
                }
                updateRequest.ModifyBy = model.RequestedBy;
                updateRequest.ModifyDate = DateTime.Now;
                updateRequest.Comments = model.RAUpdateRequest[0].Comments;
                dbContext.Entry(updateRequest).State = EntityState.Modified;
                dbContext.SaveChanges();
                isEdit = true;
            }
            return isEdit;
        }
        private bool RAEditExtentionRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            var e = from x in dbContext.PMSAllocationExtentionRequest
                    where x.RequestID == model.ID && x.IsDeleted == false
                    select x;
            foreach (PMSAllocationExtentionRequest extentionRequestDetails in e.ToList())
            {
                // Since for all the elements value should be same take zero index for all items
                if (!extentionRequestDetails.ToDate.Equals(model.RAExtentionRequest[0].EndDate))
                {
                    extentionRequestDetails.ToDate = model.RAExtentionRequest[0].EndDate;
                }
                extentionRequestDetails.ModifyBy = model.RequestedBy;
                extentionRequestDetails.ModifyDate = DateTime.Now;
                extentionRequestDetails.Comments = model.RAExtentionRequest[0].Comments;
                dbContext.Entry(extentionRequestDetails).State = EntityState.Modified;
                dbContext.SaveChanges();
                isEdit = true;
            }
            return isEdit;
        }
        private bool RAEditReleaseRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            var rr = from x in dbContext.PMSAllocationReleaseRequest
                     where x.RequestID == model.ID && x.IsDeleted == false
                     select x;
            foreach (PMSAllocationReleaseRequest releaseRequest in rr.ToList())
            {
                List<PMSAllocationSkillRatings> dbsr = (from x in dbContext.PMSAllocationSkillRatings
                                                        where x.ReleaseRequestID == releaseRequest.ID && x.IsDeleted == false
                                                        select x).ToList();
                List<RAProjectSkillRatings> modelsr = model.RAReleaseRequest[0].Ratings[0].ProjectSkillRatings;
                for (int i = 0; i < dbsr.Count(); i++)
                {
                    if (modelsr[i].SkillId == dbsr[i].ProjectSkillID)
                    {
                        dbsr[i].Rating = model.RAReleaseRequest[0].Ratings[0].ProjectSkillRatings[i].Rating;
                        dbContext.Entry(dbsr[i]).State = EntityState.Modified;
                    }
                }
                modelsr = modelsr.Where(x => !dbsr.Any(y => y.ProjectSkillID == x.SkillId)).ToList();
                if (modelsr.Count > 0)
                {
                    for (int i = 0; i < modelsr.Count(); i++)
                    {
                        dbsr[i].ProjectSkillID = model.RAReleaseRequest[0].Ratings[0].ProjectSkillRatings[i].SkillId;
                        dbsr[i].Rating = model.RAReleaseRequest[0].Ratings[0].ProjectSkillRatings[i].Rating;
                        dbsr[i].ReleaseRequestID = releaseRequest.ID;
                        dbsr[i].AllocationID = dbsr[i].AllocationID;
                        dbsr[i].IsDeleted = false;
                        dbContext.Entry(dbsr[i]).State = EntityState.Added;
                    }
                }

                var r = from x in dbContext.PMSAllocationRatings
                        where x.ReleaseRequestID == releaseRequest.ID && x.IsDeleted == false
                        select x;

                foreach (PMSAllocationRatings ratingsDetails in r.ToList())
                {
                    if (!ratingsDetails.Technical.Equals(model.RAReleaseRequest[0].Ratings[0].Technical))
                    {
                        ratingsDetails.Technical = model.RAReleaseRequest[0].Ratings[0].Technical;
                    }
                    if (!ratingsDetails.Timelines.Equals(model.RAReleaseRequest[0].Ratings[0].Timelines))
                    {
                        ratingsDetails.Timelines = model.RAReleaseRequest[0].Ratings[0].Timelines;
                    }
                    if (!ratingsDetails.Process.Equals(model.RAReleaseRequest[0].Ratings[0].Process))
                    {
                        ratingsDetails.Process = model.RAReleaseRequest[0].Ratings[0].Process;
                    }
                    if (!ratingsDetails.Quality.Equals(model.RAReleaseRequest[0].Ratings[0].Quality))
                    {
                        ratingsDetails.Quality = model.RAReleaseRequest[0].Ratings[0].Quality;
                    }
                    if (!ratingsDetails.Communication.Equals(model.RAReleaseRequest[0].Ratings[0].Communication))
                    {
                        ratingsDetails.Communication = model.RAReleaseRequest[0].Ratings[0].Communication;
                    }
                    dbContext.Entry(ratingsDetails).State = EntityState.Modified;
                }
                // Since for all the elements value should be same take zero index for all items
                if (!releaseRequest.ToDate.Equals(model.RAReleaseRequest[0].EndDate))
                {
                    releaseRequest.ToDate = model.RAReleaseRequest[0].EndDate;
                }
                releaseRequest.ModifyBy = model.RequestedBy;
                releaseRequest.ModifyDate = DateTime.Now;
                releaseRequest.Comments = model.RAReleaseRequest[0].Comments;
                dbContext.Entry(releaseRequest).State = EntityState.Modified;
                dbContext.SaveChanges();
                isEdit = true;
            }
            return isEdit;
        }
        #endregion

        #region RA Update Current Allocation Using Request Data
        public bool RAApproveDirectNewRequest(List<RAResource> model)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                bool isSuccess = false;
                try
                {

                    for (int i = 0; i < model.Count; i++)
                    {
                        PMSResourceAllocation objPMSResourceAllocation = new PMSResourceAllocation();
                        objPMSResourceAllocation.ProjectID = model[i].ProjectID;
                        objPMSResourceAllocation.PersonID = model[i].EmpID;
                        objPMSResourceAllocation.ReportingTo = model[i].ProjectReporting;
                        objPMSResourceAllocation.ProjectRole = model[i].ProjectRole;
                        objPMSResourceAllocation.ToDate = model[i].EndDate;
                        objPMSResourceAllocation.FromDate = model[i].StartDate;
                        objPMSResourceAllocation.percentage = model[i].Allocation;
                        objPMSResourceAllocation.BillbleType = model[i].Billability;
                        objPMSResourceAllocation.IsDeleted = false;
                        objPMSResourceAllocation.CreatedBy = model[i].StatusBy;
                        objPMSResourceAllocation.CreatedDate = DateTime.Now;
                        objPMSResourceAllocation.ModifyBy = model[i].StatusBy;
                        objPMSResourceAllocation.ModifyDate = DateTime.Now;
                        if (model[i].IsBGCRequired)
                        {
                            objPMSResourceAllocation.BGStatus = 1;
                            List<int> requiredBGCList = GetRequiredBGCList(dbContext, model[i].ProjectID, model[i].EmpID);
                            bool isInsertPersionBG = UpdateRequiredBGParams(requiredBGCList, model[i].ProjectID, model[i].EmpID);
                            if (isInsertPersionBG)
                            {
                                emailService.SendBGVerificationToHR(model[i], requiredBGCList);
                            }
                        }
                        else
                        {
                            objPMSResourceAllocation.BGStatus = 0;
                        }
                        objPMSResourceAllocation.BGStatus = 1;
                        dbContext.PMSResourceAllocation.Add(objPMSResourceAllocation);
                        isSuccess = UpdateRequestAction(model[i].RequestID);
                        if (isSuccess)
                        {
                            emailService.EmployeeUpdateEmail(model[i]);
                        }
                    }
                    dbContext.SaveChanges();
                    isSuccess = true;
                }

                catch
                {
                    isSuccess = false;
                }
                return isSuccess;
            }
        }

        private List<int> GetRequiredBGCList(PhoenixEntities dbContext, int projectID, int personID)
        {
            List<int> missMatchedBGParameters = new List<int>();


            List<int> projectBGParameters = (from m in dbContext.CustomerBGMapping
                                             where m.CustomerID == (from c in dbContext.ProjectList where c.ID == projectID select c.CustomerID).FirstOrDefault()
                                             select m.BGParameterID).ToList();

            List<int> resourceBGParameters = (from m in dbContext.PersonBGMapping
                                              where m.PersonID == personID && m.BGStatus == 2
                                              select m.BGParameterID).ToList();

            return missMatchedBGParameters = projectBGParameters.Where(p => !resourceBGParameters.Any(p2 => p2 == p)).ToList();

        }

        private bool UpdateRequiredBGParams(List<int> requiredBGCList, int ProjectID, int personID)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                bool isInsert = false;
                try
                {
                    foreach (var item in requiredBGCList)
                    {
                        PersonBGMapping pbg = (from x in dbContext.PersonBGMapping
                                               where x.BGParameterID == item && x.PersonID == personID
                                               select x).FirstOrDefault();
                        pbg.BGStatus = 1; // 1 is inProgress
                        dbContext.Entry(pbg).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                    dbContext.SaveChanges();
                    isInsert = true;
                }
                catch (Exception ex)
                {
                    isInsert = false;
                }
                return isInsert;

            }

        }
        public bool RAApproveDirectUpdateRequest(List<RAResource> model)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int requestType = 2;
                bool isSuccsess = false;
                try
                {
                    for (int i = 0; i < model.Count; i++)
                    {
                        int projectID = model[i].ProjectID;
                        int? subProjecID = model[i].SubProjectID;
                        int empID = model[i].EmpID;
                        var ca = new PMSResourceAllocation();
                        if (subProjecID > 0)
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                  select x).FirstOrDefault();
                        }
                        else
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                  select x).FirstOrDefault();
                        }

                        isSuccsess = RAUpdateHistoryData(ca, dbContext, requestType);
                        //if (!ca.percentage.Equals(model.Resource[i].Allocation) && model.Resource[i].Allocation > 0)
                        //{
                        //    ca.percentage = model.Resource[i].Allocation;
                        //}
                        //if (!ca.FromDate.Equals(model.Resource[i].StartDate) && model.Resource[i].StartDate != DateTime.MinValue)
                        //{
                        //    ca.FromDate = model.Resource[i].StartDate;
                        //}
                        //if (!ca.ToDate.Equals(model.Resource[i].EndDate) && model.Resource[i].EndDate != DateTime.MinValue)
                        //{
                        //    ca.ToDate = model.Resource[i].EndDate;
                        //}
                        //if (!ca.BillbleType.Equals(model.Resource[i].Billability) && model.Resource[i].Billability > 0)
                        //{
                        //    ca.BillbleType = model.Resource[i].Billability;
                        //}
                        //if (!ca.ProjectRole.Equals(model.Resource[i].ProjectRole) && model.Resource[i].ProjectRole > 0)
                        //{
                        //    ca.ProjectRole = model.Resource[i].ProjectRole;
                        //}
                        //if (ca.ReportingTo.Equals(model.Resource[i].ProjectReporting) && model.Resource[i].ProjectReporting > 0)
                        //{
                        //    ca.ReportingTo = model.Resource[i].ProjectReporting;
                        //}
                        if (isSuccsess)
                        {
                            ca.percentage = model[i].Allocation;
                            ca.FromDate = model[i].StartDate;
                            ca.ToDate = model[i].EndDate;
                            ca.BillbleType = model[i].Billability;
                            ca.ProjectRole = model[i].ProjectRole;
                            ca.ReportingTo = model[i].ProjectReporting;
                            ca.ModifyBy = model[i].StatusBy;
                            ca.ReleaseDate = null;
                            ca.ModifyDate = DateTime.Now;
                            dbContext.Entry(ca).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            isSuccsess = true;
                        }
                        else
                        {
                            isSuccsess = false;
                        }
                        isSuccsess = UpdateRequestAction(model[i].RequestID);
                        if (isSuccsess)
                        {
                            emailService.EmployeeUpdateEmail(model[i]);
                        }
                    }
                }
                catch
                {
                    isSuccsess = false;
                }
                return isSuccsess;
            }
        }
        public bool RAApproveDirectExtentionRequest(List<RAResource> model)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int requestType = 3;
                bool isSuccsess = false;
                try
                {
                    for (int i = 0; i < model.Count; i++)
                    {
                        int projectID = model[i].ProjectID;
                        int empID = model[i].EmpID;

                        PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
                                                    where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                                    select x).FirstOrDefault();

                        isSuccsess = RAUpdateHistoryData(ca, dbContext, requestType);

                        if (isSuccsess)
                        {
                            ca.percentage = model[i].Allocation;
                            ca.FromDate = model[i].StartDate;
                            ca.ToDate = model[i].EndDate;
                            ca.BillbleType = model[i].Billability;
                            ca.ProjectRole = model[i].ProjectRole;
                            ca.ReportingTo = model[i].ProjectReporting;
                            ca.ModifyBy = model[i].StatusBy;
                            ca.ModifyDate = DateTime.Now;
                            dbContext.Entry(ca).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            isSuccsess = true;
                        }
                        else
                        {
                            isSuccsess = false;
                        }
                        dbContext.Entry(ca).State = EntityState.Modified;
                        isSuccsess = UpdateRequestAction(model[i].RequestID);
                        if (isSuccsess)
                        {
                            emailService.EmployeeUpdateEmail(model[i]);
                        }
                    }
                    dbContext.SaveChanges();
                    isSuccsess = true;
                }
                catch
                {
                    isSuccsess = false;
                }
                return isSuccsess;
            }
        }
        public bool RAApproveDirectReleaseRequest(List<RAResource> model)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int requestType = 4;
                bool isSuccsess = false;
                try
                {
                    for (int i = 0; i < model.Count; i++)
                    {
                        int projectID = model[i].ProjectID;
                        int? subProjectID = model[i].SubProjectID;

                        var ca = new PMSResourceAllocation();
                        int empID = model[i].EmpID;
                        if (subProjectID > 0)
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                  select x).FirstOrDefault();
                        }
                        else
                        {
                            ca = (from x in dbContext.PMSResourceAllocation
                                  where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                  select x).FirstOrDefault();
                        }

                        isSuccsess = RAUpdateHistoryData(ca, dbContext, requestType);

                        if (isSuccsess)
                        {
                            dbContext.Entry(ca).State = EntityState.Deleted;
                            isSuccsess = true;
                            isSuccsess = UpdateRequestAction(model[i].RequestID);
                            if (isSuccsess)
                            {
                                emailService.EmployeeReleaseEmail(model[i]);
                            }
                        }
                        else
                        {
                            isSuccsess = false;
                        }
                    }
                    dbContext.SaveChanges();
                }
                catch
                {
                    isSuccsess = false;
                }
                return isSuccsess;
            }

        }
        #endregion

        #region RA Approv request
        public Task<List<ResourceAllocationResponse>> RAApproveFullRaisedRequest(RAGetRaisedRequest model, int userId)
        {
            return Task.Run(() =>
            {
                List<ResourceAllocationResponse> lstResponse = new List<ResourceAllocationResponse>();
                bool isTaskCreated = false;
                bool isToDateValid = false;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {
                        string comments = string.Empty;
                        switch (model.RequestType)
                        {
                            case 1:
                                isToDateValid = checkExtendToDateValid(dbContext, model.ProjectID, model.RANewRequest[0].EndDate);
                                if (isToDateValid)
                                {
                                    List<ResourceAllocationResponse> lstnewResponse = new List<ResourceAllocationResponse>();
                                    lstnewResponse = RAPercentageAvailability(model, dbContext);
                                    if (lstnewResponse.Any(x => x.IsSuccess == false))
                                    {
                                        return lstnewResponse;
                                    }
                                    else
                                    {
                                        isTaskCreated = RAApproveNewRequest(dbContext, model);
                                        ResourceAllocationResponse objUpdateRequestResponse = new ResourceAllocationResponse();
                                        objUpdateRequestResponse.RequestType = 1;
                                        if (isTaskCreated)
                                        {
                                            if (model.RANewRequest.FirstOrDefault().RMGComments == null || model.RANewRequest.FirstOrDefault().RMGComments == "")
                                            {
                                                comments = "Approved By RMG";
                                            }
                                            else
                                            {
                                                comments = model.RANewRequest.FirstOrDefault().RMGComments;
                                            }
                                            model.Status = 1;
                                            emailService.SendResourceAllocationActionEmail(model, userId, comments);
                                            //emailService.ResourceAllocationAllocationUpdateEmail(model, userId, comments);
                                            objUpdateRequestResponse.Response = "New Request Approved Successfully";
                                            objUpdateRequestResponse.IsSuccess = true;
                                            lstResponse.Add(objUpdateRequestResponse);
                                        }
                                    }
                                }
                                else
                                {
                                    //List<ResourceAllocationResponse> lstnewResponse = new List<ResourceAllocationResponse>();
                                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                                    objResourceAllocationResponse.IsSuccess = false;
                                    objResourceAllocationResponse.Response = "Resource end date should not exceed project or customer end date";
                                    lstResponse.Add(objResourceAllocationResponse);
                                }
                                break;
                            case 2:
                                isToDateValid = checkExtendToDateValid(dbContext, model.ProjectID, model.RAUpdateRequest[0].EndDate ?? DateTime.MinValue);
                                if (isToDateValid)
                                {
                                    List<ResourceAllocationResponse> lstUpdateResponse = new List<ResourceAllocationResponse>();
                                    lstUpdateResponse = RAPercentageAvailability(model, dbContext);
                                    if (lstUpdateResponse.Any(x => x.IsSuccess == false))
                                    {
                                        return lstUpdateResponse;
                                    }
                                    else
                                    {
                                        isTaskCreated = RAApproveUpdateRequest(dbContext, model);
                                        ResourceAllocationResponse objUpdateRequestResponse = new ResourceAllocationResponse();
                                        objUpdateRequestResponse.RequestType = 2;
                                        if (isTaskCreated)
                                        {
                                            if (model.RAUpdateRequest.FirstOrDefault().RMGComments == null || model.RAUpdateRequest.FirstOrDefault().RMGComments == "")
                                            {
                                                comments = "Approved By RMG";
                                            }
                                            else
                                            {
                                                comments = model.RAUpdateRequest.FirstOrDefault().RMGComments;
                                            }
                                            model.Status = 1;
                                            emailService.SendResourceAllocationActionEmail(model, userId, comments);
                                            //emailService.ResourceAllocationAllocationUpdateEmail(model, userId, comments);

                                            objUpdateRequestResponse.Response = "Update Request Approved Successfully";
                                            objUpdateRequestResponse.IsSuccess = true;
                                            lstResponse.Add(objUpdateRequestResponse);
                                        }
                                    }
                                }
                                else
                                {
                                    //List<ResourceAllocationResponse> lstUpdateResponse = new List<ResourceAllocationResponse>();
                                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                                    objResourceAllocationResponse.IsSuccess = false;
                                    objResourceAllocationResponse.Response = "Resource end date should not exceed project or customer end date";
                                    lstResponse.Add(objResourceAllocationResponse);
                                }
                                break;
                            case 3:
                                isToDateValid = checkExtendToDateValid(dbContext, model.ProjectID, model.RAExtentionRequest[0].EndDate);
                                if (isToDateValid)
                                {
                                    List<ResourceAllocationResponse> lstextenResponse = new List<ResourceAllocationResponse>();
                                    lstextenResponse = RAPercentageAvailability(model, dbContext);
                                    if (lstextenResponse.Any(x => x.IsSuccess == false))
                                    {
                                        return lstextenResponse;
                                    }
                                    else
                                    {
                                        isTaskCreated = RAApproveExtentionRequest(dbContext, model);
                                        ResourceAllocationResponse objExtentionResponse = new ResourceAllocationResponse();
                                        objExtentionResponse.RequestType = 3;
                                        if (isTaskCreated)
                                        {
                                            comments = model.RAExtentionRequest.FirstOrDefault().RMGComments;
                                            model.Status = 1;
                                            emailService.SendResourceAllocationActionEmail(model, userId, comments);
                                            for (int i = 0; i < model.RAExtentionRequest.Count; i++)
                                            {
                                                RAResource objResource = new RAResource();
                                                objResource.ProjectID = model.ProjectID;
                                                objResource.EmpID = model.RAExtentionRequest[i].EmpID;
                                                objResource.Allocation = model.RAExtentionRequest[i].Allocation;
                                                objResource.Billability = model.RAExtentionRequest[i].Billability;
                                                objResource.ProjectReporting = model.RAExtentionRequest[i].ProjectReporting;
                                                objResource.ProjectRole = model.RAExtentionRequest[i].ProjectRole;
                                                objResource.StartDate = model.RAExtentionRequest[i].StartDate;
                                                objResource.EndDate = model.RAExtentionRequest[i].EndDate;
                                                objResource.RequestedBy = model.RequestedBy;
                                                objResource.ActionDate = model.RAExtentionRequest[i].ActionDate;
                                                objResource.RequestID = model.ID;
                                                objResource.StatusBy = model.StatusBy;
                                                objResource.Comments = model.RAExtentionRequest[0].RMGComments;
                                                emailService.EmployeeUpdateEmail(objResource);
                                            }

                                            //emailService.ResourceAllocationAllocationUpdateEmail(model, userId, comments);

                                            objExtentionResponse.Response = "Extension Request Approved Successfully";
                                            objExtentionResponse.IsSuccess = true;
                                        }
                                        else
                                        {
                                            objExtentionResponse.Response = "Extension Request Is Not Approved";
                                            objExtentionResponse.IsSuccess = false;
                                        }
                                        lstResponse.Add(objExtentionResponse);
                                    }
                                }
                                else
                                {
                                    //List<ResourceAllocationResponse> lstextenResponse = new List<ResourceAllocationResponse>();
                                    ResourceAllocationResponse objResourceAllocationResponse = new ResourceAllocationResponse();
                                    objResourceAllocationResponse.IsSuccess = false;
                                    objResourceAllocationResponse.Response = "Resource end date should not exceed project or customer end date";
                                    lstResponse.Add(objResourceAllocationResponse);
                                }
                                break;
                            case 4:
                                isTaskCreated = RAApproveReleaseRequest(dbContext, model);
                                ResourceAllocationResponse objReleaseResponse = new ResourceAllocationResponse();
                                objReleaseResponse.RequestType = 4;
                                if (isTaskCreated)
                                {
                                    if (model.RAReleaseRequest.FirstOrDefault().RMGComments == null || model.RAReleaseRequest.FirstOrDefault().RMGComments == "")
                                    {
                                        comments = "Approved By RMG";
                                    }
                                    else
                                    {
                                        comments = model.RAReleaseRequest.FirstOrDefault().RMGComments;
                                    }
                                    model.Status = 1;
                                    emailService.SendResourceAllocationActionEmail(model, userId, comments);
                                    //emailService.SendResourceAllocationReleaseEmail(model, userId, comments);

                                    objReleaseResponse.Response = "Release Request Approved Successfully";
                                    objReleaseResponse.IsSuccess = true;
                                }
                                else
                                {
                                    objReleaseResponse.Response = "Release Request Is Not Approved";
                                    objReleaseResponse.IsSuccess = false;
                                }
                                lstResponse.Add(objReleaseResponse);
                                break;
                            default:
                                ResourceAllocationResponse objInvalidResponse = new ResourceAllocationResponse();
                                objInvalidResponse.RequestType = -1;
                                objInvalidResponse.Response = "Invalid Request Type";
                                objInvalidResponse.IsSuccess = false;
                                lstResponse.Add(objInvalidResponse);
                                break;
                        }
                        //dbContext.SaveChanges();
                        isTaskCreated = true;
                    }
                    catch
                    {
                        isTaskCreated = false;
                        ResourceAllocationResponse objInvalidResponse = new ResourceAllocationResponse();
                        objInvalidResponse.RequestType = -1;
                        objInvalidResponse.Response = "Error Occured";
                        objInvalidResponse.IsSuccess = false;
                        lstResponse.Add(objInvalidResponse);
                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return lstResponse;
            });
        }
        private bool RAApproveNewRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            bool isEditSuccessfully = RAEditNewRequest(dbContext, model);

            //for (int i = 0; i < model.RANewRequest.Count; i++)
            //{
            //    int personID = model.RANewRequest[i].EmpID;
            //    PMSAllocationNewRequest n = (from x in dbContext.PMSAllocationNewRequest
            //                                 where x.RequestID == model.ID && x.EmpID == personID
            //                                 select x).First();
            //    if (model.RANewRequest[i].ActionDate == DateTime.MinValue)
            //    {
            //        n.ActionDate = model.RANewRequest[i].StartDate;
            //    }
            //    else
            //    {
            //        n.ActionDate = model.RANewRequest[i].ActionDate;
            //    }
            //    n.RMGComments = model.RANewRequest[i].RMGComments;
            //    n.Status = 1;
            //    n.ModifyBy = model.StatusBy;
            //    n.ModifyDate = DateTime.Now;
            //    dbContext.Entry(n).State = EntityState.Modified;
            //}


            //for (int i = 0; i < model.RANewRequest.Count; i++)
            //{
            //    PMSResourceAllocation objPMSResourceAllocation = new PMSResourceAllocation();
            //    objPMSResourceAllocation.ProjectID = model.ProjectID;
            //    objPMSResourceAllocation.PersonID = model.RANewRequest[i].EmpID;
            //    objPMSResourceAllocation.ReportingTo = model.RANewRequest[i].ProjectReporting;
            //    objPMSResourceAllocation.ProjectRole = model.RANewRequest[i].ProjectRole;
            //    objPMSResourceAllocation.ToDate = model.RANewRequest[i].EndDate;
            //    objPMSResourceAllocation.FromDate = model.RANewRequest[i].StartDate;
            //    objPMSResourceAllocation.percentage = model.RANewRequest[i].Allocation;
            //    objPMSResourceAllocation.BillbleType = model.RANewRequest[i].Billability;
            //    objPMSResourceAllocation.CreatedDate = DateTime.Now;
            //    objPMSResourceAllocation.ModifyDate = DateTime.Now;
            //    objPMSResourceAllocation.CreatedBy = model.StatusBy;
            //    objPMSResourceAllocation.ModifyBy = model.StatusBy;
            //    objPMSResourceAllocation.IsDeleted = false;
            //    dbContext.PMSResourceAllocation.Add(objPMSResourceAllocation);
            //}
            PMSAllocationRequest par = (from x in dbContext.PMSAllocationRequest
                                        where x.RequestID == model.ID
                                        select x).First();
            par.Status = 1; // 1 is Approve
            par.StatusBy = model.StatusBy;
            par.StatusDate = DateTime.Now;
            dbContext.Entry(par).State = EntityState.Modified;
            dbContext.SaveChanges();
            isEdit = true;
            return isEdit;
        }
        private bool RAApproveUpdateRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            for (int i = 0; i < model.RAUpdateRequest.Count; i++)
            {
                int empID = model.RAUpdateRequest[i].EmpID;

                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
                                            where x.ProjectID == model.ProjectID && x.PersonID == empID && x.IsDeleted == false
                                            select x).FirstOrDefault();

                //if (model.RAUpdateRequest[i].ActionDate == DateTime.MinValue || model.RAUpdateRequest[i].ActionDate == null)
                //{
                //    ca.ReleaseDate = model.RAUpdateRequest[i].EndDate;
                //}
                //else
                //{
                ca.ReleaseDate = model.RAUpdateRequest[i].ActionDate;
                // }
                ca.ModifyBy = model.StatusBy;
                ca.ModifyDate = DateTime.Now;
                dbContext.Entry(ca).State = EntityState.Modified;

                PMSAllocationUpdateRequest u = (from x in dbContext.PMSAllocationUpdateRequest
                                                where x.RequestID == model.ID && x.EmpID == empID && x.IsDeleted == false
                                                select x).FirstOrDefault();

                u.Percentage = model.RAUpdateRequest[i].Allocation;
                //u.FromDate = model.RAUpdateRequest[i].StartDate;
                u.ToDate = model.RAUpdateRequest[i].EndDate;
                u.BillableType = model.RAUpdateRequest[i].Billability;
                u.ProjectRole = model.RAUpdateRequest[i].ProjectRole;
                u.ReportingTo = model.RAUpdateRequest[i].ProjectReporting;
                u.Status = 1; //1 is Approve
                if (model.RAUpdateRequest[i].RMGComments == null || model.RAUpdateRequest[i].RMGComments == "")
                {
                    u.RMGComments = "Approved By RMG";
                }
                else
                {
                    u.RMGComments = model.RAUpdateRequest[i].RMGComments;
                }

                //if (model.RAUpdateRequest[i].ActionDate == DateTime.MinValue || model.RAUpdateRequest[i].ActionDate == null)
                //{
                //    u.ActionDate = DateTime.Now.AddDays(1);
                //        //model.RAUpdateRequest[i].EndDate.Value.AddDays(1);
                //    u.FromDate = model.RAUpdateRequest[i].EndDate.Value.AddDays(1);
                //}
                //else
                //{
                u.ActionDate = model.RAUpdateRequest[i].ActionDate.Value.AddDays(1);
                u.FromDate = model.RAUpdateRequest[i].ActionDate.Value.AddDays(1);
                // }
                u.ModifyBy = model.StatusBy;
                u.ModifyDate = DateTime.Now;
                dbContext.Entry(u).State = EntityState.Modified;
            }
            PMSAllocationRequest par = (from x in dbContext.PMSAllocationRequest
                                        where x.RequestID == model.ID
                                        select x).First();
            par.Status = 1; // 1 is Approve
            par.StatusBy = model.StatusBy;
            par.StatusDate = DateTime.Now;
            dbContext.Entry(par).State = EntityState.Modified;
            dbContext.SaveChanges();
            isEdit = true;
            return isEdit;
        }
        private bool RAApproveExtentionRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;

            IEnumerable<int> dbEmpID = (from x in dbContext.PMSAllocationExtentionRequest
                                        where x.RequestID == model.ID && x.IsDeleted == false
                                        select x.EmpID);

            IEnumerable<int> modelEmpID = (from x in model.RAExtentionRequest
                                           where x.IsDeleted == false
                                           select x.EmpID);

            IEnumerable<int> rejectedEmpID = dbEmpID.Except(modelEmpID);

            var totalRejected = dbContext.PMSAllocationExtentionRequest.Where(f => rejectedEmpID.Contains(f.EmpID)).ToList();
            totalRejected.ForEach(a =>
            {
                dbContext.PMSAllocationExtentionRequest.Remove(a);
                //a.Status = 2;
                //a.RMGComments = "Extension request has been deleted";
                //a.ModifyBy = model.StatusBy;
                //a.ModifyDate = DateTime.Now;
                //a.IsDeleted = true;
            });
            //totalRejected.ForEach(a => a.Status = 2);

            for (int i = 0; i < model.RAExtentionRequest.Count; i++)
            {
                int empID = model.RAExtentionRequest[i].EmpID;
                int projectID = model.ProjectID;

                int rrID = (from r in dbContext.PMSAllocationRequest
                            join rr in dbContext.PMSAllocationReleaseRequest on r.RequestID equals rr.RequestID
                            where rr.EmpID == empID && r.ProjectID == projectID && r.IsActionPerformed != true
                            select rr.ID).FirstOrDefault();
                if (rrID > 0)
                {
                    PMSAllocationReleaseRequest rrq = (from x in dbContext.PMSAllocationReleaseRequest
                                                       where x.ID == rrID
                                                       select x).FirstOrDefault();
                    rrq.Status = 2; // 2 is Reject
                    rrq.RMGComments = "Due to resource extention release request have been rejected";
                    rrq.ModifyBy = model.StatusBy;
                    rrq.ModifyDate = DateTime.Now;
                    rrq.ActionDate = DateTime.Now;
                    dbContext.Entry(rrq).State = EntityState.Modified;

                    PMSAllocationRequest r = (from x in dbContext.PMSAllocationRequest
                                              where x.RequestID == rrq.RequestID
                                              select x).FirstOrDefault();
                    r.Status = 2; // 2 is Reject
                    r.StatusBy = model.StatusBy;
                    r.StatusDate = DateTime.Now;
                    r.IsActionPerformed = true;
                    dbContext.Entry(r).State = EntityState.Modified;
                }

                PMSAllocationExtentionRequest e = (from x in dbContext.PMSAllocationExtentionRequest
                                                   where x.RequestID == model.ID && x.EmpID == empID && x.IsDeleted == false
                                                   select x).FirstOrDefault();

                e.ToDate = model.RAExtentionRequest[i].EndDate;
                e.Status = 1;
                e.RMGComments = model.RAExtentionRequest[0].RMGComments;
                e.ModifyBy = model.StatusBy;
                e.ModifyDate = DateTime.Now;
                e.ActionDate = DateTime.Now;
                dbContext.Entry(e).State = EntityState.Modified;

                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
                                            where x.ProjectID == projectID && x.PersonID == empID && x.IsDeleted == false
                                            select x).FirstOrDefault();

                ca.ToDate = model.RAExtentionRequest[i].EndDate;
                ca.ReleaseDate = null;
                ca.ModifyBy = model.StatusBy;
                ca.ModifyDate = DateTime.Now;
                dbContext.Entry(ca).State = EntityState.Modified;

            }

            PMSAllocationRequest par = (from x in dbContext.PMSAllocationRequest
                                        where x.RequestID == model.ID
                                        select x).FirstOrDefault();
            par.Status = 1; // 1 is Approve
            par.StatusBy = model.StatusBy;
            par.StatusDate = DateTime.Now;
            par.IsActionPerformed = true;
            dbContext.Entry(par).State = EntityState.Modified;
            dbContext.SaveChanges();
            isEdit = true;
            return isEdit;
        }
        private bool RAApproveReleaseRequest(PhoenixEntities dbContext, RAGetRaisedRequest model)
        {
            bool isEdit = false;
            for (int i = 0; i < model.RAReleaseRequest.Count; i++)
            {
                int empID = model.RAReleaseRequest[i].EmpID;
                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
                                            where x.ProjectID == model.ProjectID && x.PersonID == empID && x.IsDeleted == false
                                            select x).FirstOrDefault();

                if (model.RAReleaseRequest[i].ActionDate == DateTime.MinValue)
                {
                    ca.ReleaseDate = model.RAReleaseRequest[i].EndDate;
                }
                else
                {
                    ca.ReleaseDate = model.RAReleaseRequest[i].ActionDate;
                }
                ca.ModifyBy = model.StatusBy;
                ca.ModifyDate = DateTime.Now;
                dbContext.Entry(ca).State = EntityState.Modified;

                PMSAllocationReleaseRequest rr = (from x in dbContext.PMSAllocationReleaseRequest
                                                  where x.RequestID == model.ID && x.EmpID == empID && x.IsDeleted == false
                                                  select x).FirstOrDefault();

                rr.Status = 1; // 1 is Approve
                if (model.RAReleaseRequest[i].RMGComments == null || model.RAReleaseRequest[i].RMGComments == "")
                {
                    rr.RMGComments = "Approved By RMG";
                }
                else
                {
                    rr.RMGComments = model.RAReleaseRequest[i].RMGComments;
                }
                rr.ModifyBy = model.StatusBy;
                rr.ModifyDate = DateTime.Now;
                if (model.RAReleaseRequest[i].ActionDate == DateTime.MinValue)
                {
                    rr.ActionDate = model.RAReleaseRequest[i].EndDate.AddDays(1);
                }
                else
                {
                    rr.ActionDate = model.RAReleaseRequest[i].ActionDate.Value.AddDays(1);
                }
                dbContext.Entry(rr).State = EntityState.Modified;
            }
            PMSAllocationRequest par = (from x in dbContext.PMSAllocationRequest
                                        where x.RequestID == model.ID
                                        select x).First();
            par.Status = 1; // 1 is Approve
            par.StatusBy = model.StatusBy;
            par.StatusDate = DateTime.Now;
            dbContext.Entry(par).State = EntityState.Modified;
            dbContext.SaveChanges();
            isEdit = true;
            return isEdit;
        }
        #endregion

        #region RA Rejecct request
        public Task<bool> RARejectFullRaisedRequest(RARejectRequest rARejectRequest)
        {
            return Task.Run(() =>
            {
                bool isTaskCreated = false;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    int requestId = rARejectRequest.RequestID;
                    int requestType = rARejectRequest.RequestType;
                    int userID = rARejectRequest.UserID;
                    string comment = rARejectRequest.Comment;
                    try
                    {
                        //Update First record
                        switch (requestType)
                        {
                            case 1:
                                var n = from x in dbContext.PMSAllocationNewRequest
                                        where x.RequestID == requestId && x.IsDeleted == false
                                        select x;

                                foreach (PMSAllocationNewRequest newDetails in n.ToList())
                                {
                                    newDetails.Status = 2;// 2 is Reject
                                    newDetails.RMGComments = comment;
                                    newDetails.ModifyBy = userID;
                                    newDetails.ModifyDate = DateTime.Now;
                                    dbContext.Entry(newDetails).State = EntityState.Modified;
                                }
                                break;
                            case 2:
                                var u = from x in dbContext.PMSAllocationUpdateRequest
                                        where x.RequestID == requestId && x.IsDeleted == false
                                        select x;

                                foreach (PMSAllocationUpdateRequest updateDetails in u.ToList())
                                {
                                    updateDetails.Status = 2;// 2 is Reject
                                    updateDetails.RMGComments = comment;
                                    updateDetails.ModifyBy = userID;
                                    updateDetails.ModifyDate = DateTime.Now;
                                    dbContext.Entry(updateDetails).State = EntityState.Modified;
                                }
                                break;
                            case 3:
                                var e = from x in dbContext.PMSAllocationExtentionRequest
                                        where x.RequestID == requestId && x.IsDeleted == false
                                        select x;

                                foreach (PMSAllocationExtentionRequest extentionDetails in e.ToList())
                                {
                                    extentionDetails.Status = 2;// 2 is Reject
                                    extentionDetails.RMGComments = comment;
                                    extentionDetails.ModifyBy = userID;
                                    extentionDetails.ModifyDate = DateTime.Now;
                                    dbContext.Entry(extentionDetails).State = EntityState.Modified;
                                }
                                break;
                            case 4:
                                var rr = from x in dbContext.PMSAllocationReleaseRequest
                                         where x.RequestID == requestId && x.IsDeleted == false
                                         select x;
                                foreach (PMSAllocationReleaseRequest releaseRequest in rr.ToList())
                                {
                                    releaseRequest.Status = 2;// 2 is Reject
                                    releaseRequest.RMGComments = comment;
                                    releaseRequest.ModifyBy = userID;
                                    releaseRequest.ModifyDate = DateTime.Now;
                                    dbContext.Entry(releaseRequest).State = EntityState.Modified;
                                }
                                break;
                            default:
                                Console.WriteLine("Invalid Request Type");
                                break;
                        }

                        //Update another record          
                        PMSAllocationRequest par = (from x in dbContext.PMSAllocationRequest
                                                    where x.RequestID == requestId
                                                    select x).First();
                        par.Status = 2; // 2 is Reject
                        par.StatusBy = userID;
                        par.StatusDate = DateTime.Now;
                        dbContext.Entry(par).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        // Send email
                        this.emailService.SendResourceAllocationActionStatusEmail(userID, par.RequestedBy, par.RequestID, par.RequestType, par.Status, comment);

                        isTaskCreated = true;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return isTaskCreated;
            });
        }
        #endregion

        #region RA Get MyAllocation
        public async Task<RAMyAllocation> RAGetMyAllocation(int userID, int projectID)
        {
            RAMyAllocation raGetMyAllocationViewModel = new RAMyAllocation();
            try
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                if (userID != 0)
                {

                    raGetMyAllocationViewModel.CurrentAllocation = (from pra in dbContext.PMSResourceAllocation
                                                                    where pra.PersonID == userID
                                                                    orderby pra.ID descending
                                                                    select new RACurrentAllocation
                                                                    {
                                                                        ID = pra.ID,
                                                                        PersonID = pra.PersonID,
                                                                        FullName = dbContext.People.Where(x => x.ID == pra.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        ProjectID = pra.ProjectID,
                                                                        ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                                        Percentage = pra.percentage,
                                                                        FromDate = pra.FromDate,
                                                                        ToDate = pra.ToDate,
                                                                        ProjectRole = pra.ProjectRole,
                                                                        RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == pra.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                                                        IsDeleted = pra.IsDeleted,
                                                                        ReportingTo = pra.ReportingTo,
                                                                        ReportingToName = dbContext.People.Where(x => x.ID == pra.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        BillbleType = pra.BillbleType,
                                                                        BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == pra.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                                        ReleaseDate = pra.ReleaseDate,
                                                                        EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == pra.PersonID).Select(x => new RAEmploymentDetails()
                                                                        {
                                                                            DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                            DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(), //x.DeliveryUnit,
                                                                            ResourcePool = x.ResourcePool,
                                                                            WorkLocation = x.WorkLocation
                                                                        }).FirstOrDefault(),
                                                                    }).ToList();
                    raGetMyAllocationViewModel.ProjectedAllocation = RAGetProjectedAllocation(userID);
                    raGetMyAllocationViewModel.HistoryAllocation = (from prah in dbContext.PMSResourceAllocationHistory
                                                                    where prah.PersonID == userID
                                                                    orderby prah.ID descending
                                                                    select new RAHistoryAllocation
                                                                    {
                                                                        ID = prah.ID,
                                                                        ResourceAllocationID = prah.ResourceAllocationID,
                                                                        PersonID = prah.PersonID,
                                                                        FullName = dbContext.People.Where(x => x.ID == prah.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        ProjectID = prah.ProjectID,
                                                                        ProjectName = dbContext.ProjectList.Where(x => x.ID == prah.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                                        Percentage = prah.percentage,
                                                                        FromDate = prah.FromDate,
                                                                        ToDate = prah.ToDate,
                                                                        ProjectRole = prah.ProjectRole,
                                                                        RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == prah.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                                                        IsDeleted = prah.IsDeleted,
                                                                        ReportingTo = prah.ReportingTo,
                                                                        ReportingToName = dbContext.People.Where(x => x.ID == prah.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        BillbleType = prah.BillbleType,
                                                                        BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == prah.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                                        ReleaseDate = prah.ReleaseDate,
                                                                        RequestType = prah.RequestType,
                                                                        EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == prah.PersonID).Select(x => new RAEmploymentDetails()
                                                                        {
                                                                            DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == prah.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                            DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == prah.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                                                            ResourcePool = x.ResourcePool,
                                                                            WorkLocation = x.WorkLocation
                                                                        }).FirstOrDefault(),
                                                                        ModifyBy = prah.ModifyBy,
                                                                        Ratings = (from ar in dbContext.PMSAllocationRatings
                                                                                   where ar.AllocationID == prah.ResourceAllocationID
                                                                                   select new RARatings
                                                                                   {
                                                                                       Communication = ar.Communication,
                                                                                       Discipline = ar.Discipline,
                                                                                       Process = ar.Process,
                                                                                       Quality = ar.Quality,
                                                                                       Technical = ar.Technical,
                                                                                       Timelines = ar.Timelines,
                                                                                       ProjectSkillRatings = (from sr in dbContext.PMSAllocationSkillRatings
                                                                                                              where sr.AllocationID == prah.ResourceAllocationID
                                                                                                              select new RAProjectSkillRatings
                                                                                                              {
                                                                                                                  SkillId = sr.ProjectSkillID,
                                                                                                                  Rating = sr.Rating,
                                                                                                              }).ToList()
                                                                                   }).ToList(),
                                                                    }).ToList();
                }
                else
                {

                    raGetMyAllocationViewModel.CurrentAllocation = (from pra in dbContext.PMSResourceAllocation
                                                                    where pra.ProjectID == projectID
                                                                    orderby pra.ID descending
                                                                    select new RACurrentAllocation
                                                                    {
                                                                        ID = pra.ID,
                                                                        PersonID = pra.PersonID,
                                                                        FullName = dbContext.People.Where(x => x.ID == pra.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        ProjectID = pra.ProjectID,
                                                                        ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                                        Percentage = pra.percentage,
                                                                        FromDate = pra.FromDate,
                                                                        ToDate = pra.ToDate,
                                                                        ProjectRole = pra.ProjectRole,
                                                                        RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == pra.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                                                        IsDeleted = pra.IsDeleted,
                                                                        ReportingTo = pra.ReportingTo,
                                                                        ReportingToName = dbContext.People.Where(x => x.ID == pra.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        BillbleType = pra.BillbleType,
                                                                        BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == pra.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                                        ReleaseDate = pra.ReleaseDate,
                                                                        EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == pra.PersonID).Select(x => new RAEmploymentDetails()
                                                                        {
                                                                            DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                            DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                                                            ResourcePool = x.ResourcePool,
                                                                            WorkLocation = x.WorkLocation
                                                                        }).FirstOrDefault(),
                                                                    }).ToList();
                    raGetMyAllocationViewModel.ProjectedAllocation = RAGetProjectedAllocation(userID);
                    raGetMyAllocationViewModel.HistoryAllocation = (from prah in dbContext.PMSResourceAllocationHistory
                                                                    where prah.ProjectID == projectID
                                                                    orderby prah.ID descending
                                                                    select new RAHistoryAllocation
                                                                    {
                                                                        ID = prah.ID,
                                                                        ResourceAllocationID = prah.ResourceAllocationID,
                                                                        PersonID = prah.PersonID,
                                                                        FullName = dbContext.People.Where(x => x.ID == prah.PersonID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        ProjectID = prah.ProjectID,
                                                                        ProjectName = dbContext.ProjectList.Where(x => x.ID == prah.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                                                        Percentage = prah.percentage,
                                                                        FromDate = prah.FromDate,
                                                                        ToDate = prah.ToDate,
                                                                        ProjectRole = prah.ProjectRole,
                                                                        RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == prah.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                                                        IsDeleted = prah.IsDeleted,
                                                                        ReportingTo = prah.ReportingTo,
                                                                        ReportingToName = dbContext.People.Where(x => x.ID == prah.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                        BillbleType = prah.BillbleType,
                                                                        BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == prah.BillbleType).Select(x => x.Discription).FirstOrDefault(),
                                                                        ReleaseDate = prah.ReleaseDate,
                                                                        RequestType = prah.RequestType,
                                                                        EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == prah.PersonID).Select(x => new RAEmploymentDetails()
                                                                        {
                                                                            DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == prah.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                                                            DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == prah.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                                                            ResourcePool = x.ResourcePool,
                                                                            WorkLocation = x.WorkLocation
                                                                        }).FirstOrDefault(),
                                                                        ModifyBy = prah.ModifyBy,
                                                                        Ratings = (from ar in dbContext.PMSAllocationRatings
                                                                                   where ar.AllocationID == prah.ResourceAllocationID
                                                                                   select new RARatings
                                                                                   {
                                                                                       Communication = ar.Communication,
                                                                                       Discipline = ar.Discipline,
                                                                                       Process = ar.Process,
                                                                                       Quality = ar.Quality,
                                                                                       Technical = ar.Technical,
                                                                                       Timelines = ar.Timelines,
                                                                                       ProjectSkillRatings = (from sr in dbContext.PMSAllocationSkillRatings
                                                                                                              where sr.AllocationID == prah.ResourceAllocationID
                                                                                                              select new RAProjectSkillRatings
                                                                                                              {
                                                                                                                  SkillId = sr.ProjectSkillID,
                                                                                                                  Rating = sr.Rating,
                                                                                                              }).ToList()
                                                                                   }).ToList(),
                                                                    }).ToList();
                }
                return await Task.Run(() => { return raGetMyAllocationViewModel; });
            }
            catch
            {
                return raGetMyAllocationViewModel;
            }
        }
        #endregion

        #region RA Get Projected Allocation
        private IEnumerable<RAProjectedAllocation> RAGetProjectedAllocation(int userID)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                IEnumerable<RAProjectedAllocation> lstRAProjected;
                List<RAProjectedAllocation> lstRANew = new List<RAProjectedAllocation>();
                List<RAProjectedAllocation> lstRAUpdate = new List<RAProjectedAllocation>();
                List<RAProjectedAllocation> lstRAExtension = new List<RAProjectedAllocation>();
                List<RAProjectedAllocation> lstRARelease = new List<RAProjectedAllocation>();
                lstRANew = (from pra in dbContext.PMSAllocationRequest
                            join n in dbContext.PMSAllocationNewRequest on pra.RequestID equals n.RequestID
                            where n.EmpID == userID && n.Status == 1 && pra.IsActionPerformed == false
                            orderby pra.RequestID descending
                            select new RAProjectedAllocation
                            {
                                ID = pra.RequestID,
                                PersonID = n.EmpID,
                                FullName = dbContext.People.Where(x => x.ID == n.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                ProjectID = pra.ProjectID,
                                ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                Percentage = n.Percentage,
                                FromDate = n.FromDate,
                                ToDate = n.ToDate,
                                ProjectRole = n.ProjectRole,
                                RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == n.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                IsDeleted = pra.IsDeleted,
                                ReportingTo = n.ReportingTo,
                                ReportingToName = dbContext.People.Where(x => x.ID == n.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                BillbleType = n.BillableType,
                                BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == n.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                RequestType = 1,
                                EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == n.EmpID).Select(x => new RAEmploymentDetails()
                                {
                                    DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                    DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                    ResourcePool = x.ResourcePool,
                                    WorkLocation = x.WorkLocation
                                }).FirstOrDefault(),
                                IsProjected = true
                            }).ToList();

                lstRAUpdate = (from pra in dbContext.PMSAllocationRequest
                               join u in dbContext.PMSAllocationUpdateRequest on pra.RequestID equals u.RequestID
                               where u.EmpID == userID && u.Status == 1 && pra.IsActionPerformed == false
                               orderby pra.RequestID descending
                               select new RAProjectedAllocation
                               {
                                   ID = pra.RequestID,
                                   PersonID = u.EmpID,
                                   FullName = dbContext.People.Where(x => x.ID == u.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                   ProjectID = pra.ProjectID,
                                   ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                   Percentage = u.Percentage ?? 0,
                                   FromDate = u.FromDate ?? DateTime.MinValue,
                                   ToDate = u.ToDate ?? DateTime.MinValue,
                                   ProjectRole = u.ProjectRole ?? 0,
                                   RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == u.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                   IsDeleted = pra.IsDeleted,
                                   ReportingTo = u.ReportingTo ?? 0,
                                   ReportingToName = dbContext.People.Where(x => x.ID == u.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                   BillbleType = u.BillableType ?? 0,
                                   BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == u.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                   RequestType = 2,
                                   EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == u.EmpID).Select(x => new RAEmploymentDetails()
                                   {
                                       DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                       DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                       ResourcePool = x.ResourcePool,
                                       WorkLocation = x.WorkLocation
                                   }).FirstOrDefault(),
                                   IsProjected = true
                               }).ToList();

                lstRAExtension = (from pra in dbContext.PMSAllocationRequest
                                  join e in dbContext.PMSAllocationExtentionRequest on pra.RequestID equals e.RequestID
                                  where e.EmpID == userID && e.Status == 1 && pra.IsActionPerformed == false
                                  orderby pra.RequestID descending
                                  select new RAProjectedAllocation
                                  {
                                      ID = pra.RequestID,
                                      PersonID = e.EmpID,
                                      FullName = dbContext.People.Where(x => x.ID == e.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      ProjectID = pra.ProjectID,
                                      ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                      Percentage = e.Percentage,
                                      FromDate = e.FromDate,
                                      ToDate = e.ToDate,
                                      ProjectRole = e.ProjectRole,
                                      RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == e.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                      IsDeleted = pra.IsDeleted,
                                      ReportingTo = e.ReportingTo,
                                      ReportingToName = dbContext.People.Where(x => x.ID == e.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      BillbleType = e.BillableType,
                                      BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == e.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                      RequestType = 3,
                                      EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == e.EmpID).Select(x => new RAEmploymentDetails()
                                      {
                                          DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                          DeliveryUnit = dbContext.ProjectList.Where(y => y.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                          ResourcePool = x.ResourcePool,
                                          WorkLocation = x.WorkLocation
                                      }).FirstOrDefault(),
                                      IsProjected = true
                                  }).ToList();

                //lstRARelease = (from pra in dbContext.PMSAllocationRequest
                //                join rr in dbContext.PMSAllocationReleaseRequest on pra.RequestID equals rr.RequestID
                //                where rr.EmpID == userID && rr.Status == 1 && pra.IsActionPerformed == false
                //                orderby pra.RequestID descending
                //                select new RAProjectedAllocation
                //                {
                //                    ID = pra.RequestID,
                //                    PersonID = rr.EmpID,
                //                    FullName = dbContext.People.Where(x => x.ID == rr.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                //                    ProjectID = pra.ProjectID,
                //                    ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                //                    Percentage = rr.Percentage,
                //                    FromDate = rr.FromDate,
                //                    ToDate = rr.ToDate,
                //                    ProjectRole = rr.ProjectRole,
                //                    RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == rr.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                //                    IsDeleted = pra.IsDeleted,
                //                    ReportingTo = rr.ReportingTo,
                //                    ReportingToName = dbContext.People.Where(x => x.ID == rr.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                //                    BillbleType = rr.BillableType,
                //                    BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == rr.BillableType).Select(x => x.Discription).FirstOrDefault(),
                //                    RequestType = 4,
                //                    EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == rr.EmpID).Select(x => new RAEmploymentDetails()
                //                    {
                //                        DeliveryTeam = x.DeliveryTeam,
                //                        DeliveryUnit = x.DeliveryUnit,
                //                        ResourcePool = x.ResourcePool,
                //                        WorkLocation = x.WorkLocation
                //                    }).FirstOrDefault(),
                //                }).ToList();

                lstRAProjected = lstRANew.Concat(lstRAUpdate).Concat(lstRAExtension);

                return lstRAProjected;
            }
        }
        private IEnumerable<CurrentResourceAllocationModel> RAGetProjectedAllocationCurrent(int userID)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                IEnumerable<CurrentResourceAllocationModel> lstRAProjected;
                List<CurrentResourceAllocationModel> lstRANew = new List<CurrentResourceAllocationModel>();
                List<CurrentResourceAllocationModel> lstRAUpdate = new List<CurrentResourceAllocationModel>();
                List<CurrentResourceAllocationModel> lstRAExtension = new List<CurrentResourceAllocationModel>();
                List<CurrentResourceAllocationModel> lstRARelease = new List<CurrentResourceAllocationModel>();
                lstRANew = (from pra in dbContext.PMSAllocationRequest
                            join n in dbContext.PMSAllocationNewRequest on pra.RequestID equals n.RequestID
                            where n.EmpID == userID && n.Status == 1 && pra.IsActionPerformed == false
                            orderby pra.RequestID descending
                            select new CurrentResourceAllocationModel
                            {
                                ID = pra.RequestID,
                                RequestType = 1,
                                PersonID = n.EmpID,
                                FullName = dbContext.People.Where(x => x.ID == n.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                ProjectID = pra.ProjectID,
                                ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                percentage = n.Percentage,
                                FromDate = n.FromDate,
                                ToDate = n.ToDate,
                                ProjectRole = n.ProjectRole,
                                RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == n.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                IsDeleted = pra.IsDeleted,
                                ReportingTo = n.ReportingTo,
                                ReportingToName = dbContext.People.Where(x => x.ID == n.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                BillbleType = n.BillableType,
                                BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == n.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == n.EmpID).Select(x => new RAEmploymentDetails()
                                {
                                    DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                    DeliveryUnit = dbContext.ProjectList.Where(y => x.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                    ResourcePool = x.ResourcePool,
                                    WorkLocation = x.WorkLocation
                                }).FirstOrDefault(),
                                IsProjected = true
                            }).ToList();

                lstRAUpdate = (from pra in dbContext.PMSAllocationRequest
                               join u in dbContext.PMSAllocationUpdateRequest on pra.RequestID equals u.RequestID
                               where u.EmpID == userID && u.Status == 1 && pra.IsActionPerformed == false
                               orderby pra.RequestID descending
                               select new CurrentResourceAllocationModel
                               {
                                   ID = pra.RequestID,
                                   RequestType = 2,
                                   PersonID = u.EmpID,
                                   FullName = dbContext.People.Where(x => x.ID == u.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                   ProjectID = pra.ProjectID,
                                   ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                   percentage = u.Percentage ?? 0,
                                   FromDate = u.FromDate ?? DateTime.MinValue,
                                   ToDate = u.ToDate ?? DateTime.MinValue,
                                   ProjectRole = u.ProjectRole ?? 0,
                                   RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == u.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                   IsDeleted = pra.IsDeleted,
                                   ReportingTo = u.ReportingTo ?? 0,
                                   ReportingToName = dbContext.People.Where(x => x.ID == u.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                   BillbleType = u.BillableType ?? 0,
                                   BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == u.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                   EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == u.EmpID).Select(x => new RAEmploymentDetails()
                                   {
                                       DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                       DeliveryUnit = dbContext.ProjectList.Where(y => x.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                       ResourcePool = x.ResourcePool,
                                       WorkLocation = x.WorkLocation
                                   }).FirstOrDefault(),
                                   IsProjected = true
                               }).ToList();

                lstRAExtension = (from pra in dbContext.PMSAllocationRequest
                                  join e in dbContext.PMSAllocationExtentionRequest on pra.RequestID equals e.RequestID
                                  where e.EmpID == userID && e.Status == 1 && pra.IsActionPerformed == false
                                  orderby pra.RequestID descending
                                  select new CurrentResourceAllocationModel
                                  {
                                      ID = pra.RequestID,
                                      RequestType = 3,
                                      PersonID = e.EmpID,
                                      FullName = dbContext.People.Where(x => x.ID == e.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      ProjectID = pra.ProjectID,
                                      ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                                      percentage = e.Percentage,
                                      FromDate = e.FromDate,
                                      ToDate = e.ToDate,
                                      ProjectRole = e.ProjectRole,
                                      RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == e.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                                      IsDeleted = pra.IsDeleted,
                                      ReportingTo = e.ReportingTo,
                                      ReportingToName = dbContext.People.Where(x => x.ID == e.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                      BillbleType = e.BillableType,
                                      BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == e.BillableType).Select(x => x.Discription).FirstOrDefault(),
                                      EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == e.EmpID).Select(x => new RAEmploymentDetails()
                                      {
                                          DeliveryTeam = dbContext.ProjectList.Where(t => t.ID == pra.ProjectID).Select(t => t.DeliveryTeam).FirstOrDefault(),//x.DeliveryTeam,
                                          DeliveryUnit = dbContext.ProjectList.Where(y => x.ID == pra.ProjectID).Select(y => y.DeliveryUnit).FirstOrDefault(),//x.DeliveryUnit,
                                          ResourcePool = x.ResourcePool,
                                          WorkLocation = x.WorkLocation
                                      }).FirstOrDefault(),
                                      IsProjected = true
                                  }).ToList();

                //lstRARelease = (from pra in dbContext.PMSAllocationRequest
                //                join rr in dbContext.PMSAllocationReleaseRequest on pra.RequestID equals rr.RequestID
                //                where rr.EmpID == userID && rr.Status == 1 && pra.IsActionPerformed == false
                //                orderby pra.RequestID descending
                //                select new CurrentResourceAllocationModel
                //                {
                //                    ID = pra.RequestID,
                //                    RequestType = 4,
                //                    PersonID = rr.EmpID,
                //                    FullName = dbContext.People.Where(x => x.ID == rr.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                //                    ProjectID = pra.ProjectID,
                //                    ProjectName = dbContext.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault(),
                //                    percentage = rr.Percentage,
                //                    FromDate = rr.FromDate,
                //                    ToDate = rr.ToDate,
                //                    ProjectRole = rr.ProjectRole,
                //                    RoleName = dbContext.PMSRoles.Where(r => r.PMSRoleID == rr.ProjectRole).Select(r => r.PMSRoleDescription).FirstOrDefault(),
                //                    IsDeleted = pra.IsDeleted,
                //                    ReportingTo = rr.ReportingTo,
                //                    ReportingToName = dbContext.People.Where(x => x.ID == rr.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                //                    BillbleType = rr.BillableType,
                //                    BillbleTypeName = dbContext.PMSAllocationBillableType.Where(x => x.ID == rr.BillableType).Select(x => x.Discription).FirstOrDefault(),
                //                    EmploymentDetails = dbContext.PersonEmployment.Where(x => x.PersonID == rr.EmpID).Select(x => new RAEmploymentDetails()
                //                    {
                //                        DeliveryTeam = x.DeliveryTeam,
                //                        DeliveryUnit = x.DeliveryUnit,
                //                        ResourcePool = x.ResourcePool,
                //                        WorkLocation = x.WorkLocation
                //                    }).FirstOrDefault(),
                //                    IsProjected = true
                //                }).ToList();

                lstRAProjected = lstRANew.Concat(lstRAUpdate).Concat(lstRAExtension);

                return lstRAProjected;
            }
        }
        #endregion

        #region RA Get PMS Roles
        public async Task<List<PMSRolesViewModel>> RAGetPMSRoles()
        {
            var pmsRoles = new List<PMSRolesViewModel>();
            await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var result = _db.PMSRoles.Where(c => c.IsDeleted != true).ToList();
                    pmsRoles = Mapper.Map<List<PMSRolesViewModel>>(result);
                }
            });
            return pmsRoles;
        }
        #endregion

        #region RA Get PMS Billable Type
        public async Task<List<PMSAllocationBillableType>> RAGetPMSBillableType()
        {
            var lstPMSBillable = new List<PMSAllocationBillableType>();
            try
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                lstPMSBillable = (from b in dbContext.PMSAllocationBillableType.Where(x => x.IsDeleted == false)
                                  select b).ToList();
                return await Task.Run(() => { return lstPMSBillable; });
            }
            catch
            {
                return lstPMSBillable;
            }
        }
        #endregion

        #region RA Percentage Availability
        private List<ResourceAllocationResponse> RAPercentageAvailability(RAGetRaisedRequest model, PhoenixEntities dbContext)
        {
            List<ResourceAllocationResponse> lstResponse = new List<ResourceAllocationResponse>();
            int personID = 0;
            int RequestedPercentage = 0;
            int projectId = 0;
            int subProjectId = 0;
            List<int> lsttotalPercentage = new List<int>();
            List<int> lstProjectedPercentage = new List<int>();
            List<int> lstNewPercentage = new List<int>();
            List<int> lstUpdatePercentage = new List<int>();
            List<int> lstExtensionPercentage = new List<int>();
            List<int> lstReleasePercentage = new List<int>();
            IEnumerable<RAPercentage> projectedPercentage;
            List<RAPercentage> lstprojectedPercentage = new List<RAPercentage>();
            List<RAPercentage> lstprojectedReleasePercentage = new List<RAPercentage>();
            int requestedProject = 0;
            int actualRequested = 0;
            int totalProjected = 0;
            int totalCurrent = 0;
            int totalUtilized = 0;
            int availablePercentage = 0;
            DateTime startDate;
            DateTime endDate;
            if (model.RequestType == 1)
            {
                for (int i = 0; i < model.RANewRequest.Count; i++)
                {
                    personID = model.RANewRequest[i].EmpID;
                    startDate = model.RANewRequest[i].StartDate;
                    endDate = model.RANewRequest[i].EndDate;
                    RequestedPercentage = model.RANewRequest[i].Allocation;
                    if (RequestedPercentage == 0)
                    {
                        ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        objResponse.PersonID = personID;
                        objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        objResponse.Percentage = RequestedPercentage;
                        objResponse.Response = "Available";
                        objResponse.IsSuccess = true;
                        lstResponse.Add(objResponse);
                    }
                    else
                    {
                        projectId = model.ProjectID;
                        subProjectId = model.SubProjectID;

                        List<RAPercentage> projectedNewPercentage = (from r in dbContext.PMSAllocationRequest
                                                                     join n in dbContext.PMSAllocationNewRequest on r.RequestID equals n.RequestID
                                                                     where n.EmpID == personID && n.Status == 1 && r.IsActionPerformed == false
                                                                     select new RAPercentage
                                                                     {
                                                                         ProjectID = r.ProjectID,
                                                                         PersonID = n.EmpID,
                                                                         Percentage = n.Percentage,
                                                                         FromDate = n.FromDate,
                                                                         ToDate = n.ToDate,
                                                                         ActionDate = n.ActionDate,
                                                                         BillableType = n.BillableType
                                                                     }).ToList();

                        List<RAPercentage> projectedUpdatePercentage = (from r in dbContext.PMSAllocationRequest
                                                                        join u in dbContext.PMSAllocationUpdateRequest on r.RequestID equals u.RequestID
                                                                        where u.EmpID == personID && u.Status == 1 && r.IsActionPerformed == false
                                                                        select new RAPercentage
                                                                        {
                                                                            ProjectID = r.ProjectID,
                                                                            PersonID = u.EmpID,
                                                                            Percentage = u.Percentage ?? 0,
                                                                            FromDate = u.FromDate ?? DateTime.MinValue,
                                                                            ToDate = u.ToDate ?? DateTime.MinValue,
                                                                            ActionDate = u.ActionDate,
                                                                            BillableType = u.BillableType ?? 0
                                                                        }).ToList();

                        //List<RAPercentage> projectedExtendPercentage = (from r in dbContext.PMSAllocationRequest
                        //                                                join e in dbContext.PMSAllocationExtentionRequest on r.RequestID equals e.RequestID
                        //                                                where e.EmpID == personID && e.Status == 1 && r.IsActionPerformed == false
                        //                                                select new RAPercentage
                        //                                                {
                        //                                                    ProjectID = r.ProjectID,
                        //                                                    PersonID = e.EmpID,
                        //                                                    Percentage = e.Percentage,
                        //                                                    FromDate = e.FromDate,
                        //                                                    ToDate = e.ToDate,
                        //                                                    ActionDate = e.ActionDate
                        //                                                }).ToList();


                        projectedPercentage = projectedNewPercentage.Concat(projectedUpdatePercentage);
                        lstprojectedPercentage = projectedPercentage.Where(x => ((
                                                                                    (x.FromDate <= startDate) && (x.ToDate >= startDate)) ||
                                                                                    ((x.FromDate <= endDate) && (x.ToDate >= endDate)) ||
                                                                                    ((x.FromDate >= startDate) && (x.ToDate <= endDate))
                                                                                 )).ToList();

                        totalProjected = lstprojectedPercentage.Sum(x => x.Percentage);
                        List<RAPercentage> currentPercentage = (from c in dbContext.PMSResourceAllocation.Where(r => (r.PersonID == personID && (r.ReleaseDate >= startDate || !r.ReleaseDate.HasValue)))
                                                                select new RAPercentage
                                                                {
                                                                    ProjectID = c.ProjectID,
                                                                    PersonID = c.PersonID,
                                                                    Percentage = c.percentage,
                                                                    ActionDate = c.ReleaseDate,
                                                                    BillableType = c.BillbleType
                                                                }).ToList();

                        totalCurrent = currentPercentage.Sum(x => x.Percentage);
                        totalUtilized = totalCurrent + totalProjected;

                        //requestedProject = dbContext.PMSResourceAllocation.Where(r => r.PersonID == personID && r.ProjectID == projectId).Sum(r => r.percentage);
                        //actualRequested = RequestedPercentage - requestedProject;
                        availablePercentage = 100 - totalUtilized;
                        ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        objResponse.PersonID = personID;
                        objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        objResponse.Percentage = RequestedPercentage;

                        if (currentPercentage.Any() && currentPercentage.Exists(x => x.ProjectID == model.ProjectID && x.BillableType == model.RANewRequest[0].Billability))
                        {
                            objResponse.Response = objResponse.PersonName + " is already allocated for requested project and billable Type";
                            objResponse.IsSuccess = false;
                        }
                        else if (projectedNewPercentage.Exists(x => x.ProjectID == model.ProjectID && x.BillableType == model.RANewRequest[0].Billability))
                        {
                            objResponse.Response = objResponse.PersonName + " is already being requested for the requested project and billable Type";
                            objResponse.IsSuccess = false;
                        }
                        else if (projectedUpdatePercentage.Exists(x => x.ProjectID == model.ProjectID && x.BillableType == model.RANewRequest[0].Billability))
                        {
                            objResponse.Response = "Update Allocation request is being already requested for the requested project and billable Type respectively for " + objResponse.PersonName;
                            objResponse.IsSuccess = false;
                        }
                        else if (availablePercentage >= RequestedPercentage)
                        {
                            objResponse.Response = "Available";
                            objResponse.IsSuccess = true;
                        }
                        else
                        {
                            objResponse.Response = objResponse.PersonName + " Not Available For " + objResponse.Percentage + "%";
                            objResponse.IsSuccess = false;
                        }
                        lstResponse.Add(objResponse);
                    }
                }
            }
            else if (model.RequestType == 2)
            {
                for (int i = 0; i < model.RAUpdateRequest.Count; i++)
                {
                    personID = model.RAUpdateRequest[i].EmpID;
                    startDate = model.RAUpdateRequest[i].ActionDate ?? DateTime.MinValue;
                    startDate = startDate.AddDays(1);
                    endDate = model.RAUpdateRequest[i].EndDate ?? DateTime.MinValue;
                    RequestedPercentage = model.RAUpdateRequest[i].Allocation ?? default(int);
                    if (RequestedPercentage == 0)
                    {
                        ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        objResponse.PersonID = personID;
                        objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        objResponse.Percentage = RequestedPercentage;
                        objResponse.Response = "Available";
                        objResponse.IsSuccess = true;
                        lstResponse.Add(objResponse);
                    }
                    else
                    {
                        projectId = model.ProjectID;
                        List<RAPercentage> projectedNewPercentage = (from r in dbContext.PMSAllocationRequest
                                                                     join n in dbContext.PMSAllocationNewRequest on r.RequestID equals n.RequestID
                                                                     where n.EmpID == personID && n.Status == 1 && r.IsActionPerformed == false
                                                                     select new RAPercentage
                                                                     {
                                                                         ProjectID = r.ProjectID,
                                                                         PersonID = n.EmpID,
                                                                         Percentage = n.Percentage,
                                                                         FromDate = n.FromDate,
                                                                         ToDate = n.ToDate,
                                                                         ActionDate = n.ActionDate,
                                                                         BillableType = n.BillableType
                                                                     }).ToList();

                        List<RAPercentage> projectedUpdatePercentage = (from r in dbContext.PMSAllocationRequest
                                                                        join u in dbContext.PMSAllocationUpdateRequest on r.RequestID equals u.RequestID
                                                                        where u.EmpID == personID && u.Status == 1 && r.IsActionPerformed == false
                                                                        select new RAPercentage
                                                                        {
                                                                            ProjectID = r.ProjectID,
                                                                            PersonID = u.EmpID,
                                                                            Percentage = u.Percentage ?? 0,
                                                                            FromDate = u.FromDate ?? DateTime.MinValue,
                                                                            ToDate = u.ToDate ?? DateTime.MinValue,
                                                                            ActionDate = u.ActionDate,
                                                                            BillableType = u.BillableType ?? 0
                                                                        }).ToList();

                        //List<RAPercentage> projectedExtendPercentage = (from r in dbContext.PMSAllocationRequest
                        //                                                join e in dbContext.PMSAllocationExtentionRequest on r.RequestID equals e.RequestID
                        //                                                where e.EmpID == personID && e.Status == 1 && r.IsActionPerformed == false
                        //                                                select new RAPercentage
                        //                                                {
                        //                                                    ProjectID = r.ProjectID,
                        //                                                    PersonID = e.EmpID,
                        //                                                    Percentage = e.Percentage,
                        //                                                    FromDate = e.FromDate,
                        //                                                    ToDate = e.ToDate,
                        //                                                    ActionDate = e.ActionDate
                        //                                                }).ToList();


                        projectedPercentage = projectedNewPercentage.Concat(projectedUpdatePercentage);
                        lstprojectedPercentage = projectedPercentage.Where(x => ((
                                                                                    (x.FromDate <= startDate) && (x.ToDate >= startDate)) ||
                                                                                    ((x.FromDate <= endDate) && (x.ToDate >= endDate)) ||
                                                                                    ((x.FromDate >= startDate) && (x.ToDate <= endDate))
                                                                                 )).ToList();

                        totalProjected = lstprojectedPercentage.Sum(x => x.Percentage);
                        List<RAPercentage> currentPercentage = (from c in dbContext.PMSResourceAllocation.Where(r => (r.PersonID == personID && (r.ReleaseDate >= startDate || !r.ReleaseDate.HasValue)))
                                                                select new RAPercentage
                                                                {
                                                                    ProjectID = c.ProjectID,
                                                                    PersonID = c.PersonID,
                                                                    Percentage = c.percentage,
                                                                    ActionDate = c.ReleaseDate,
                                                                    BillableType = c.BillbleType
                                                                }).ToList();

                        int allocId = model.RAUpdateRequest[0].AllocationID;
                        var alloc = dbContext.PMSResourceAllocation.Where(r => r.ID == allocId).Select(x => x.percentage).ToList();
                        if (alloc.Any())
                            requestedProject = alloc.First();
                        else
                            requestedProject = 0;
                        totalCurrent = currentPercentage.Sum(x => x.Percentage);
                        totalUtilized = totalCurrent + totalProjected;
                        actualRequested = RequestedPercentage - requestedProject;
                        availablePercentage = 100 - totalUtilized;
                        ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        objResponse.PersonID = personID;
                        objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        objResponse.Percentage = RequestedPercentage;

                        //if (currentPercentage.Any() && currentPercentage.Exists(x => x.ProjectID == model.ProjectID && x.BillableType == model.RAUpdateRequest[0].Billability && x.Percentage != RequestedPercentage))
                        //{
                        //    objResponse.Response = objResponse.PersonName + " is already allocated for requested project and billable Type";
                        //    objResponse.IsSuccess = false;
                        //}
                        //else 
                        if (projectedNewPercentage.Exists(x => x.ProjectID == model.ProjectID && x.BillableType == model.RAUpdateRequest[0].Billability))
                        {
                            objResponse.Response = objResponse.PersonName + " is already being requested for the requested project and billable Type";
                            objResponse.IsSuccess = false;
                        }
                        else if (projectedUpdatePercentage.Exists(x => x.ProjectID == model.ProjectID && x.BillableType == model.RAUpdateRequest[0].Billability))
                        {
                            objResponse.Response = "Update Allocation request is being already requested for the requested project and billable Type respectively for " + objResponse.PersonName;
                            objResponse.IsSuccess = false;
                        }
                        else if (availablePercentage >= actualRequested)
                        {
                            objResponse.Response = "Available";
                            objResponse.IsSuccess = true;
                        }
                        else
                        {
                            objResponse.Response = objResponse.Response = objResponse.PersonName + " Not Available For " + objResponse.Percentage + "%";
                            objResponse.IsSuccess = false;
                        }
                        lstResponse.Add(objResponse);
                    }
                }
            }
            else if (model.RequestType == 3)
            {
                for (int i = 0; i < model.RAExtentionRequest.Count; i++)
                {
                    personID = model.RAExtentionRequest[i].EmpID;
                    startDate = model.RAExtentionRequest[i].StartDate;
                    endDate = model.RAExtentionRequest[i].EndDate;
                    RequestedPercentage = model.RAExtentionRequest[i].Allocation;
                    if (RequestedPercentage == 0)
                    {
                        ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        objResponse.PersonID = personID;
                        objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        objResponse.Percentage = RequestedPercentage;
                        objResponse.Response = "Available";
                        objResponse.IsSuccess = true;
                        lstResponse.Add(objResponse);
                    }
                    else
                    {
                        projectId = model.ProjectID;
                        subProjectId = model.SubProjectID;

                        List<RAPercentage> projectedNewPercentage = (from r in dbContext.PMSAllocationRequest
                                                                     join n in dbContext.PMSAllocationNewRequest on r.RequestID equals n.RequestID
                                                                     where n.EmpID == personID && n.Status == 1 && r.IsActionPerformed == false
                                                                     select new RAPercentage
                                                                     {
                                                                         ProjectID = r.ProjectID,
                                                                         PersonID = n.EmpID,
                                                                         Percentage = n.Percentage,
                                                                         FromDate = n.FromDate,
                                                                         ToDate = n.ToDate,
                                                                         ActionDate = n.ActionDate,
                                                                         BillableType = n.BillableType
                                                                     }).ToList();

                        List<RAPercentage> projectedUpdatePercentage = (from r in dbContext.PMSAllocationRequest
                                                                        join u in dbContext.PMSAllocationUpdateRequest on r.RequestID equals u.RequestID
                                                                        where u.EmpID == personID && u.Status == 1 && r.IsActionPerformed == false
                                                                        select new RAPercentage
                                                                        {
                                                                            ProjectID = r.ProjectID,
                                                                            PersonID = u.EmpID,
                                                                            Percentage = u.Percentage ?? 0,
                                                                            FromDate = u.FromDate ?? DateTime.MinValue,
                                                                            ToDate = u.ToDate ?? DateTime.MinValue,
                                                                            ActionDate = u.ActionDate,
                                                                            BillableType = u.BillableType ?? 0
                                                                        }).ToList();

                        //List<RAPercentage> projectedExtendPercentage = (from r in dbContext.PMSAllocationRequest
                        //                                                join e in dbContext.PMSAllocationExtentionRequest on r.RequestID equals e.RequestID
                        //                                                where e.EmpID == personID && e.Status == 1 && r.IsActionPerformed == false
                        //                                                select new RAPercentage
                        //                                                {
                        //                                                    ProjectID = r.ProjectID,
                        //                                                    PersonID = e.EmpID,
                        //                                                    Percentage = e.Percentage,
                        //                                                    FromDate = e.FromDate,
                        //                                                    ToDate = e.ToDate,
                        //                                                    ActionDate = e.ActionDate
                        //                                                }).ToList();


                        projectedPercentage = projectedNewPercentage.Concat(projectedUpdatePercentage);
                        lstprojectedPercentage = projectedPercentage.Where(x => ((
                                                                                    (x.FromDate <= startDate) && (x.ToDate >= startDate)) ||
                                                                                    ((x.FromDate <= endDate) && (x.ToDate >= endDate)) ||
                                                                                    ((x.FromDate >= startDate) && (x.ToDate <= endDate))
                                                                                 )).ToList();

                        totalProjected = lstprojectedPercentage.Sum(x => x.Percentage);
                        ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        objResponse.PersonID = personID;
                        objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        objResponse.Percentage = RequestedPercentage;
                        if (totalProjected == 0)
                        {
                            objResponse.Response = "Available";
                            objResponse.IsSuccess = true;
                        }
                        else
                        {
                            objResponse.Response = objResponse.PersonName + " having " + totalProjected + "% projected allocation";
                            objResponse.IsSuccess = false;
                        }
                        //List<RAPercentage> currentPercentage = (from c in dbContext.PMSResourceAllocation.Where(r => (r.PersonID == personID && (r.ReleaseDate >= startDate || !r.ReleaseDate.HasValue)))
                        //                                        select new RAPercentage
                        //                                        {
                        //                                            ProjectID = c.ProjectID,
                        //                                            PersonID = c.PersonID,
                        //                                            Percentage = c.percentage,
                        //                                            ActionDate = c.ReleaseDate
                        //                                        }).ToList();

                        //totalCurrent = currentPercentage.Sum(x => x.Percentage);
                        //totalUtilized = totalCurrent + totalProjected;
                        //requestedProject = dbContext.PMSResourceAllocation.Where(r => r.PersonID == personID && r.ProjectID == projectId).Select(r => r.percentage).SingleOrDefault();
                        //actualRequested = RequestedPercentage - requestedProject;
                        //availablePercentage = 100 - totalUtilized;
                        //ResourceAllocationResponse objResponse = new ResourceAllocationResponse();
                        //objResponse.PersonID = personID;
                        //objResponse.PersonName = dbContext.People.Where(x => x.ID == personID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                        //objResponse.Percentage = RequestedPercentage;
                        //if (availablePercentage >= actualRequested)
                        //{
                        //    objResponse.Response = "Available";
                        //    objResponse.IsSuccess = true;
                        //}
                        //else
                        //{
                        //    objResponse.Response = objResponse.PersonName + " Not Available For " + objResponse.Percentage + "%";
                        //    objResponse.IsSuccess = false;
                        //}
                        lstResponse.Add(objResponse);
                    }
                }
            }
            return lstResponse;
        }
        #endregion

        #region RA Current Alloation Change
        public Task<List<ResourceAllocationResponse>> RAPerformActionOnCurrentAllocation(CurrentResourceAllocationModel model)
        {
            return Task.Run(() =>
            {
                List<ResourceAllocationResponse> lstResponse = new List<ResourceAllocationResponse>();
                ResourceAllocationResponse objUpdateRequestResponse = new ResourceAllocationResponse();
                ResourceAllocationResponse objReleaseResponse = new ResourceAllocationResponse();
                bool isTaskCreated = false;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    try
                    {
                        switch (model.RequestType)
                        {
                            case 2:
                                bool isToDateValid = checkExtendToDateValid(dbContext, model.ProjectID, model.ToDate);
                                if (isToDateValid)
                                {
                                    RAGetRaisedRequest objRAGetRaisedRequest = new RAGetRaisedRequest();
                                    objRAGetRaisedRequest.ProjectID = model.ProjectID;
                                    List<RAUpdateRequest> lstRAUpdateRequest = new List<RAUpdateRequest>();
                                    RAUpdateRequest objRARAUpdateRequest = new RAUpdateRequest();
                                    objRARAUpdateRequest.EmpID = model.PersonID;
                                    objRARAUpdateRequest.Allocation = model.percentage;
                                    objRARAUpdateRequest.Billability = model.BillbleType;
                                    objRARAUpdateRequest.StartDate = model.FromDate;
                                    objRARAUpdateRequest.EndDate = model.ToDate;
                                    objRARAUpdateRequest.ProjectReporting = model.ReportingTo;
                                    objRARAUpdateRequest.ProjectRole = model.ProjectRole;
                                    objRARAUpdateRequest.ActionDate = model.ActionDate;
                                    objRARAUpdateRequest.AllocationID = model.AllocationID;
                                    lstRAUpdateRequest.Add(objRARAUpdateRequest);
                                    objRAGetRaisedRequest.RAUpdateRequest = lstRAUpdateRequest;
                                    objRAGetRaisedRequest.RequestType = model.RequestType;
                                    List<ResourceAllocationResponse> lstUpdateResponse = new List<ResourceAllocationResponse>();
                                    lstUpdateResponse = RAPercentageAvailability(objRAGetRaisedRequest, dbContext);
                                    if (lstUpdateResponse.Any(x => x.IsSuccess == false))
                                    {
                                        return lstUpdateResponse;
                                    }
                                    else
                                    {
                                        RARaisedRequest objRARaisedRequestUpdate = new RARaisedRequest();
                                        RARequest objRAUpdateRequest = new RARequest();
                                        objRAUpdateRequest.ProjectID = model.ProjectID;
                                        objRAUpdateRequest.RequestType = model.RequestType;
                                        objRAUpdateRequest.RequestedBy = model.RequestBy;
                                        objRAUpdateRequest.IsRmg = true;
                                        objRARaisedRequestUpdate.Request = objRAUpdateRequest;
                                        objRARaisedRequestUpdate.Request.ID = RARequest(dbContext, objRARaisedRequestUpdate.Request,model.AllocationID);
                                        if (objRARaisedRequestUpdate.Request.ID > 0)
                                        {
                                            List<RARequestDetail> lstRARequestDetail = new List<RARequestDetail>();
                                            RARequestDetail objRequestDetail = new RARequestDetail();
                                            objRequestDetail.IsRmg = true;
                                            objRequestDetail.EmpID = model.PersonID;
                                            objRequestDetail.CreatedBy = model.RequestBy;
                                            objRequestDetail.CreatedDate = DateTime.Now;
                                            objRequestDetail.ModifyBy = model.RequestBy;
                                            objRequestDetail.ModifyDate = DateTime.Now;
                                            if (model.Comments == null || model.Comments == "")
                                            {
                                                //objPMSAllocationUpdateRequest.RMGComments = "Approved By RMG";
                                                objRequestDetail.Comments = "Updated By RMG";
                                                objRequestDetail.RMGComments = "Updated By RMG";
                                            }
                                            else
                                            {
                                                //objPMSAllocationUpdateRequest.RMGComments = model.RequestDetail[i].RMGComments;
                                                objRequestDetail.Comments = model.Comments;
                                                objRequestDetail.RMGComments = model.Comments;
                                            }

                                            lstRARequestDetail.Add(objRequestDetail);
                                            objRARaisedRequestUpdate.RequestDetail = lstRARequestDetail;
                                            List<RAResource> lstRAResource = new List<RAResource>();
                                            RAResource objRAResource = new RAResource();
                                            objRAResource.ProjectRole = model.ProjectRole;
                                            objRAResource.ProjectReporting = model.ReportingTo;
                                            objRAResource.StartDate = model.FromDate;
                                            objRAResource.EndDate = model.ToDate;
                                            objRAResource.Billability = model.BillbleType;
                                            objRAResource.Allocation = model.percentage;
                                            objRAResource.ActionDate = model.ActionDate;
                                            objRAResource.AllocationID = model.AllocationID;
                                            lstRAResource.Add(objRAResource);
                                            objRARaisedRequestUpdate.Resource = lstRAResource;
                                            lstResponse = RAUpdateRequest(dbContext, objRARaisedRequestUpdate);
                                            if (lstResponse.Any(x => x.IsSuccess == false))
                                            {
                                                return lstResponse;
                                            }
                                            else
                                            {
                                                objUpdateRequestResponse.RequestType = 2;
                                                objUpdateRequestResponse.Response = "Update Allocation Successfully";
                                                objUpdateRequestResponse.IsSuccess = true;
                                                lstResponse.Add(objUpdateRequestResponse);
                                            }
                                        }
                                        else
                                        {
                                            objUpdateRequestResponse.Response = "Update Is Not Approved";
                                            objUpdateRequestResponse.IsSuccess = false;
                                            lstResponse.Add(objUpdateRequestResponse);
                                        }
                                    }
                                }
                                else
                                {
                                    objUpdateRequestResponse.Response = "Resource end date should not exceed project or customer end date";
                                    objUpdateRequestResponse.IsSuccess = false;
                                    lstResponse.Add(objUpdateRequestResponse);
                                }
                                break;
                            case 4:
                                RARaisedRequest objRARaisedRequestRelease = new RARaisedRequest();
                                RARequest objRAReleaseRequest = new RARequest();
                                objRAReleaseRequest.ProjectID = model.ProjectID;
                                objRAReleaseRequest.RequestType = model.RequestType;
                                objRAReleaseRequest.RequestedBy = model.RequestBy;
                                objRAReleaseRequest.IsRmg = true;
                                objRARaisedRequestRelease.Request = objRAReleaseRequest;
                                objRARaisedRequestRelease.Request.ID = RARequest(dbContext, objRARaisedRequestRelease.Request, model.AllocationID);
                                if (objRARaisedRequestRelease.Request.ID > 0)
                                {
                                    PMSAllocationReleaseRequest objPMSAllocationReleaseRequest = new PMSAllocationReleaseRequest();
                                    objPMSAllocationReleaseRequest.EmpID = model.PersonID;
                                    objPMSAllocationReleaseRequest.ToDate = model.ToDate;
                                    objPMSAllocationReleaseRequest.Comments = "Released By RMG";
                                    objPMSAllocationReleaseRequest.RequestID = objRARaisedRequestRelease.Request.ID;
                                    objPMSAllocationReleaseRequest.Status = 1; // Approve
                                    objPMSAllocationReleaseRequest.RMGComments = "Released By RMG";
                                    objPMSAllocationReleaseRequest.CreatedBy = model.RequestBy;
                                    objPMSAllocationReleaseRequest.ModifyBy = model.RequestBy;
                                    objPMSAllocationReleaseRequest.CreatedDate = DateTime.Now;
                                    objPMSAllocationReleaseRequest.ModifyDate = DateTime.Now;
                                    objPMSAllocationReleaseRequest.IsDeleted = false;
                                    ///////////////// Insert Record In PMSAllocationReleaseRequest ////////////////////////////////////
                                    dbContext.PMSAllocationReleaseRequest.Add(objPMSAllocationReleaseRequest);
                                    RARequest objRequest = new RARequest();
                                    objRequest.ProjectID = model.ProjectID;
                                    objRARaisedRequestRelease.Request = objRequest;
                                    List<RARequestDetail> lstRARequestDetail = new List<RARequestDetail>();
                                    RARequestDetail objRequestDetailRelease = new RARequestDetail();
                                    objRequestDetailRelease.EmpID = model.PersonID;
                                    objRequestDetailRelease.Comments = "Released By RMG";
                                    objRequestDetailRelease.RMGComments = "Released By RMG";
                                    objRequestDetailRelease.Status = 1;
                                    objRequestDetailRelease.ModifyBy = model.RequestBy;
                                    objRequestDetailRelease.CreatedBy = model.RequestBy;
                                    objRequestDetailRelease.ModifyDate = DateTime.Now;
                                    objRequestDetailRelease.CreatedDate = DateTime.Now;
                                    objRequestDetailRelease.IsDeleted = false;
                                    objRequestDetailRelease.IsRmg = true;
                                    lstRARequestDetail.Add(objRequestDetailRelease);
                                    objRARaisedRequestRelease.RequestDetail = lstRARequestDetail;
                                    List<RAResource> lstRAResourceRelease = new List<RAResource>();
                                    RAResource objRAResourceRelease = new RAResource();
                                    objRAResourceRelease.ProjectReporting = model.ReportingTo;
                                    objRAResourceRelease.Allocation = model.percentage;
                                    objRAResourceRelease.AllocationID = model.AllocationID;
                                    objRAResourceRelease.Billability = model.BillbleType;
                                    objRAResourceRelease.EndDate = model.ToDate;
                                    objRAResourceRelease.ProjectRole = model.ProjectRole;
                                    objRAResourceRelease.StartDate = model.FromDate;
                                    objRAResourceRelease.ActionDate = model.ActionDate;
                                    lstRAResourceRelease.Add(objRAResourceRelease);
                                    objRARaisedRequestRelease.Resource = lstRAResourceRelease;
                                    lstResponse = RAReleaseRequest(dbContext, objRARaisedRequestRelease);
                                    if (lstResponse.Any(x => x.IsSuccess == false))
                                    {
                                        return lstResponse;
                                    }
                                    else
                                    {
                                        objReleaseResponse.RequestType = 4;
                                        objReleaseResponse.Response = "Release Successfully";
                                        objReleaseResponse.IsSuccess = true;
                                    }

                                }
                                else
                                {
                                    objReleaseResponse.Response = "Release Is Not Approved";
                                    objReleaseResponse.IsSuccess = false;
                                }
                                lstResponse.Add(objReleaseResponse);
                                break;
                            default:
                                ResourceAllocationResponse objInvalidResponse = new ResourceAllocationResponse();
                                objInvalidResponse.RequestType = -1;
                                objInvalidResponse.Response = "Invalid Request Type";
                                objInvalidResponse.IsSuccess = false;
                                lstResponse.Add(objInvalidResponse);
                                break;
                        }
                        dbContext.SaveChanges();
                        isTaskCreated = true;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if (isTaskCreated)
                    service.Finalize(true);

                return lstResponse;
            });
        }
        #endregion
        //#163086816 On Hold
        public bool DeleteRAData(int UserId, DateTime ApprovalDate)
        {
            var flag = false;
            bool isSuccsess = false;
            PMSResourceAllocation pmsresAllocation = new PMSResourceAllocation();

            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    int requestType = 4;
                    var ResouceAllcationData = dbContext.PMSResourceAllocation.Where(x => x.PersonID == UserId).ToList();
                    if (ResouceAllcationData.Count != 0)
                    {
                        ResouceAllcationData.ForEach(resAllocData =>
                        {
                            resAllocData.ReleaseDate = ApprovalDate;
                            //---Update PMSResourceAllocationHistory table with ReleaseDate = ExitDate. 
                            isSuccsess = RAUpdateHistoryData(resAllocData, dbContext, requestType);
                            if (isSuccsess)
                            {
                                // -- delete record from PMSResourceAllocation
                                dbContext.PMSResourceAllocation.Remove(resAllocData);
                                dbContext.SaveChanges();
                                // --- call email service to send email to Manager.
                                RAResource model = new RAResource();
                                model.EmpID = resAllocData.PersonID;
                                model.ProjectID = resAllocData.ProjectID;
                                model.ActionDate = resAllocData.ReleaseDate.Value.AddDays(+1);
                                model.Allocation = resAllocData.percentage;
                                model.Comments = "Auto Release By System";
                                model.Billability = resAllocData.BillbleType;
                                model.ProjectReporting = resAllocData.ReportingTo;
                                model.ProjectRole = resAllocData.ProjectRole;
                                model.StartDate = resAllocData.FromDate;
                                model.EndDate = resAllocData.ToDate;
                                model.StatusBy = resAllocData.CreatedBy;
                                emailService.EmployeeReleaseEmail(model);
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    flag = false;
                }
                return flag;
            }
        }
        // #163086816 On Hold
        //private bool SendExitProcessCompeletionEmail(int UserId)
        //{
        //    var isCreated = false;

        //    using (PhoenixEntities dbcontext = new PhoenixEntities())
        //    {
        //        var EmailTemlpates = dbcontext.EmailTemplate.Where(x => x.TemplateFor == "ExitCompletionWithProjectRelease").FirstOrDefault();
        //        if (EmailTemlpates != null)
        //        {
        //            string template = EmailTemlpates.Html;
        //            var subject = EmailTemlpates.Subjects;
        //            var person = service.First<Person>(x => x.ID == UserId);
        //            var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault().EmailGroup;
        //            var Username = dbcontext.PersonEmployment.Where(x => x.ID == UserId).Select(x => x.UserName);
        //            var RptMgrId = dbcontext.PersonReporting.Where(x => x.PersonID == UserId).Select(x => x.ReportingTo).FirstOrDefault();
        //            var RptMgremail = dbcontext.PersonEmployment.Where(x => x.PersonID == RptMgrId).Select(x => x.OrganizationEmail).FirstOrDefault();

        //            var ProjIds = dbcontext.PMSResourceAllocation.Where(x => x.PersonID == UserId).Select(x => x.ProjectID).ToList();
        //            var projName = dbcontext.ProjectList.Where(x => ProjIds.Contains(x.ID)).Select(x => x.ProjectName).ToList();
        //            var ProjnameLst = string.Join(",", projName.ToArray());

        //            var exitprocMgrID = dbcontext.PersonEmployment.Where(x => x.ID == UserId).Select(x => x.ExitProcessManager).FirstOrDefault();
        //            var exitprocMgremail = dbcontext.PersonEmployment.Where(x => x.ID == exitprocMgrID).Select(x => x.OrganizationEmail).FirstOrDefault();

        //            var DMids = dbcontext.ProjectList.Where(x => ProjIds.Contains(x.ID)).Select(x => x.DeliveryManager).Distinct();
        //            var DMSemails = dbcontext.PersonEmployment.Where(x => DMids.Contains(x.PersonID)).Select(x => x.OrganizationEmail);
        //            var DMemailLst = string.Join(",", DMSemails);

        //            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
        //            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
        //            template = template.Replace("{{username}}", Username.ToString());
        //            template = template.Replace("{{employeeid}}", UserId.ToString());
        //            template = template.Replace("{{projectname}}", ProjnameLst); 

        //            try
        //            {
        //                isCreated = service.Create<Emails>(new Emails
        //                {
        //                    Content = template,
        //                    Date = DateTime.Now,
        //                    EmailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]),
        //                    EmailTo = RptMgremail + "," + exitprocMgremail,
        //                    Subject = subject,
        //                    EmailCC = RMG + "," + DMemailLst,
        //                }, e => e.Id == 0);

        //                if (isCreated)
        //                    service.Finalize(true);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //        return isCreated;
        //    }
        //}


        #region RA Update History Data
        private bool RAUpdateHistoryData(PMSResourceAllocation model, PhoenixEntities dbContext, int requestType)
        {
            bool isSuccsess = false;
            try
            {
                PMSResourceAllocationHistory objPMSResourceAllocation = new PMSResourceAllocationHistory();
                objPMSResourceAllocation.ResourceAllocationID = model.ID;
                objPMSResourceAllocation.ProjectID = model.ProjectID;
                objPMSResourceAllocation.PersonID = model.PersonID;
                objPMSResourceAllocation.ReportingTo = model.ReportingTo;
                objPMSResourceAllocation.ProjectRole = model.ProjectRole;
                objPMSResourceAllocation.ToDate = model.ToDate;
                objPMSResourceAllocation.FromDate = model.FromDate;
                objPMSResourceAllocation.percentage = model.percentage;
                objPMSResourceAllocation.BillbleType = model.BillbleType;
                objPMSResourceAllocation.IsDeleted = false;
                objPMSResourceAllocation.ModifyBy = model.CreatedBy;
                objPMSResourceAllocation.ModifyDate = DateTime.Now;
                if (model.ReleaseDate == null || model.ReleaseDate == default(DateTime))
                {
                    objPMSResourceAllocation.ReleaseDate = DateTime.Now.Date.AddDays(-1);//model.ReleaseDate ?? DateTime.MinValue;
                }
                else
                {
                    objPMSResourceAllocation.ReleaseDate = model.ReleaseDate ?? DateTime.MinValue;
                }

                objPMSResourceAllocation.RequestType = requestType;
                dbContext.PMSResourceAllocationHistory.Add(objPMSResourceAllocation);
                dbContext.SaveChanges();
                return isSuccsess = true;
            }
            catch (Exception e)
            {
                return isSuccsess;
            }
        }
        #endregion

        #region Update Request Action
        private bool UpdateRequestAction(int requestID)
        {
            bool isSuccsess = false;
            try
            {
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    PMSAllocationRequest r = (from x in dbContext.PMSAllocationRequest
                                              where x.RequestID == requestID
                                              select x).FirstOrDefault();
                    r.IsActionPerformed = true;
                    dbContext.Entry(r).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return isSuccsess = true;
                }
            }
            catch
            {
                return isSuccsess;
            }
        }
        #endregion

        //#region Get User Allocation
        //private void IsAlreadyAllocated() 
        //{
        //    throw NotImplementedException();
        //}
        //#endregion
    }
}

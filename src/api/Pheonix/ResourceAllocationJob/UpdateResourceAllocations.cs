using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.VM.Classes.ResourceAllocation;

namespace ResourceAllocationJob
{
    class UpdateResourceAllocations
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            Log4Net.Debug("Resource allocation job started: =" + DateTime.Now);
            try
            {
                IResourceAllocationService _service;
                IBasicOperationsService service = new BasicOperationsService(new ContextRepository<PhoenixEntities>(new PhoenixEntities()));
                _service = new ResourceAllocationService(service, new EmailSendingService(service));
                bool isAction = false;
                List<RAResource> resourceNew = new List<RAResource>();
                List<RAResource> resourceUpdate = new List<RAResource>();
                List<RAResource> resourceExtention = new List<RAResource>();
                List<RAResource> resourceRelease = new List<RAResource>();
                List<RAGetRaisedRequest> lstRARequestData = new List<RAGetRaisedRequest>();
                lstRARequestData = GetRARequestData();
                if (lstRARequestData.Count > 0)
                {
                    resourceNew = GetNewResourceForCA(lstRARequestData);
                    if (resourceNew.Count > 0)
                    {
                        isAction = _service.RAApproveDirectNewRequest(resourceNew);
                    }

                    resourceUpdate = GetUpdateResourceForCA(lstRARequestData);
                    if (resourceUpdate.Count > 0)
                    {
                        isAction = _service.RAApproveDirectUpdateRequest(resourceUpdate);
                    }

                    //resourceExtention = GetExtendResourceForCA(lstRARequestData);
                    //if (resourceExtention.Count > 0)
                    //{
                    //    isAction = _service.RAApproveDirectExtentionRequest(resourceExtention);
                    //}

                    resourceRelease = GetReleaseResourceForCA(lstRARequestData);
                    if (resourceRelease.Count > 0)
                    {
                        isAction = _service.RAApproveDirectReleaseRequest(resourceRelease);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
                //entites.Dispose();
            }
            Log4Net.Debug("Absent Tracing job finished: =" + DateTime.Now);
        }

        private static List<RAGetRaisedRequest> GetRARequestData()
        {
            List<RAGetRaisedRequest> lstRARequestData = new List<RAGetRaisedRequest>();
            try
            {
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    lstRARequestData = (from r in dbContext.PMSAllocationRequest
                                        where r.Status == 1 && r.IsActionPerformed == false
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
                                            StatusBy = r.StatusBy ?? 0,
                                            RANewRequest = (from n in dbContext.PMSAllocationNewRequest
                                                            where n.RequestID == r.RequestID && DbFunctions.TruncateTime(n.ActionDate) <= DbFunctions.AddDays(DateTime.Today, 0)
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
                                                                ActionDate = n.ActionDate,
                                                                BGStatus = n.BGStatus
                                                            }).ToList(),

                                            RAUpdateRequest = (from u in dbContext.PMSAllocationUpdateRequest
                                                               where u.RequestID == r.RequestID && DbFunctions.TruncateTime(u.ActionDate) <= DbFunctions.AddDays(DateTime.Today, 0)
                                                               select new RAUpdateRequest
                                                               {
                                                                   Billability = u.BillableType,
                                                                   Comments = u.Comments,
                                                                   StartDate = u.FromDate,
                                                                   EndDate = u.ToDate,
                                                                   Allocation = u.Percentage,
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
                                                                   ActionDate = u.ActionDate
                                                               }).ToList(),

                                            //RAExtentionRequest = (from e in dbContext.PMSAllocationExtentionRequest
                                            //                      where e.RequestID == r.RequestID && DbFunctions.TruncateTime(e.ActionDate) <= DbFunctions.AddDays(DateTime.Today, 0)
                                            //                      select new RAExtentionRequest
                                            //                      {
                                            //                          Comments = e.Comments,
                                            //                          EndDate = e.ToDate,
                                            //                          EmpID = e.EmpID,
                                            //                          FullName = dbContext.People.Where(x => x.ID == e.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                            //                          RequestID = e.RequestID,
                                            //                          Status = e.Status,
                                            //                          CreatedBy = e.CreatedBy,
                                            //                          CreatedDate = e.CreatedDate,
                                            //                          ModifyBy = e.ModifyBy,
                                            //                          ModifyDate = e.ModifyDate,
                                            //                          RMGComments = e.RMGComments,
                                            //                          ActionDate = e.ActionDate
                                            //                      }).ToList(),
                                            RAReleaseRequest = (from rr in dbContext.PMSAllocationReleaseRequest
                                                                where rr.RequestID == r.RequestID && DbFunctions.TruncateTime(rr.ActionDate) <= DbFunctions.AddDays(DateTime.Today, 0) && rr.Status == 1
                                                                select new RAReleaseRequest
                                                                {
                                                                    Comments = rr.Comments,
                                                                    EndDate = rr.ToDate,
                                                                    EmpID = rr.EmpID,
                                                                    FullName = dbContext.People.Where(x => x.ID == rr.EmpID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                    Billability = rr.BillableType,
                                                                    StartDate = rr.FromDate,
                                                                    Allocation = rr.Percentage,
                                                                    ProjectRole = rr.ProjectRole,
                                                                    RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == rr.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault(),
                                                                    ProjectReporting = rr.ReportingTo,
                                                                    ProjectReportingName = dbContext.People.Where(x => x.ID == rr.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                                                    RequestID = r.RequestID,
                                                                    Status = rr.Status,
                                                                    CreatedBy = rr.CreatedBy,
                                                                    CreatedDate = rr.CreatedDate,
                                                                    ModifyBy = rr.ModifyBy,
                                                                    ModifyDate = rr.ModifyDate,
                                                                    RMGComments = rr.RMGComments,
                                                                    ActionDate = rr.ActionDate,
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
            }
            catch (Exception ex)
            {
                return lstRARequestData;
            }
            return lstRARequestData;
        }

        private static List<RAResource> GetNewResourceForCA(List<RAGetRaisedRequest> lstRARequestData)
        {
            List<RAResource> lstresource = new List<RAResource>();

            int j = 0;
            while (j < lstRARequestData.Count)
            {
                for (int i = 0; i < lstRARequestData[j].RANewRequest.Count; i++)
                {
                    RAResource objResource = new RAResource();
                    objResource.ProjectID = lstRARequestData[j].ProjectID;
                    objResource.EmpID = lstRARequestData[j].RANewRequest[i].EmpID;
                    objResource.Allocation = lstRARequestData[j].RANewRequest[i].Allocation;
                    objResource.Billability = lstRARequestData[j].RANewRequest[i].Billability;
                    objResource.ProjectReporting = lstRARequestData[j].RANewRequest[i].ProjectReporting;
                    objResource.ProjectRole = lstRARequestData[j].RANewRequest[i].ProjectRole;
                    objResource.StartDate = lstRARequestData[j].RANewRequest[i].StartDate;
                    objResource.EndDate = lstRARequestData[j].RANewRequest[i].EndDate;
                    objResource.RequestedBy = lstRARequestData[j].RequestedBy;
                    objResource.ActionDate = lstRARequestData[j].RANewRequest[i].ActionDate;
                    objResource.RequestID = lstRARequestData[j].ID;
                    objResource.StatusBy = lstRARequestData[j].StatusBy;
                    objResource.Comments = lstRARequestData[j].RANewRequest[i].RMGComments;
                    if (lstRARequestData[j].RANewRequest[i].BGStatus == 1)
                    {
                        objResource.IsBGCRequired = true;
                    }
                    else
                    {
                        objResource.IsBGCRequired = false;
                    }
                    lstresource.Add(objResource);
                }
                j++;
            }
            return lstresource;
        }

        private static List<RAResource> GetUpdateResourceForCA(List<RAGetRaisedRequest> lstRARequestData)
        {
            List<RAResource> lstresource = new List<RAResource>();
            int j = 0;
            while (j < lstRARequestData.Count)
            {
                for (int i = 0; i < lstRARequestData[j].RAUpdateRequest.Count; i++)
                {
                    RAResource objResource = new RAResource();
                    objResource.ProjectID = lstRARequestData[j].ProjectID;
                    objResource.EmpID = lstRARequestData[j].RAUpdateRequest[i].EmpID;
                    objResource.Allocation = lstRARequestData[j].RAUpdateRequest[i].Allocation ?? 0;
                    objResource.Billability = lstRARequestData[j].RAUpdateRequest[i].Billability ?? 0;
                    objResource.ProjectReporting = lstRARequestData[j].RAUpdateRequest[i].ProjectReporting ?? 0;
                    objResource.ProjectRole = lstRARequestData[j].RAUpdateRequest[i].ProjectRole ?? 0;
                    objResource.StartDate = lstRARequestData[j].RAUpdateRequest[i].StartDate ?? DateTime.MinValue;
                    objResource.EndDate = lstRARequestData[j].RAUpdateRequest[i].EndDate ?? DateTime.MinValue;
                    objResource.RequestedBy = lstRARequestData[j].RequestedBy;
                    objResource.ActionDate = lstRARequestData[j].RAUpdateRequest[i].ActionDate;
                    objResource.RequestID = lstRARequestData[j].ID;
                    objResource.StatusBy = lstRARequestData[j].StatusBy;
                    objResource.Comments = lstRARequestData[j].RAUpdateRequest[i].RMGComments;
                    lstresource.Add(objResource);
                }
                j++;
            }
            return lstresource;
        }

        private static List<RAResource> GetExtendResourceForCA(List<RAGetRaisedRequest> lstRARequestData)
        {
            List<RAResource> lstresource = new List<RAResource>();
            int j = 0;
            while (j < lstRARequestData.Count)
            {
                for (int i = 0; i < lstRARequestData[j].RAExtentionRequest.Count; i++)
                {
                    RAResource objResource = new RAResource();
                    objResource.ProjectID = lstRARequestData[j].ProjectID;
                    objResource.EmpID = lstRARequestData[j].RAExtentionRequest[i].EmpID;
                    objResource.Allocation = lstRARequestData[j].RAExtentionRequest[i].Allocation;
                    objResource.Billability = lstRARequestData[j].RAExtentionRequest[i].Billability;
                    objResource.ProjectReporting = lstRARequestData[j].RAExtentionRequest[i].ProjectReporting;
                    objResource.ProjectRole = lstRARequestData[j].RAExtentionRequest[i].ProjectRole;
                    objResource.StartDate = lstRARequestData[j].RAExtentionRequest[i].StartDate;
                    objResource.EndDate = lstRARequestData[j].RAExtentionRequest[i].EndDate;
                    objResource.RequestedBy = lstRARequestData[j].RequestedBy;
                    objResource.ActionDate = lstRARequestData[j].RAExtentionRequest[i].ActionDate;
                    objResource.RequestID = lstRARequestData[j].ID;
                    objResource.StatusBy = lstRARequestData[j].StatusBy;
                    objResource.Comments = lstRARequestData[j].RAExtentionRequest[i].RMGComments;
                    lstresource.Add(objResource);
                }
                j++;
            }
            return lstresource;
        }

        private static List<RAResource> GetReleaseResourceForCA(List<RAGetRaisedRequest> lstRARequestData)
        {
            List<RAResource> lstresource = new List<RAResource>();
            int j = 0;
            while (j < lstRARequestData.Count)
            {
                for (int i = 0; i < lstRARequestData[j].RAReleaseRequest.Count; i++)
                {
                    RAResource objResource = new RAResource();
                    objResource.ProjectID = lstRARequestData[j].ProjectID;
                    objResource.EmpID = lstRARequestData[j].RAReleaseRequest[i].EmpID;
                    objResource.Allocation = lstRARequestData[j].RAReleaseRequest[i].Allocation;
                    objResource.Billability = lstRARequestData[j].RAReleaseRequest[i].Billability;
                    objResource.ProjectReporting = lstRARequestData[j].RAReleaseRequest[i].ProjectReporting;
                    objResource.ProjectRole = lstRARequestData[j].RAReleaseRequest[i].ProjectRole;
                    objResource.StartDate = lstRARequestData[j].RAReleaseRequest[i].StartDate;
                    objResource.EndDate = lstRARequestData[j].RAReleaseRequest[i].EndDate;
                    objResource.RequestedBy = lstRARequestData[j].RequestedBy;
                    objResource.ActionDate = lstRARequestData[j].RAReleaseRequest[i].ActionDate;
                    objResource.RequestID = lstRARequestData[j].ID;
                    objResource.StatusBy = lstRARequestData[j].StatusBy;
                    objResource.Comments = lstRARequestData[j].RAReleaseRequest[i].RMGComments;
                    lstresource.Add(objResource);
                }
                j++;
            }
            return lstresource;
        }
    }
}

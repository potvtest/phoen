using AutoMapper;
using Pheonix.Core.Repository.Utils;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Pheonix.Core.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private PhoenixEntities _phoenixEntity;
        IDeletedRecordsLog deletedLogs;
        private IEmailService _EmailService;
        public ProjectRepository(IEmailService emailService)
        {
            _phoenixEntity = new PhoenixEntities();
            deletedLogs = new DeletedRecordsLog();
            _EmailService = emailService;
        }
        public IEnumerable<ProjectList> GetList(string filters, int personID)
        {

            using (var db = _phoenixEntity)
            {
                var projectList = (from p in db.ProjectList
                                   join c in db.Customer on p.CustomerID equals c.ID //into tempc
                                   join dt in db.DeliveryTeam on p.DeliveryTeam equals dt.ID
                                   join du in db.DeliveryUnit on p.DeliveryUnit equals du.ID
                                   //from cj in tempc.DefaultIfEmpty()
                                   where p.ParentProjId == null
                                   orderby p.ID
                                   select new
                                   {
                                       ID = p.ID,
                                       CustomerName = c.Name ?? "N/A",
                                       CustomerStartDate = c.CreatedOn,
                                       CustomerEndDate = c.ValidTill,
                                       Active = p.Active,
                                       ActualEndDate = p.ActualEndDate,
                                       ActualStartDate = p.ActualStartDate,
                                       Billable = p.Billable,
                                       CreatedBy = p.CreatedBy,
                                       CreatedOn = p.CreatedOn,
                                       CustomerID = p.CustomerID,
                                       DeliveryTeam = p.DeliveryTeam,
                                       DeliveryUnit = p.DeliveryUnit,
                                       DelUnitName = du.Name,
                                       DelTeamName = dt.Name,
                                       Description = p.Description,
                                       IsExternal = p.IsExternal,
                                       IsOffshore = p.IsOffshore,
                                       ProjectCode = p.ProjectCode ?? "N/A",
                                       ProjectManager = p.ProjectManager,
                                       ProjectManagerName = db.People.Where(x => x.ID == p.ProjectManager).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                       DeliveryManager = p.DeliveryManager,
                                       DeliveryManagerName = db.People.Where(x => x.ID == p.DeliveryManager).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                                       ProjectName = p.ProjectName ?? "N/A",
                                       ProjectType = p.ProjectType,
                                       ParentProjId = p.ParentProjId,
                                       ProjectMethodology = p.ProjectMethodology,
                                       Process = p.Process,
                                       SprintDuration = p.SprintDuration


                                   }).AsEnumerable().Select(x => new ProjectList()
                                   {
                                       ID = x.ID,
                                       CustomerName = x.CustomerName ?? "N/A",
                                       CustomerStartDate = x.CustomerStartDate,
                                       CustomerEndDate = x.CustomerEndDate,
                                       Active = x.Active,
                                       ActualEndDate = x.ActualEndDate,
                                       ActualStartDate = x.ActualStartDate,
                                       Billable = x.Billable,
                                       CreatedBy = x.CreatedBy,
                                       CreatedOn = x.CreatedOn,
                                       CustomerID = x.CustomerID,
                                       DeliveryTeam = x.DeliveryTeam,
                                       DeliveryUnit = x.DeliveryUnit,
                                       DelTeamName = x.DelTeamName,
                                       DelUnitName = x.DelUnitName,
                                       Description = x.Description,
                                       IsExternal = x.IsExternal,
                                       IsOffshore = x.IsOffshore,
                                       ProjectCode = x.ProjectCode ?? "N/A",
                                       ProjectManager = x.ProjectManager,
                                       ProjectManagerName = x.ProjectManagerName,
                                       DeliveryManager = x.DeliveryManager,
                                       DeliveryManagerName = x.DeliveryManagerName,
                                       ProjectName = x.ProjectName ?? "N/A",
                                       ProjectType = x.ProjectType,
                                       ParentProjId = x.ParentProjId,
                                       ProjectMethodology = x.ProjectMethodology,
                                       Process = x.Process,
                                       SprintDuration = x.SprintDuration

                                   }).ToList();
               
                var roles = db.PersonInRole.Where(x => x.PersonID == personID).Select(x => x.RoleID).ToList();
                bool role = false;
                if (roles != null)
                {
                    if (
                            roles.Contains(21) || roles.Contains(23) || roles.Contains(33) //finance roles
                            || roles.Contains(24) || roles.Contains(38) //hr roles
                            || roles.Contains(27) || roles.Contains(35) //rmg roles
                            || roles.Contains(28)//pmo roles
                        )
                        role = true;
                }
                List<int> projectID = new List<int>();
                projectID = db.PMSResourceAllocation.Where(x => x.PersonID == personID).Select(x => x.ProjectID).ToList();
                projectID.AddRange(db.ProjectList.Where(x => x.ProjectManager == personID || x.DeliveryManager == personID).Select(x => x.ID).ToList());
                projectID.AddRange(db.PMSConfiguration.Where(x => x.PersonID == personID).Select(x => x.Project.Value).ToList());

                if (role)
                    return projectList;
                else
                    return projectList.Where(x => projectID.Contains(x.ID));

                //TODO: Remove while optimize
                //if (role)
                //{
                //    var projectList = (from p in db.ProjectList
                //                       join c in db.Customer on p.CustomerID equals c.ID into tempc
                //                       join dt in db.DeliveryTeam on p.DeliveryTeam equals dt.ID
                //                       join du in db.DeliveryUnit on p.DeliveryUnit equals du.ID
                //                       from cj in tempc.DefaultIfEmpty()
                //                       where p.ParentProjId == null
                //                       orderby p.ID
                //                       select new
                //                       {
                //                           ID = p.ID,
                //                           CustomerName = cj.Name ?? "N/A",
                //                           Active = p.Active,
                //                           ActualEndDate = p.ActualEndDate,
                //                           ActualStartDate = p.ActualStartDate,
                //                           Billable = p.Billable,
                //                           CreatedBy = p.CreatedBy,
                //                           CreatedOn = p.CreatedOn,
                //                           CustomerID = p.CustomerID,
                //                           DeliveryTeam = p.DeliveryTeam,
                //                           DeliveryUnit = p.DeliveryUnit,
                //                           DelUnitName = du.Name,
                //                           DelTeamName = dt.Name,
                //                           Description = p.Description,
                //                           IsExternal = p.IsExternal,
                //                           IsOffshore = p.IsOffshore,
                //                           ProjectCode = p.ProjectCode ?? "N/A",
                //                           ProjectManager = p.ProjectManager,
                //                           ProjectName = p.ProjectName ?? "N/A",
                //                           ProjectType = p.ProjectType,
                //                           ParentProjId = p.ParentProjId,
                //                           ProjectMethodology = p.ProjectMethodology,
                //                           Process = p.Process,
                //                           SprintDuration = p.SprintDuration


                //                       }).AsEnumerable().Select(x => new ProjectList()
                //                       {
                //                           ID = x.ID,
                //                           CustomerName = x.CustomerName ?? "N/A",
                //                           Active = x.Active,
                //                           ActualEndDate = x.ActualEndDate,
                //                           ActualStartDate = x.ActualStartDate,
                //                           Billable = x.Billable,
                //                           CreatedBy = x.CreatedBy,
                //                           CreatedOn = x.CreatedOn,
                //                           CustomerID = x.CustomerID,
                //                           DeliveryTeam = x.DeliveryTeam,
                //                           DeliveryUnit = x.DeliveryUnit,
                //                           DelTeamName = x.DelTeamName,
                //                           DelUnitName = x.DelUnitName,
                //                           Description = x.Description,
                //                           IsExternal = x.IsExternal,
                //                           IsOffshore = x.IsOffshore,
                //                           ProjectCode = x.ProjectCode ?? "N/A",
                //                           ProjectManager = x.ProjectManager,
                //                           ProjectName = x.ProjectName ?? "N/A",
                //                           ProjectType = x.ProjectType,
                //                           ParentProjId = x.ParentProjId,
                //                           ProjectMethodology = x.ProjectMethodology,
                //                           Process = x.Process,
                //                           SprintDuration = x.SprintDuration

                //                       }).ToList();
                //    return projectList;
                //}
                //else
                //{
                //    var projectID = db.PMSResourceAllocation.Where(x => x.PersonID == personID).Select(x => x.ID).ToList();

                //    var projectList = (from p in db.ProjectList
                //                       join c in db.Customer on p.CustomerID equals c.ID into tempc
                //                       join dt in db.DeliveryTeam on p.DeliveryTeam equals dt.ID
                //                       join du in db.DeliveryUnit on p.DeliveryUnit equals du.ID
                //                       join r in db.PMSResourceAllocation on p.ID equals r.ProjectID 
                //                       join pms in db.PMSConfiguration on p.ID equals pms.Project
                //                       from cj in tempc.DefaultIfEmpty()
                //                       where p.ParentProjId == null 
                //                       && r.PersonID == personID                                 
                //                       orderby p.ID
                //                       select new
                //                       {
                //                           ID = p.ID,
                //                           CustomerName = cj.Name ?? "N/A",
                //                           Active = p.Active,
                //                           ActualEndDate = p.ActualEndDate,
                //                           ActualStartDate = p.ActualStartDate,
                //                           Billable = p.Billable,
                //                           CreatedBy = p.CreatedBy,
                //                           CreatedOn = p.CreatedOn,
                //                           CustomerID = p.CustomerID,
                //                           DeliveryTeam = p.DeliveryTeam,
                //                           DeliveryUnit = p.DeliveryUnit,
                //                           DelUnitName = du.Name,
                //                           DelTeamName = dt.Name,
                //                           Description = p.Description,
                //                           IsExternal = p.IsExternal,
                //                           IsOffshore = p.IsOffshore,
                //                           ProjectCode = p.ProjectCode ?? "N/A",
                //                           ProjectManager = p.ProjectManager,
                //                           ProjectName = p.ProjectName ?? "N/A",
                //                           ProjectType = p.ProjectType,
                //                           ParentProjId = p.ParentProjId,
                //                           ProjectMethodology = p.ProjectMethodology,
                //                           Process = p.Process,
                //                           SprintDuration = p.SprintDuration


                //                       }).AsEnumerable().Select(x => new ProjectList()
                //                       {
                //                           ID = x.ID,
                //                           CustomerName = x.CustomerName ?? "N/A",
                //                           Active = x.Active,
                //                           ActualEndDate = x.ActualEndDate,
                //                           ActualStartDate = x.ActualStartDate,
                //                           Billable = x.Billable,
                //                           CreatedBy = x.CreatedBy,
                //                           CreatedOn = x.CreatedOn,
                //                           CustomerID = x.CustomerID,
                //                           DeliveryTeam = x.DeliveryTeam,
                //                           DeliveryUnit = x.DeliveryUnit,
                //                           DelTeamName = x.DelTeamName,
                //                           DelUnitName = x.DelUnitName,
                //                           Description = x.Description,
                //                           IsExternal = x.IsExternal,
                //                           IsOffshore = x.IsOffshore,
                //                           ProjectCode = x.ProjectCode ?? "N/A",
                //                           ProjectManager = x.ProjectManager,
                //                           ProjectName = x.ProjectName ?? "N/A",
                //                           ProjectType = x.ProjectType,
                //                           ParentProjId = x.ParentProjId,
                //                           ProjectMethodology = x.ProjectMethodology,
                //                           Process = x.Process,
                //                           SprintDuration = x.SprintDuration

                //                       }).ToList();
                //    return projectList;
                //}
                
            }
        }
        public ActionResult Add(ProjectViewModel model, int personid)
        {
            ActionResult result = new ActionResult();
            try
            {
                bool isChildProject = false;
                using (var db = _phoenixEntity)
                {
                    ProjectList dbModel = Mapper.Map<ProjectViewModel, ProjectList>(model);
                    if (db.ProjectList.Any(c => c.ProjectName.ToLower() == model.ProjectName.ToLower() && c.IsDeleted != true))
                    {
                        result.isActionPerformed = false;
                        result.message = string.Format("Project name already exists");
                        return result;
                    }
                    dbModel.ProjectName = model.ProjectName.TrimEnd();
                    db.ProjectList.Add(dbModel);
                    db.SaveChanges();
                    _EmailService.SendProjectMails(ProjectMailAction.Creation, dbModel, personid);
                    if (dbModel.ParentProjId > 0) { isChildProject = true; }
                }
                result.isActionPerformed = true;
                if (!isChildProject)
                    result.message = string.Format("Project added successfully");
                else
                    result.message = string.Format("Sub-Project added successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        public ActionResult Update(ProjectViewModel model, int personid)
        {
            ActionResult result = new ActionResult();
            try
            {
                bool isChildProject = false;
                using (var db = _phoenixEntity)
                {
                    if (db.ProjectList.Any(c => c.ProjectName.ToLower() == model.ProjectName.ToLower() && c.IsDeleted != true && c.ID != model.ID))
                    {
                        result.isActionPerformed = false;
                        result.message = string.Format("Project name already exists");
                        return result;
                    }

                    ProjectList dbModel = db.ProjectList.Where(x => x.ID == model.ID).SingleOrDefault();

                    if (dbModel != null)
                    {
                        model.ProjectName = model.ProjectName.TrimEnd();
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<ProjectViewModel, ProjectList>(model));
                        db.SaveChanges();
                        //_EmailService.SendProjectMails(ProjectMailAction.Update, dbModel, personid);
                        if (dbModel.ParentProjId > 0) { isChildProject = true; }
                    }
                }
                result.isActionPerformed = true;
                if (!isChildProject)
                    result.message = string.Format("Project updated successfully");
                else
                    result.message = string.Format("Sub-Project updated successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        public void Delete(int id, int personid)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var project = db.ProjectList.Where(x => x.ID == id).FirstOrDefault();

                    if (project != null)
                    {
                        project.IsDeleted = true;
                        db.SaveChanges();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
        public ProjectList GetProject(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var projectList = (from p in db.ProjectList
                                       join c in db.Customer on p.CustomerID equals c.ID into tempc
                                       join dt in db.DeliveryTeam on p.DeliveryTeam equals dt.ID
                                       join du in db.DeliveryUnit on p.DeliveryUnit equals du.ID
                                       from cj in tempc.DefaultIfEmpty()
                                       orderby p.ID
                                       select new
                                       {
                                           ID = p.ID,
                                           CustomerName = cj.Name ?? "N/A",                                           
                                           Active = p.Active,
                                           ActualEndDate = p.ActualEndDate,
                                           ActualStartDate = p.ActualStartDate,
                                           Billable = p.Billable,
                                           CreatedBy = p.CreatedBy,
                                           CreatedOn = p.CreatedOn,
                                           CustomerID = p.CustomerID,
                                           DeliveryTeam = p.DeliveryTeam,
                                           DeliveryManager = p.DeliveryManager,
                                           DeliveryUnit = p.DeliveryUnit,
                                           DelUnitName = du.Name,
                                           DelTeamName = dt.Name,
                                           Description = p.Description,
                                           IsExternal = p.IsExternal,
                                           IsOffshore = p.IsOffshore,
                                           ProjectCode = p.ProjectCode ?? "N/A",
                                           ProjectManager = p.ProjectManager,
                                           ProjectName = p.ProjectName ?? "N/A",
                                           ProjectType = p.ProjectType,
                                           ParentProjId = p.ParentProjId,
                                           ProjectMethodology = p.ProjectMethodology,
                                           Process = p.Process,
                                           SprintDuration = p.SprintDuration


                                       }).AsEnumerable().Select(x => new ProjectList()
                                       {
                                           ID = x.ID,
                                           CustomerName = x.CustomerName ?? "N/A",                                           
                                           Active = x.Active,
                                           ActualEndDate = x.ActualEndDate,
                                           ActualStartDate = x.ActualStartDate,
                                           Billable = x.Billable,
                                           CreatedBy = x.CreatedBy,
                                           CreatedOn = x.CreatedOn,
                                           CustomerID = x.CustomerID,
                                           DeliveryTeam = x.DeliveryTeam,
                                           DeliveryManager = x.DeliveryManager,
                                           DeliveryUnit = x.DeliveryUnit,
                                           DelUnitName = x.DelUnitName,
                                           DelTeamName = x.DelTeamName,
                                           Description = x.Description,
                                           IsExternal = x.IsExternal,
                                           IsOffshore = x.IsOffshore,
                                           ProjectCode = x.ProjectCode ?? "N/A",
                                           ProjectManager = x.ProjectManager,
                                           ProjectName = x.ProjectName ?? "N/A",
                                           ProjectType = x.ProjectType,
                                           ParentProjId = x.ParentProjId,
                                           ProjectMethodology = x.ProjectMethodology,
                                           Process = x.Process,
                                           SprintDuration = x.SprintDuration

                                       }).Where(x => x.ID == id).FirstOrDefault();

                    return projectList;
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }

        }
        private bool AddLogs(DeletedRecordsLogDetailViewModel model)
        {
            return deletedLogs.AddLogs(model);
        }
        public IEnumerable<ProjectList> GetSubProjectDetails(int projId)
        {
            using (var db = _phoenixEntity)
            {
                var sublist = (from p in db.ProjectList
                               join c in db.Customer on p.CustomerID equals c.ID into tempc
                               join dt in db.DeliveryTeam on p.DeliveryTeam equals dt.ID
                               join du in db.DeliveryUnit on p.DeliveryUnit equals du.ID
                               from cj in tempc.DefaultIfEmpty()
                               where p.IsDeleted != true && p.ParentProjId == projId
                               orderby p.ID
                               select new
                               {
                                   ID = p.ID,
                                   CustomerName = cj.Name ?? "N/A",                                  
                                   Active = p.Active,
                                   ActualEndDate = p.ActualEndDate,
                                   ActualStartDate = p.ActualStartDate,
                                   Billable = p.Billable,
                                   CreatedBy = p.CreatedBy,
                                   CreatedOn = p.CreatedOn,
                                   CustomerID = p.CustomerID,
                                   DeliveryTeam = p.DeliveryTeam,
                                   DeliveryManager = p.DeliveryManager,
                                   DeliveryUnit = p.DeliveryUnit,
                                   DelUnitName = du.Name,
                                   DelTeamName = dt.Name,
                                   Description = p.Description,
                                   IsExternal = p.IsExternal,
                                   IsOffshore = p.IsOffshore,
                                   ProjectCode = p.ProjectCode ?? "N/A",
                                   ProjectManager = p.ProjectManager,
                                   ProjectName = p.ProjectName ?? "N/A",
                                   ProjectType = p.ProjectType,
                                   ParentProjId = p.ParentProjId,
                                   ProjectMethodology = p.ProjectMethodology,
                                   Process = p.Process,
                                   SprintDuration = p.SprintDuration

                               }).AsEnumerable().Select(x => new ProjectList()
                               {
                                   ID = x.ID,
                                   CustomerName = x.CustomerName ?? "N/A",                                  
                                   Active = x.Active,
                                   ActualEndDate = x.ActualEndDate,
                                   ActualStartDate = x.ActualStartDate,
                                   Billable = x.Billable,
                                   CreatedBy = x.CreatedBy,
                                   CreatedOn = x.CreatedOn,
                                   CustomerID = x.CustomerID,
                                   DeliveryTeam = x.DeliveryTeam,
                                   DeliveryManager = x.DeliveryManager,
                                   DelUnitName = x.DelUnitName,
                                   DelTeamName = x.DelTeamName,
                                   DeliveryUnit = x.DeliveryUnit,
                                   Description = x.Description,
                                   IsExternal = x.IsExternal,
                                   IsOffshore = x.IsOffshore,
                                   ProjectCode = x.ProjectCode ?? "N/A",
                                   ProjectManager = x.ProjectManager,
                                   ProjectName = x.ProjectName ?? "N/A",
                                   ProjectType = x.ProjectType,
                                   ParentProjId = x.ParentProjId,
                                   ProjectMethodology = x.ProjectMethodology,
                                   Process = x.Process,
                                   SprintDuration = x.SprintDuration

                               }).ToList();
                return sublist;
            }


        }
        public bool CheckIfSubProjectPresent(int id)
        {
            bool result = false;
            using (var db = new PhoenixEntities())
            {
                var sublist = (from p in db.ProjectList where p.IsDeleted != true && p.ParentProjId == id select p).ToList();

                if (sublist.Count > 0)
                    result = true;
                else
                    result = false;
            }
            return result;
        }

        // PMSConfigurations
        public ActionResult Add(PMSConfigurationViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    PMSConfiguration dbModel = Mapper.Map<PMSConfigurationViewModel, PMSConfiguration>(model);
                    dbModel.IsDeleted = false;
                    db.PMSConfiguration.Add(dbModel);
                    db.SaveChanges();
                }
                result.isActionPerformed = true;
                result.message = string.Format("Project Configuration Role added successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        public ActionResult Update(PMSConfigurationViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    PMSConfiguration dbModel = db.PMSConfiguration.Where(x => x.Id == model.Id).SingleOrDefault();

                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PMSConfigurationViewModel, PMSConfiguration>(model));
                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Project Configuration Role updated successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        public IEnumerable<object> GetList(int id)
        {
            using (var db = _phoenixEntity)
            {
                var pmsConfig = (from p in db.PMSConfiguration
                                 join c in db.PMSRoles on p.Role equals c.PMSRoleID
                                 join r in db.People on p.PersonID equals r.ID
                                 where p.Project == id && p.IsDeleted != true
                                 orderby p.Id
                                 select new
                                 {
                                     Id = p.Id,
                                     Role = c.PMSRoleDescription ?? "N/A",
                                     Resource = r.FirstName + " " + r.LastName,
                                     Project = p.Project,
                                     roleId = p.Role,
                                     personId = p.PersonID

                                 }).AsEnumerable().ToList();
                return pmsConfig;
            }
        }
        public void Delete(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var pmsConfig = db.PMSConfiguration.Where(x => x.Id == id).FirstOrDefault();

                    if (pmsConfig != null)
                    {
                        pmsConfig.IsDeleted = true;
                        db.SaveChanges();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public ActionResult Add(ProjectSkillViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    ProjectSkill dbModel = Mapper.Map<ProjectSkillViewModel, ProjectSkill>(model);
                    db.ProjectSkill.Add(dbModel);
                    db.SaveChanges();
                }
                result.isActionPerformed = true;
                result.message = string.Format("Project Skill added successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        public ActionResult Update(ProjectSkillViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    ProjectSkill dbModel = db.ProjectSkill.Where(x => x.ID == model.ID).SingleOrDefault();

                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<ProjectSkillViewModel, ProjectSkill>(model));
                        db.SaveChanges();
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Project Skill updated successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        public IEnumerable<object> GetSkillList(int id)
        {
            using (var db = _phoenixEntity)
            {
                var skill = (from p in db.ProjectSkill
                             join c in db.SkillMatrices on p.SkillID equals c.ID
                             where p.ProjectID == id && p.IsDeleted != true
                             orderby p.ID
                             select new
                             {
                                 Id = p.ID,
                                 Skill = c.Name ?? "N/A",
                                 Project = p.ProjectID,
                                 skillId = p.SkillID
                             }).AsEnumerable().ToList();
                return skill;
            }
        }
        public void DeleteSkill(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var skill = db.ProjectSkill.Where(x => x.ID == id).FirstOrDefault();

                    if (skill != null)
                    {
                        //skill.IsDeleted = true;
                        db.ProjectSkill.Remove(skill);
                        db.SaveChanges();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
        // PMSConfigurations ends
    }
}

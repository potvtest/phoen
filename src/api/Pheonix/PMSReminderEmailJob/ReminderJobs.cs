using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using log4net;
using System.Reflection;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.Core.Helpers;
using System.Configuration;
using System.Data.Entity;
using Pheonix.Models.VM.Classes.ResourceAllocation;
using System.IO;
using Pheonix.Core.v1.Services.Email;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace PMSReminderEmailJob
{
    public class ReminderEmailSender
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Job runjob = new Job();
            runjob.CustomerContractEndDateReminderJob();
            runjob.ProjectEndDateReminderJob();
            runjob.RAEndDateReminderJob();
            runjob.EmployeeContractEndDateReminderJob();
            runjob.BGCEndDateReminderJob();
            runjob.RAPercentageDetailsJob();
        }
        public class Job
        {
            IUnityContainer _container = UnityRegister.LoadContainer();
            EmailTemplate EmailTemlpates = new EmailTemplate();
            StringBuilder stringBuilder = new StringBuilder();

            public void CustomerContractEndDateReminderJob()
            {
                try
                {
                    var basicservice = _container.Resolve<BasicOperationsService>();

                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        Log4Net.Debug("Customer EndDate Reminder Job has started: " + DateTime.Now);

                        var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault()?.EmailGroup;
                        var AMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "FI").FirstOrDefault()?.EmailGroup;
                        var PMO = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "PMO").FirstOrDefault()?.EmailGroup;

                        var customers = dbcontext.CustomerContract
                            .Where(x =>
                                DbFunctions.TruncateTime(x.DateValidTill) == DbFunctions.AddDays(DateTime.Today, 30) ||
                                DbFunctions.TruncateTime(x.DateValidTill) == DbFunctions.AddDays(DateTime.Today, 60) ||
                                DbFunctions.TruncateTime(x.DateValidTill) == DbFunctions.AddDays(DateTime.Today, 90) ||
                                DbFunctions.TruncateTime(x.DateValidTill) == DbFunctions.TruncateTime(DateTime.Today))
                            .ToList();

                        foreach (var cust in customers)
                        {
                            var emailTemplate = dbcontext.EmailTemplate.FirstOrDefault(x => x.TemplateFor == "CustomerContractEndDatereminder");
                            string template = emailTemplate.Html;
                            string subject = emailTemplate.Subjects;

                            var deliveryManagers = dbcontext.ProjectList.Where(x => x.CustomerID == cust.CustomerID).Select(x => x.DeliveryManager).Distinct().ToList();                       

                            var projectManagers = dbcontext.ProjectList.Where(x => x.CustomerID == cust.CustomerID).Select(x => x.ProjectManager).Distinct().ToList();

                            var activeManagerEmails = (from employment in dbcontext.PersonEmployment
                                                       join person in dbcontext.People on employment.PersonID equals person.ID
                                                       where (deliveryManagers.Contains(employment.PersonID) || projectManagers.Contains(employment.PersonID))
                                                       && employment.EmploymentStatus == 1 && person.Active
                                                       select employment.OrganizationEmail).Distinct().ToList();

                            if (!activeManagerEmails.Any())
                            {
                                Log4Net.Warn($"Skipping email for customer {cust.CustomerID} because no active recipients found.");
                                continue;
                            }

                            string RptMgrEmails = string.Join(",", activeManagerEmails);

                            template = template.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"));
                            template = template.Replace("{{contractname}}", cust.ContractName);
                            template = template.Replace("{{customername}}", cust.Customer.Name);
                            template = template.Replace("{{contractenddate}}", cust.DateValidTill?.ToString("yyyy-MM-dd"));

                            try
                            {
                                bool isCreated = basicservice.Create(new Emails
                                {
                                    Content = template,
                                    Date = DateTime.Now,
                                    EmailFrom = ConfigurationManager.AppSettings["helpdeskEmailId"],
                                    EmailTo = RptMgrEmails,
                                    Subject = subject,
                                    EmailCC = string.Join(",", new[] { RMG, PMO, AMG }.Where(email => !string.IsNullOrEmpty(email)))
                                }, e => e.Id == 0);

                                if (isCreated)
                                {
                                    basicservice.Finalize(true);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log4Net.Error("Exception in CustomerContractEndDateReminderJob, Exception Message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception in Main method CustomerContractEndDateReminderJob, Exception Message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                }
                Log4Net.Debug("Customer EndDate Reminder Job has stopped: " + DateTime.Now);
            }

            public void ProjectEndDateReminderJob()
            {
                try
                {
                    var basicservice = _container.Resolve<BasicOperationsService>();

                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        Log4Net.Debug("Reminder Job has started: = " + DateTime.Now);
                        List<int?> DMList = new List<int?>();
                        List<int?> Mgrids = new List<int?>();
                        List<int?> DeliverMgrIds = new List<int?>();

                        var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault().EmailGroup;
                        var AMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "FI").FirstOrDefault().EmailGroup;
                        var PMO = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "PMO").FirstOrDefault().EmailGroup;

                        var projects = dbcontext.ProjectList.Where(x =>
                        (DbFunctions.TruncateTime(x.ActualEndDate) == DbFunctions.AddDays(DateTime.Today, 15)
                        || DbFunctions.TruncateTime(x.ActualEndDate) == DbFunctions.AddDays(DateTime.Today, 30)
                        || DbFunctions.TruncateTime(x.ActualEndDate) == DateTime.Today)).ToList();

                        // --- consider this table PMSResourceAllocation
                        foreach (var proj in projects)
                        {
                            DMList.Clear();

                            EmailTemlpates = dbcontext.EmailTemplate.Where(x => x.TemplateFor == "ProjectEndDatereminder").FirstOrDefault();
                            string template = EmailTemlpates.Html;
                            var subject = EmailTemlpates.Subjects;

                            var DMIds = dbcontext.ProjectList.Where(x => x.ID == proj.ID).Select(x => x.DeliveryManager).ToList();
                            DMList.AddRange(DMIds);

                            var PMids = dbcontext.ProjectList.Where(x => x.ID == proj.ID).Select(X => X.ProjectManager).ToList();
                            DMList.AddRange(PMids);

                            var RptMgr = dbcontext.PersonEmployment.Where(x => DMList.Contains(x.PersonID)).Select(x => x.OrganizationEmail).Distinct().ToList();
                            var RptMgrs = string.Join(" , ", RptMgr.ToArray());

                            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                            template = template.Replace("{{projectenddate}}", proj.ActualEndDate.ToStandardDate());
                            template = template.Replace("{{projectname}}", proj.ProjectName);

                            bool isCreated;
                            try
                            {
                                isCreated = basicservice.Create<Emails>(new Emails
                                {
                                    Content = template,
                                    Date = DateTime.Now,
                                    EmailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]),
                                    EmailTo = RptMgrs,
                                    Subject = subject,
                                    EmailCC = RMG + "," + PMO + "," + AMG,
                                }, e => e.Id == 0);

                                if (isCreated)
                                    basicservice.Finalize(true);
                            }
                            catch (Exception ex)
                            {
                                Log4Net.Error("Exception in ProjectEndDateReminderJob, Exception Message: " + ex.StackTrace);
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Log4Net.Error("Exception in Main method ProjectEndDateReminderJob, Exception Message: " + ex.StackTrace);
                }
                Log4Net.Debug("Project End Date Reminder Job has stopped: = " + DateTime.Now);
            }

            public void RAEndDateReminderJob()
            {
                try
                {
                    var basicservice = _container.Resolve<BasicOperationsService>();

                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        Log4Net.Debug("Reminder Job has started: =" + DateTime.Now);
                        EmailTemlpates = dbcontext.EmailTemplate.Where(x => x.TemplateFor == "RAEndDateReminder").FirstOrDefault();
                        if (EmailTemlpates != null)
                        {
                            string template = EmailTemlpates.Html;
                            var subject = EmailTemlpates.Subjects;
                            var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault().EmailGroup;

                            //var Resources = dbcontext.PMSResourceAllocation
                            //    .Where(x => DbFunctions.TruncateTime(x.ToDate) < DateTime.Today && x.ReleaseDate == null).GroupBy(x => x.ReportingTo).ToList();

                            var Resources = (from PMSResourceAllocation in dbcontext.PMSResourceAllocation
                                             join person in dbcontext.People on PMSResourceAllocation.PersonID equals person.ID
                                             where (DbFunctions.TruncateTime(PMSResourceAllocation.ToDate) < DateTime.Today
                                                    && PMSResourceAllocation.ReleaseDate == null
                                                    && person.Active == true)
                                             select PMSResourceAllocation).GroupBy(x => x.ReportingTo).ToList();

                            if (Resources != null)
                            {
                                for (int i = 0; i < Resources.Count; i++)  /// for each proj id
                                {
                                    var resGrp1 = Resources[i];
                                    var resGrp = resGrp1.First<PMSResourceAllocation>();
                                    string PrjMgr = string.Empty;
                                    string RptMgr = string.Empty;
                                    string DM = string.Empty;

                                    string fileName = string.Concat("RAEndDateReminder", "_", "_", resGrp.ReportingTo.ToString(), "_", "_", DateTime.Now.ToString("yyyyMMdd"), ".csv");
                                    string filePath = GetFolderPath(fileName);

                                    PrjMgr = dbcontext.ProjectList.Where(x => x.ID == resGrp.ProjectID).Select(x => x.ProjectManager).FirstOrDefault().ToString();
                                    var PrjMgrEmail = dbcontext.PersonEmployment.Where(x => x.PersonID.ToString() == PrjMgr).Select(x => x.OrganizationEmail).FirstOrDefault();
                                    DM = dbcontext.ProjectList.Where(x => x.ID == resGrp.ProjectID).Select(x => x.DeliveryManager).FirstOrDefault().ToString();
                                    var DMEmail = dbcontext.PersonEmployment.Where(x => x.PersonID.ToString() == DM).Select(x => x.OrganizationEmail).FirstOrDefault();
                                    RptMgr = dbcontext.PMSResourceAllocation.Where(x => x.ReportingTo == resGrp.ReportingTo).Select(x => x.ReportingTo).FirstOrDefault().ToString();
                                    var RptMgrEmail = dbcontext.PersonEmployment.Where(x => x.PersonID.ToString() == RptMgr).Select(x => x.OrganizationEmail).FirstOrDefault();


                                    if (filePath.IndexOf("_new") > -1)
                                    {
                                        filePath = filePath.Replace("_new", "");
                                        stringBuilder.AppendLine(BindHeadersForCsv());
                                    }

                                    foreach (var res in resGrp1)   /// for each resurce in projids
                                    {
                                        var flag = GetResourceAllocationEndDateReminderEmailAttachments(res, stringBuilder, filePath);
                                    }
                                    bool isCreated;
                                    try
                                    {
                                        template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                                        isCreated = basicservice.Create<Emails>(new Emails
                                        {
                                            Content = template,
                                            Date = DateTime.Now,
                                            EmailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]),
                                            EmailTo = PrjMgrEmail + "," + RptMgrEmail /*+ "," + DMEmail*/,
                                            Subject = subject,
                                            EmailCC = RMG,
                                            Attachments = filePath
                                        }, e => e.Id == 0);

                                        if (isCreated)
                                            basicservice.Finalize(true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log4Net.Error("Exception in RAEndDateReminderJob, Exception Message: " + ex.StackTrace);
                                    }

                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Log4Net.Error("Exception in Main method RAEndDateReminderJob, Exception Message: " + ex.StackTrace);
                }
                Log4Net.Debug("Resource Allocation End Date Reminder Job has stopped: = " + DateTime.Now);
            }

            public void EmployeeContractEndDateReminderJob()
            {
                try
                {
                    var basicservice = _container.Resolve<BasicOperationsService>();

                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        Log4Net.Debug("Contract End Reminder Job has started: = " + DateTime.Now);

                        var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault().EmailGroup;
                        var HR = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "HR").FirstOrDefault().EmailGroup;

                        var contractNeartoEnd = dbcontext.PersonEmployment.Where(x => (x.EmployeeType.ToLower() == "contract" && x.ExitDate == null &&
                        (DbFunctions.TruncateTime(x.ProbationReviewDate) == DbFunctions.AddDays(DateTime.Today, 15) ||
                        DbFunctions.TruncateTime(x.ProbationReviewDate) == DbFunctions.AddDays(DateTime.Today, 10) ||
                        DbFunctions.TruncateTime(x.ProbationReviewDate) == DbFunctions.AddDays(DateTime.Today, 5) ||
                        DbFunctions.TruncateTime(x.ProbationReviewDate) == DateTime.Today))).ToList();

                        // --- consider this table PMSResourceAllocation
                        foreach (var proj in contractNeartoEnd)
                        {
                            EmailTemlpates = dbcontext.EmailTemplate.Where(x => x.TemplateFor == "ContractEndNotify").FirstOrDefault();
                            string template = EmailTemlpates.Html;
                            var subject = EmailTemlpates.Subjects;

                            int? EPM = dbcontext.PersonEmployment.Where(x => x.PersonID == proj.PersonID).Select(x => x.ExitProcessManager).FirstOrDefault();
                            int? RM = dbcontext.PersonReporting.Where(x => x.PersonID == proj.PersonID).Select(x => x.ReportingTo).FirstOrDefault();

                            var EPMids = dbcontext.PersonEmployment.Where(x => x.PersonID == EPM).Select(X => X.OrganizationEmail).FirstOrDefault().ToString();
                            var RMids = dbcontext.PersonEmployment.Where(x => x.PersonID == RM).Select(X => X.OrganizationEmail).FirstOrDefault().ToString();

                            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                            template = template.Replace("{{username}}", proj.Person.FirstName + " " + proj.Person.LastName);
                            template = template.Replace("{{employeeid}}", Convert.ToString(proj.PersonID));
                            template = template.Replace("{{contractenddate}}", Convert.ToString(proj.ProbationReviewDate));

                            bool isCreated;
                            try
                            {
                                isCreated = basicservice.Create<Emails>(new Emails
                                {
                                    Content = template,
                                    Date = DateTime.Now,
                                    EmailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]),
                                    EmailTo = EPMids + "," + RMids,
                                    Subject = subject,
                                    EmailCC = RMG + "," + HR,
                                }, e => e.Id == 0);

                                if (isCreated)
                                    basicservice.Finalize(true);
                            }
                            catch (Exception ex)
                            {
                                Log4Net.Error("Exception in EmployeeContractEndDateReminderJob, Exception Message: " + ex.StackTrace);
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Log4Net.Error("Exception in Main method EmployeeContractEndDateReminderJob, Exception Message: " + ex.StackTrace);
                }
                Log4Net.Debug("Contract End Reminder Job has stopped: = " + DateTime.Now);
            }

            private bool GetResourceAllocationEndDateReminderEmailAttachments(PMSResourceAllocation resource, StringBuilder stringbuilder, string filePath)
            {
                bool isCsvCreated = false;
                try
                {
                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        var emp = dbcontext.PersonPersonals.Where(x => x.Person.ID == resource.PersonID).Select(x => x.Person).FirstOrDefault();
                        var reportingMgrid = dbcontext.PMSResourceAllocation.Where(c => c.PersonID == resource.PersonID).Select(c => c.ReportingTo).FirstOrDefault();
                        var reportingMgr = dbcontext.PersonPersonals.Where(x => x.Person.ID == reportingMgrid).Select(x => x.Person).FirstOrDefault();
                        StringBuilder stringBuilder1 = new StringBuilder();

                        stringBuilder1 = stringBuilder;

                        stringBuilder1.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        resource.ProjectID,
                        GetProjectName(resource.ProjectID),
                        resource.PersonID,
                        emp.FirstName + " " + emp.LastName,
                        resource.FromDate.ToShortDateString(),
                        resource.ToDate.ToShortDateString(),
                        GetProjectRole(resource.ProjectRole),
                        resource.percentage + "%",
                        resource.ReportingTo,
                        reportingMgr.FirstName + " " + reportingMgr.LastName,
                        GetProjectBillability(resource.BillbleType)
                    ));

                        System.IO.File.AppendAllText(filePath, stringBuilder1.ToString());
                        isCsvCreated = true;
                        stringBuilder = stringBuilder.Clear();
                    }
                }
                catch (Exception ex)
                {
                    isCsvCreated = false;
                    Log4Net.Error("Exception in GetResourceAllocationEndDateReminderEmailAttachments Method Exception Message: " + ex.StackTrace);                   
                }
                return isCsvCreated;

            }

            private string BindHeadersForCsv()
            {
                return ("ProjctID,Project Name, Resource ID, Resource Name,Start Date, End Date, Project Role, Percentage, Reporting ManagerId, Reporting Manager, Billability");
            }

            string GetFolderPath(string filePath)
            {
                try
                {
                    string path = Convert.ToString(ConfigurationManager.AppSettings["ReminderEmailJobPath"]);
                    path = string.Concat(path, @"\ResourceAllocationEndDate\");

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    if (!System.IO.File.Exists(path + filePath))
                    {
                        System.IO.File.Create(path + filePath).Close();
                        return path + filePath + "_new";
                    }

                    return path + filePath;
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception in GetFolderPath" + ex.StackTrace);
                    return string.Empty;
                }
            }

            private string GetProjectBillability(int billability)
            {
                string projectRole = string.Empty;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    projectRole = dbContext.PMSAllocationBillableType.Where(x => x.ID == billability).Select(x => x.Discription).FirstOrDefault();
                }
                return projectRole;
            }

            private string GetProjectName(int projectId)
            {
                string projectName = string.Empty;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    projectName = dbContext.ProjectList.Where(x => x.ID == projectId).Select(x => x.ProjectName).FirstOrDefault();
                }
                return projectName;
            }

            private string GetProjectRole(int roleId)
            {
                string projectRole = string.Empty;
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    projectRole = dbContext.PMSRoles.Where(x => x.PMSRoleID == roleId).Select(x => x.PMSRoleDescription).FirstOrDefault();
                }
                return projectRole;
            }

            public void BGCEndDateReminderJob()
            {
                try
                {
                    var basicservice = _container.Resolve<BasicOperationsService>();
                    var emailService = _container.Resolve<EmailSendingService>();

                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        Log4Net.Debug("BGC Reminder Job has started: = " + DateTime.Now);
                        List<int?> DMList = new List<int?>();
                        List<int?> Mgrids = new List<int?>();
                        List<int?> DeliverMgrIds = new List<int?>();

                        var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault().EmailGroup;
                        var HRG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "HR").FirstOrDefault().EmailGroup;

                        var resourceList = dbcontext.PMSResourceAllocation.Where(x =>
                        (DbFunctions.TruncateTime(x.ToDate) == DbFunctions.AddDays(DateTime.Today, -15)) && x.BGStatus == 1).ToList();

                        // Extend resource by 15 days
                        foreach (var reso in resourceList)
                        {
                            reso.ToDate = DbFunctions.AddDays(reso.ToDate, 15).Value;
                            reso.ReleaseDate = null;
                            reso.ModifyBy = reso.ModifyBy;
                            reso.ModifyDate = DateTime.Now;
                            dbcontext.Entry(reso).State = EntityState.Modified;

                            // Send extention mail to resource
                            RAResource model = new RAResource();
                            model.EmpID = reso.PersonID;
                            model.ProjectID = reso.ProjectID;
                            model.ProjectReporting = reso.ReportingTo;
                            model.ProjectRole = reso.ProjectRole;
                            model.StartDate = reso.FromDate;
                            model.EndDate = DbFunctions.AddDays(reso.ToDate, 15).Value;
                            model.Billability = reso.BillbleType;
                            model.Allocation = reso.percentage;
                            model.StatusBy = reso.ModifyBy;
                            model.Comments = "Allocation Extended";


                            emailService.EmployeeUpdateEmail(model);

                            DMList.Clear();

                            EmailTemlpates = dbcontext.EmailTemplate.Where(x => x.TemplateFor == "BGCEndDatereminder").FirstOrDefault();
                            string template = EmailTemlpates.Html;
                            var subject = EmailTemlpates.Subjects;

                            var DMIds = dbcontext.ProjectList.Where(x => x.ID == reso.ID).Select(x => x.DeliveryManager).ToList();
                            DMList.AddRange(DMIds);

                            var PMids = dbcontext.ProjectList.Where(x => x.ID == reso.ID).Select(X => X.ProjectManager).ToList();
                            DMList.AddRange(PMids);

                            var RptMgr = dbcontext.PersonEmployment.Where(x => DMList.Contains(x.PersonID)).Select(x => x.OrganizationEmail).Distinct().ToList();
                            var RptMgrs = string.Join(" , ", RptMgr.ToArray());

                            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                            // template = template.Replace("{{projectenddate}}", reso.ActualEndDate.ToStandardDate());
                            // template = template.Replace("{{projectname}}", reso.ProjectName);

                            bool isCreated;
                            try
                            {
                                isCreated = basicservice.Create<Emails>(new Emails
                                {
                                    Content = template,
                                    Date = DateTime.Now,
                                    EmailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]),
                                    EmailTo = RptMgrs,
                                    Subject = subject,
                                    EmailCC = RMG + "," + HRG
                                }, e => e.Id == 0);

                                if (isCreated)
                                    basicservice.Finalize(true);
                            }
                            catch (Exception ex)
                            {
                                Log4Net.Error("Exception in BGCEndDateReminderJob Method Exception Message: " + ex.StackTrace);                              
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Log4Net.Error("Exception in BGCEndDateReminderJob Method Exception Message: " + ex.StackTrace);
                }
                Log4Net.Debug("BGC Reminder Job has stopped: = " + DateTime.Now);
            }

            public void RAPercentageDetailsJob()
            {
                try
                {
                    var basicservice = _container.Resolve<BasicOperationsService>();

                    using (PhoenixEntities dbcontext = new PhoenixEntities())
                    {
                        Log4Net.Debug("Reminder Job has started: =" + DateTime.Now);
                        var raPercentageSheet1 = dbcontext.rpt_RAPercentage_sheet1().ToList();
                        var raPercentageSheet2 = dbcontext.rpt_RAPercentage_sheet2().ToList();
                        EmailTemlpates = dbcontext.EmailTemplate.Where(x => x.TemplateFor == "RAPercentageDetails").FirstOrDefault();
                        if (EmailTemlpates != null)
                        {
                            string template = EmailTemlpates.Html;
                            var subject = EmailTemlpates.Subjects;
                            var RMG = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "RMG").FirstOrDefault().EmailGroup;
                            var HR = dbcontext.HelpDeskCategories.Where(x => x.Prefix == "HR").FirstOrDefault().EmailGroup;

                            if (raPercentageSheet1 != null & raPercentageSheet2 != null)
                            {
                                string fileName = string.Concat("RAPercentageDetails", "_", "_", DateTime.Now.ToString("yyyyMMdd"), ".xlsx");
                                string filePath = GetPath() + fileName;
                                var flag = GenerateAttachmentFileAsExcel(raPercentageSheet1, raPercentageSheet2, filePath);
                                bool isCreated;
                                ;
                                try
                                {
                                    template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                                    isCreated = basicservice.Create<Emails>(new Emails
                                    {
                                        Content = template,
                                        Date = DateTime.Now,
                                        EmailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]),
                                        EmailTo = RMG,
                                        Subject = subject,
                                        EmailCC = HR,
                                        Attachments = filePath,
                                    }, e => e.Id == 0);

                                    if (isCreated)
                                        basicservice.Finalize(true);
                                }
                                catch (Exception ex)
                                {
                                    Log4Net.Error("Exception in Main method RAPercentageDetailsJob, Exception Message: " + ex.StackTrace);
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Log4Net.Error("Exception in Main method RAPercentageDetailsJob, Exception Message: " + ex.StackTrace);
                }
                Log4Net.Debug("Resource Allocation Percentage Job has stopped: =" + DateTime.Now);
            }

            static string GetPath()
            {
                try
                {
                    string path = Convert.ToString(ConfigurationManager.AppSettings["ReminderEmailJobPath"]);
                    path = string.Concat(path, @"\ResourceAllocationPercentage\");

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    return path;
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception Message: " + ex.StackTrace);
                    return string.Empty;
                }
            }

            private static string GenerateAttachmentFileAsExcel(List<rpt_RAPercentage_sheet1_Result> raPercentageSheet1, List<rpt_RAPercentage_sheet2_Result> raPercentageSheet2, string filePath)
            {
                try
                {
                    using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                    {
                        WorkbookPart workbookPart = document.AddWorkbookPart();
                        workbookPart.Workbook = new Workbook();

                        Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                        // Excel sheet 1
                        WorksheetPart worksheetPart1 = workbookPart.AddNewPart<WorksheetPart>();
                        Worksheet workSheet1 = new Worksheet();
                        SheetData sheetData1 = new SheetData();

                        Sheet sheet1 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart1),
                            SheetId = 1,
                            Name = "RAPercentageSheet1"
                        };
                        sheets.Append(sheet1);

                        string[] dataColumns = GetDataColumnNamesSheet1();
                        worksheetPart1.Worksheet = new Worksheet(sheetData1);
                        worksheetPart1 = AddColumnsToSheet(worksheetPart1, dataColumns);
                        Row headerRow = GenerateHeaderRow(dataColumns);
                        sheetData1.AppendChild(headerRow);
                        sheetData1 = GenerateDataRowsForSheet1(sheetData1, raPercentageSheet1);

                        // Excel sheet 2
                        WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                        Worksheet workSheet2 = new Worksheet();
                        SheetData sheetData2 = new SheetData();

                        Sheet sheet2 = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                            SheetId = 2,
                            Name = "RAPercentageSheet2"
                        };
                        sheets.Append(sheet2);

                        string[] dataColumns2 = GetDataColumnNamesSheet2();
                        worksheetPart2.Worksheet = new Worksheet(sheetData2);
                        worksheetPart2 = AddColumnsToSheet(worksheetPart2, dataColumns2);
                        Row headerRow2 = GenerateHeaderRow(dataColumns2);
                        sheetData2.AppendChild(headerRow2);
                        sheetData2 = GenerateDataRowsForSheet2(sheetData2, raPercentageSheet2);

                        workbookPart.Workbook.Save();
                        document.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception in GenerateAttachmentFileAsExcel Method Exception Message: " + ex.StackTrace);
                    throw;
                }
                return "success";
            }

            private static SheetData GenerateDataRowsForSheet1(SheetData sheetData, List<rpt_RAPercentage_sheet1_Result> model)
            {
                try
                {
                    foreach (var item in model)
                    {
                        Row newRow = new Row();

                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.EmployeeID)) ? string.Empty : Convert.ToString(item.EmployeeID)));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.C_Consumed)) ? string.Empty : Convert.ToString(item.C_Consumed)));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.C_Availabile)) ? string.Empty : Convert.ToString(item.C_Availabile)));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.FirstName) ? string.Empty : item.FirstName));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.LastName) ? string.Empty : item.LastName));

                        sheetData.AppendChild(newRow);
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception in the method GenerateDataRows Exception Message: " + ex.StackTrace);
                    throw;
                }

                return sheetData;
            }

            private static SheetData GenerateDataRowsForSheet2(SheetData sheetData, List<rpt_RAPercentage_sheet2_Result> model)
            {
                try
                {
                    foreach (var item in model)
                    {
                        Row newRow = new Row();

                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.EmployeeID)) ? string.Empty : Convert.ToString(item.EmployeeID)));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.FirstName) ? string.Empty : item.FirstName));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.LastName) ? string.Empty : item.LastName));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ProjectID)) ? string.Empty : Convert.ToString(item.ProjectID)));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.ProjectName) ? string.Empty : item.ProjectName));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.C_Allocated)) ? string.Empty : Convert.ToString(item.C_Allocated)));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.FromDate) ? string.Empty : item.FromDate));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.ToDate) ? string.Empty : item.ToDate));
                        newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.BillbleType)) ? string.Empty : Convert.ToString(item.BillbleType)));

                        sheetData.AppendChild(newRow);
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Exception in the method GenerateDataRows Exception Message: " + ex.StackTrace);
                    throw;
                }

                return sheetData;
            }

            private static Cell GetGeneratedCell(string cellValue)
            {
                Cell objCell = new Cell();
                objCell.DataType = CellValues.String;
                objCell.CellValue = new CellValue(cellValue);
                return objCell;
            }

            private static Row GenerateHeaderRow(string[] dataColumns)
            {
                Row headerRow = new Row();
                //adding column
                List<String> columns = new List<string>();

                for (int n = 0; n < dataColumns.Length; n++)
                {
                    columns.Add(dataColumns[n]);
                    Cell cell = new Cell();
                    cell.DataType = CellValues.InlineString;

                    //execute run for bold
                    Run run1 = new Run();
                    run1.Append(new Text(dataColumns[n]));
                    RunProperties run1Properties = new RunProperties();
                    run1Properties.Append(new Bold());
                    run1.RunProperties = run1Properties;
                    //complete run
                    //create a new inline string and append it
                    InlineString instr = new InlineString();
                    instr.Append(run1);
                    cell.Append(instr);
                    headerRow.AppendChild(cell);
                }

                return headerRow;
            }

            private static WorksheetPart AddColumnsToSheet(WorksheetPart worksheetPart, string[] dataColumns)
            {
                Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
                bool needToInsertColumns = false;

                if (lstColumns == null)
                {
                    lstColumns = new Columns();
                    needToInsertColumns = true;
                }

                for (UInt32Value i = 1; i <= dataColumns.Length; i++)
                    lstColumns.Append(new Column() { Min = i, Max = i, Width = 9, BestFit = true, CustomWidth = true });

                if (needToInsertColumns)
                    worksheetPart.Worksheet.InsertAt(lstColumns, 0);

                return worksheetPart;
            }

            private static string[] GetDataColumnNamesSheet1()
            {
                string[] objColumnNames = new string[] {
                                                       "Employee ID",
                                                       "% Consumed",
                                                       "% Available",
                                                       "First Name",
                                                       "Last Name"
            };
                return objColumnNames;
            }

            private static string[] GetDataColumnNamesSheet2()
            {
                string[] objColumnNames = new string[] {
                                                       "Employee ID",
                                                       "First Name",
                                                       "Last Name",
                                                       "ProjectID",
                                                       "ProjectName",
                                                       "% Allocated",
                                                       "FromDate",
                                                       "ToDate",
                                                       "BillbleType"
            };
                return objColumnNames;
            }
        }
    }
}


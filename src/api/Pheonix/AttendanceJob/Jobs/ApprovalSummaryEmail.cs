using log4net;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace AttendanceJob.Jobs
{
    public class ApprovalSummaryEmail : IJob
    {
        private ApprovalService approvalService = null;
        public IBasicOperationsService _service;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ApprovalSummaryEmail(IBasicOperationsService service)
        {
            _service = service;
        }

        //List<Module> lstModules = new List<Module> {
        //    new Module { id=1, name = "leaves", shortName = "L" },
        //    new Module { id=2, name = "profile", shortName = "P" },
        //    new Module { id=3, name = "expense", shortName = "E" },
        //    new Module { id=4, name = "compensatoryOff", shortName = "CO" },
        //    new Module { id=5, name = "helpdesk", shortName = "HD" },
        //    new Module { id=6, name = "travel", shortName = "T" },
        //    new Module { id=7, name = "appraisals", shortName = "A" },
        //    new Module { id=8, name = "onetoone", shortName = "OO" },
        //    new Module { id=9, name = "confirmations", shortName = "C" }            
        //};
        List<Module> lstModules = new List<Module> {
            new Module { id=1, name = "Leaves", shortName = "L" },
            new Module { id=2, name = "Profile", shortName = "P" },
            new Module { id=3, name = "Expense", shortName = "E" },
            new Module { id=4, name = "Compensatory Off", shortName = "CO" },
            new Module { id=5, name = "Helpdesk", shortName = "HD" },
            new Module { id=6, name = "Travel", shortName = "T" },
            new Module { id=7, name = "Appraisals", shortName = "A" },
            new Module { id=8, name = "One to One", shortName = "OO" },
            new Module { id=9, name = "Confirmation", shortName = "C" }            
        };

        public void RunJob()
        {
            try
            {
                approvalService = new ApprovalService(_service);

                var allPendingApprovalIds = _service.Top<ApprovalDetail>(0, x => x.Status == 0 || x.Status == 3).Select(x => x.ApproverID).Distinct().ToList();
                var templateType = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ApprovalSummary));
                var templateContent = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ApprovalSummaryContent));
                string baseUrl = Convert.ToString(ConfigurationManager.AppSettings["baseUrl"]);

                foreach (var pendingApproval in allPendingApprovalIds)
                {
                    try
                    {
                        //var template = _service.First<EmailTemplate>(x => x.TemplateFor == templateType).Html;

                        var template = _service.First<EmailTemplate>(x => x.TemplateFor == templateContent).Html;
                        var mainTemplate = _service.First<EmailTemplate>(x => x.TemplateFor == templateType).Html;
                        var data = approvalService.ListAllApprovalsFor(pendingApproval.Value);
                        var person = _service.First<Person>(x => x.ID == pendingApproval.Value && x.Active == true);
                        string templateData = "";

                        if (data.Count > 0)
                        {
                            foreach (var approvals in data)
                            {
                                int moduleId = Convert.ToInt32(approvals.Module);
                                var module = lstModules.Where(x => x.id == moduleId).First();

                                if (approvals.Count != 0)
                                {
                                    templateData = templateData + template;
                                    templateData = templateData.Replace("{{module}}", module.name);
                                    templateData = templateData.Replace("{{modulecount}}", Convert.ToString(approvals.Count));
                                }
                            }

                            //template = template.Replace("{{imageUrl}}", baseUrl + person.Image);
                            //template = template.Replace("{{userfullname}}", person.FirstName + " " + person.LastName);
                            //template = template.Replace("{{profile}}", "0");
                            //template = template.Replace("{{attendance}}", "0");
                            //template = template.Replace("{{leaves}}", "0");
                            //template = template.Replace("{{expense}}", "0");
                            //template = template.Replace("{{confirmations}}", "0");
                            //template = template.Replace("{{seperations}}", "0");
                            //template = template.Replace("{{appraisals}}", "0");
                            //template = template.Replace("{{onetoone}}", "0");
                            //template = template.Replace("{{travel}}", "0");
                            //template = template.Replace("{{helpdesk}}", "0");
                            //template = template.Replace("{{compensatoryOff}}", "0");

                            mainTemplate = mainTemplate.Replace("{{imageUrl}}", baseUrl + person.Image);
                            mainTemplate = mainTemplate.Replace("{{userfullname}}", person.FirstName + " " + person.LastName);
                            mainTemplate = mainTemplate.Replace("{{content}}", templateData);

                            bool isCreated = _service.Create<Emails>(new Emails
                            {
                                Content = mainTemplate,
                                Date = DateTime.Now,
                                EmailFrom = person.PersonEmployment.First().OrganizationEmail,
                                EmailTo = person.PersonEmployment.First().OrganizationEmail,
                                Subject = "Weekly Report",
                            }, e => e.Id == 0);

                            if (isCreated)
                                _service.Finalize(true);
                        }
                    }
                    catch (Exception)
                    {
                        Log4Net.Error("Weekely Summary mail Failed for PersonId ==> " + pendingApproval.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Weekely Summary mail: " + ex.Message);
            }

        }
    }
}

//"Leave", "Profile", "Expense", "Compensatory Off", "HelpDesk", "Travel", "Appraisals", "One-To-One"
//let smallModules: Array<string> = ["leave", "profile", "expense", "compoff", "helpdesk", "travel", "appraisals", "onetoone"];
//let shortModules: Array<string> = ["L", "P", "E", "CO", "HD", "T", "A", "OO"];

public class Module
{
    public int id { get; set; }
    public string name { get; set; }
    public string shortName { get; set; }
}


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
    public class ClosePendingApprovalRequestEmail : IJob
    {
        private ApprovalService approvalService = null;
        public IBasicOperationsService _service;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ClosePendingApprovalRequestEmail(IBasicOperationsService service)
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
            new Module { id=9, name = "Confirmation", shortName = "C" },
            new Module { id=10, name = "Separation", shortName = "S" },
            new Module { id=11, name = "Separation Process", shortName = "SP" }
        };

        public void RunJob()
        {
            try
            {
                for (int count = 0; count < 3; count++)
                {
                    int days = CheckDates(count);
                    //DateTime toCheckDate = DateTime.Now.AddDays(2);
                    DateTime toCheckDate = DateTime.Now.AddDays(days + 1);
                    var _toCheckDate = toCheckDate.Date;
                    var DB = new PhoenixEntities();
                    List<int> resignedPersonIDs = new List<int>();
                    approvalService = new ApprovalService(_service);
                    var separationData = DB.Separation.Where(x => x.StatusID != 1 && x.StatusID != 5 && x.ApprovalDate == _toCheckDate && x.Person1.PersonEmployment.FirstOrDefault().OfficeLocation.Value == count).ToList();
                    foreach (var item in separationData)
                    {
                        resignedPersonIDs.Add(item.PersonID);
                    }

                    var allPendingApprovalIds = _service.Top<ApprovalDetail>(0, x => (x.Status == 0 || x.Status == 3) && resignedPersonIDs.Contains(x.ApproverID.Value)).Select(x => x.ApproverID).Distinct().ToList();
                    var templateType = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ApprovalSummaryMail));
                    var templateContent = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SeparationApprovalContent));
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

                            string exitProcessMgrEmail = GetPersonEmail(person.PersonEmployment.First().ExitProcessManager.Value);

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

                                mainTemplate = mainTemplate.Replace("{{imagename}}", baseUrl + person.Image);
                                mainTemplate = mainTemplate.Replace("{{username}}", person.FirstName + " " + person.LastName);
                                mainTemplate = mainTemplate.Replace("{{employeeid}}", person.ID.ToString());
                                mainTemplate = mainTemplate.Replace("{{content}}", templateData);
                                mainTemplate = mainTemplate.Replace("{{loggedinuser}}", "Vibrant Desk");

                                bool isCreated = _service.Create<Emails>(new Emails
                                {
                                    Content = mainTemplate,
                                    Date = DateTime.Now,
                                    EmailFrom = ConfigurationManager.AppSettings["helpdeskEmailId"].ToString(),
                                    EmailTo = person.PersonEmployment.First().OrganizationEmail,
                                    EmailCC = exitProcessMgrEmail + "," + GetHRGroupEmailIds(),
                                    Subject = "Notification : Approvals pending on Vibrant web Refresh",
                                }, e => e.Id == 0);

                                if (isCreated)
                                    _service.Finalize(true);
                            }
                        }

                        catch (Exception)
                        {
                            Log4Net.Error("Pending closure email Failed for PersonId ==> " + pendingApproval.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Pending closure email for On-Notice user: " + ex.Message);
            }

        }

        public string GetPersonEmail(int personID)
        {
            PhoenixEntities entites = new PhoenixEntities();
            Person personData = entites.People.Where(x => x.ID == personID).FirstOrDefault();
            string email = personData.PersonEmployment.First().OrganizationEmail;
            return email;
        }

        string GetHRGroupEmailIds()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            string hrGroupRoleId = (ConfigurationManager.AppSettings["HRGroupForSeparation"]).ToString();
            emails = context.HelpDeskCategories.Where(t => t.Prefix == hrGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }

        public int CheckDates(int location)
        {
            DateTime daytoCheck = DateTime.Now.AddDays(2);
            int count = 1;
            using (PhoenixEntities context = new PhoenixEntities())
            {

                var holidayList = context.HolidayList.Where(x => x.Location == location);

                for (DateTime index = DateTime.Now.Date; index <= daytoCheck; index = index.AddDays(1))
                {
                    foreach (var holiday in holidayList)
                    {
                        if (index.Date.CompareTo(holiday.Date) == 0)
                        {
                            count++;
                            daytoCheck = daytoCheck.AddDays(1);
                        }
                    }

                    if (index.DayOfWeek == DayOfWeek.Saturday || index.DayOfWeek == DayOfWeek.Sunday)
                    {
                        count++;
                        daytoCheck = daytoCheck.AddDays(1);
                        //if (index.AddDays(+count).DayOfWeek == DayOfWeek.Saturday || index.AddDays(+count).DayOfWeek == DayOfWeek.Sunday)
                        //{
                        //    count++;
                        //    index.AddDays(+count);

                        //    foreach (var holiday in holidayList)
                        //    {
                        //        if (index.Date.CompareTo(holiday.Date) == 1)
                        //        {
                        //            count++;
                        //            daytoCheck.AddDays(+count);
                        //        }
                        //    }
                        //}
                    }
                }


            }

            return count;
        }
    }
}

//"Leave", "Profile", "Expense", "Compensatory Off", "HelpDesk", "Travel", "Appraisals", "One-To-One"
//let smallModules: Array<string> = ["leave", "profile", "expense", "compoff", "helpdesk", "travel", "appraisals", "onetoone"];
//let shortModules: Array<string> = ["L", "P", "E", "CO", "HD", "T", "A", "OO"];


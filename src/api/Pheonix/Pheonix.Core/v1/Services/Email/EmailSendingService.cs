using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Pheonix.Core.Helpers;
using Pheonix.Models.ViewModels;
using System.Text;
using AutoMapper;
using Pheonix.Models.VM;
using System.Text.RegularExpressions;
using Pheonix.Models.VM.Classes.ResourceAllocation;
using System.IO;
using Pheonix.Models;
using Pheonix.Models.VM.Classes.Candidate;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using static Pheonix.Core.Helpers.EnumHelpers;

namespace Pheonix.Core.v1.Services.Email
{
    public class EmailSendingService : IEmailService
    {
        private IBasicOperationsService service;
        private List<EmailTemplate> lstTemlpates = null;
        private int[] monthDay = new int[12] { 31, -1, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private PhoenixEntities _phoenixEntity;

        public EmailSendingService(IBasicOperationsService opsService)
        {
            service = opsService;
            lstTemlpates = fillTemplates();
            _phoenixEntity = new PhoenixEntities();
            //_phoenixEntity.Database.Connection.Open();
        }
        public void SendExpenseApprovalEmail(Expense expense, Person person, ApprovalStage stage, string comments)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;
            int emailTempalteType = 0;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ExpenseApproval))).First();

            string template = emailTemplate.Html;
            var allStatements = JsonConvert.DeserializeObject<ApprovalStatements>(emailTemplate.OptionalVariables);
            var allSubjects = JsonConvert.DeserializeObject<ApprovalStatements>(emailTemplate.Subjects);
            Person reporting = null;

            switch (stage)
            {
                case ApprovalStage.Submitted:
                    {
                        statement = allStatements.submitted;
                        subject = allSubjects.submitted;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == expense.PrimaryApprover).FirstOrDefault();
                            reportingManager = reporting.PersonEmployment.First().OrganizationEmail;
                            emailActionId = Convert.ToString(expense.ExpenseId);
                            emailTempalteType = Convert.ToInt32(EnumHelpers.EmailTemplateType.ExpenseApproval);
                        }
                        break;
                    }
                //case ExpenseStage.PrimaryApprover:
                //    {
                //        statement = allStatements.primaryApprover;
                //        subject = allSubjects.primaryApprover;
                //        using (PhoenixEntities entites = new PhoenixEntities())
                //        {
                //            reportingManager = QueryHelper.GetManger(entites, person.ID).organizationemail;
                //        }
                //        break;
                //    }
                case ApprovalStage.SecondaryApprover:
                    {
                        statement = allStatements.secondaryApprover;
                        subject = allSubjects.secondaryApprover;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            int roleId = Convert.ToInt32(ConfigurationManager.AppSettings["FinanceRoleId"]);
                            reportingManager = populateReportingManager(QueryHelper.GetFinanceEmail(roleId));
                            reporting = entites.PersonInRole.Where(x => x.RoleID == roleId).FirstOrDefault().Person;
                        }
                        emailCC = expense.Person.PersonEmployment.FirstOrDefault().OrganizationEmail;
                        //if (reportingManager.ToLower() == personCC.ToLower())
                        //emailCC = personCC;

                        break;
                    }
                case ApprovalStage.ModuleAdmin:
                    {
                        statement = allStatements.finance;
                        subject = allSubjects.finance;
                        reportingManager = expense.Person.PersonEmployment.FirstOrDefault().OrganizationEmail;
                        reporting = expense.Person;
                        break;
                    }
                case ApprovalStage.Rejected:
                    {
                        statement = allStatements.rejected;
                        subject = allSubjects.rejected;
                        reportingManager = expense.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = expense.Person;
                        break;
                    }
                case ApprovalStage.OnHold:
                    {
                        statement = allStatements.onHold;
                        subject = allSubjects.onHold;
                        reportingManager = expense.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = expense.Person;
                        break;
                    }
            }

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{statements}}", statement);
            template = template.Replace("{{username}}", expense.Person.FirstName + " " + expense.Person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(expense.Person.ID));
            template = template.Replace("{{toname}}", reporting.FirstName + " " + reporting.LastName);   // logged in user reporting manager.
            template = template.Replace("{{statusfrom}}", "Open");
            template = template.Replace("{{statusto}}", "Assigned");
            template = template.Replace("{{formname}}", expense.ReimbursementTitle);
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
            template = template.Replace("{{comments}}", comments);
            template = template.Replace("{{requestedby}}", expense.Person.FirstName + " " + expense.Person.LastName);
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + expense.Person.Image);
            template = template.Replace("{{requestid}}", Convert.ToString(expense.ExpenseId));
            template = template.Replace("{{seatlocation}}", expense.Person.PersonEmployment.First().SeatingLocation);
            template = template.Replace("{{extension}}", expense.Person.PersonEmployment.First().OfficeExtension);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                EmailTo = reportingManager,    ///Email address of logged in user reporting manager. except for last finance stage
                Subject = subject.Replace("{{formname}}", expense.ReimbursementTitle),
                EmailCC = emailCC,
                //EmailAction = emailActionId,
                //TemplateType = emailTempalteType
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        public void SendUserProfileApproval(int empID, string empName, string cardName, string from, string to, string imageName)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.UserProfile))).First().Html;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{toname}}", "HR");
            template = template.Replace("{{username}}", empName);
            template = template.Replace("{{employeeid}}", empID.ToString());
            template = template.Replace("{{employeedetailstab}}", cardName);
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + imageName);
            template = template.Replace("{{loggedinuser}}", empName);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = from,
                EmailTo = to,
                Subject = "User Profile",
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        public void SendUserProfileStatus(int empID, string empName, string cardName, string from, string to, int statusID, string imageName)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.UserProfileStatus))).First().Html;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{toname}}", empName);
            template = template.Replace("{{username}}", empName);
            template = template.Replace("{{employeeid}}", empID.ToString());
            template = template.Replace("{{employeedetailstab}}", cardName);
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + imageName);
            template = template.Replace("{{loggedinuser}}", "HR"); //ND: Need to change this to HR's name.
            switch (statusID)
            {
                case 1:
                    template = template.Replace("{{status}}", "Approved");
                    break;
                case 2:
                    template = template.Replace("{{status}}", "Rejected");
                    break;
                case 3:
                    template = template.Replace("{{status}}", "OnHold.<br>Need Documents for verification");
                    break;
                default:
                    break;
            }

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = from,
                EmailTo = to,
                Subject = "User Profile Status",
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        public void SendAttendanceApproval(int empID, string empName, string senderEmail, string approverName, string approvalEmail, string startDate, string imageName)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Attendance))).First().Html;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{toname}}", approverName);
            template = template.Replace("{{username}}", empName);
            template = template.Replace("{{employeeid}}", empID.ToString());
            template = template.Replace("{{startdate}}", "(" + startDate + ")");
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + imageName);
            template = template.Replace("{{loggedinuser}}", empName);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = senderEmail,
                EmailTo = approvalEmail,
                Subject = "Manual SignIn-SignOut",
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        public void SendLeaveApproval(EmailLeaveDetails objEmailLeaveDetails)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Leave))).First().Html;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{toname}}", objEmailLeaveDetails.approverName);
            template = template.Replace("{{username}}", objEmailLeaveDetails.personName);
            template = template.Replace("{{employeeid}}", objEmailLeaveDetails.personID.ToString());
            template = template.Replace("{{startdate}}", objEmailLeaveDetails.FromDate1);
            template = template.Replace("{{enddate}}", objEmailLeaveDetails.ToDate1);
            template = template.Replace("{{noofdays}}", objEmailLeaveDetails.leaveCount.ToString());
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + objEmailLeaveDetails.personImage);
            template = template.Replace("{{loggedinuser}}", objEmailLeaveDetails.personName);
            template = template.Replace("{{narration}}", objEmailLeaveDetails.Narration);


            var locationId = service.First<PersonEmployment>(x => x.PersonID == objEmailLeaveDetails.personID);

            string leaveTypeText = "";

            switch (objEmailLeaveDetails.LeaveType)
            {
                case 0:
                    leaveTypeText = "Comp Off";
                    break;
                case 1:
                    if (locationId.OfficeLocation == 2)
                    {
                        leaveTypeText = "Leave";
                    }
                    else
                    {
                        leaveTypeText = "Privilege Leave";
                    }
                    break;
                case 2:
                    leaveTypeText = "LWP";
                    break;
                case 3:
                    leaveTypeText = "Maternity Leave";
                    break;
                case 4:
                    leaveTypeText = "Paternity Leave";
                    break;
                case 5:
                    leaveTypeText = "Long Leave";
                    break;
                case 6:
                    leaveTypeText = "Birthday Leave";
                    break;
                case 7:
                    leaveTypeText = "Floating Holiday";
                    break;
                case 8:
                    leaveTypeText = "Special Floating Holiday";
                    break;
                case 9:
                    leaveTypeText = "Casual Leave";
                    break;
                case 10:
                    leaveTypeText = "MTP";
                    break;
                case 11:
                    leaveTypeText = "Sick Leave";
                    break;
                default:
                    leaveTypeText = "";
                    break;

            }

            template = template.Replace("{{leavetype}}", leaveTypeText);

            bool isCreated = false;
            if (objEmailLeaveDetails.LeaveType == 3) 
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = objEmailLeaveDetails.personOrganizationEmail,
                    EmailTo = objEmailLeaveDetails.approverOrganizationEmail,
                    Subject = "Leave Applied",
                    EmailAction = objEmailLeaveDetails.ID.ToString(),
                    EmailCC = service.First<HelpDeskCategories>(x => x.Name == "RMG").EmailGroup + "," + service.First<HelpDeskCategories>(x => x.Name == "HR").EmailGroup,
                    TemplateType = Convert.ToInt32(EnumHelpers.EmailTemplateType.Leave)

                }, e => e.Id == 0);
            }
            else
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = objEmailLeaveDetails.personOrganizationEmail,
                    EmailTo = objEmailLeaveDetails.approverOrganizationEmail,
                    Subject = "Leave Applied",
                    EmailAction = objEmailLeaveDetails.ID.ToString(),
                    TemplateType = Convert.ToInt32(EnumHelpers.EmailTemplateType.Leave)

                }, e => e.Id == 0);
            }

            if (isCreated)
                service.Finalize(true);
        }
        public void SendLeaveApprovalStatus(int empID, string empName, string senderEmail, string approverName, string approvalEmail, string startDate, string endDate, int noOfDays, string status, string imageName, int? leaveType)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.LeaveStatus))).First().Html;
            string copyToMail = string.Empty;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{toname}}", empName);
            template = template.Replace("{{username}}", empName);
            template = template.Replace("{{employeeid}}", empID.ToString());
            template = template.Replace("{{status}}", status);
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + imageName);
            template = template.Replace("{{loggedinuser}}", approverName);
            //For: #149624435 - Change done to display leave From date, To date & No. of days
            template = template.Replace("{{startdate}}", "(" + startDate + ")");
            template = template.Replace("{{enddate}}", "(" + endDate + ")");
            template = template.Replace("{{noofdays}}", noOfDays.ToString());

            if (leaveType == 3)
                copyToMail = service.First<HelpDeskCategories>(x => x.Name == "RMG").EmailGroup + "," + service.First<HelpDeskCategories>(x => x.Name == "HR").EmailGroup;


            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = senderEmail,
                EmailTo = approvalEmail,
                Subject = "Leave Approval Status",
                EmailCC = copyToMail,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        public void SendHelpdeskApproval(string issueId, PersonHelpDesk newHelpDeskTicket, string empName, string senderEmail, string approverName, string approvalEmail, int status, string managerName, string managerEmail, string managerComment)
        {
            string template = CreateTemplateOnStatus(issueId, newHelpDeskTicket, empName, ref senderEmail, approverName, ref approvalEmail, status, managerName, managerEmail, managerComment);
            string emailCC = string.Empty;
            if (newHelpDeskTicket.AssignedTo != null)
                emailCC = service.First<HelpDeskCategories>(x => x.ID == newHelpDeskTicket.CategoryID).EmailGroup;
            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = senderEmail,
                EmailTo = approvalEmail,
                EmailCC = emailCC,
                Subject = "HelpDesk Approval",
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        public void SendAppraisalInitiat(string senderEmail, string sendToEmail)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ApprasalInitiation))).First().Html;

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = senderEmail,
                EmailTo = sendToEmail,
                Subject = "Self Appraisal for the year " + Convert.ToString(DateTime.Now.Year - 1) + "-" + Convert.ToString(DateTime.Now.Year),
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        public void SendTravelApproval(Traveller travel, Person loggedInUser, ApprovalStage stage, string comments)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.TravelApproval))).First();

            string template = emailTemplate.Html;
            var allStatements = JsonConvert.DeserializeObject<ApprovalStatements>(emailTemplate.OptionalVariables);
            var allSubjects = JsonConvert.DeserializeObject<ApprovalStatements>(emailTemplate.Subjects);
            Person reporting = null;
            string title = "Travel Approval";
            emailCC = string.Concat(emailCC, "travel@v2solutions.com");

            switch (stage)
            {
                case ApprovalStage.Submitted:
                    {
                        statement = allStatements.submitted;
                        subject = allSubjects.submitted;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == travel.PrimaryApproverId).FirstOrDefault();
                            reportingManager = reporting.PersonEmployment.First().OrganizationEmail;
                        }
                        break;
                    }
                case ApprovalStage.PrimaryApprover:
                    {
                        statement = allStatements.primaryApprover;
                        subject = allSubjects.primaryApprover;
                        reportingManager = travel.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = travel.Person;
                        //To send mail for Exit Process Manager
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == travel.PrimaryApproverId).FirstOrDefault();
                            emailCC = String.Concat(emailCC, ",", reporting.PersonEmployment.First().OrganizationEmail);
                        }

                        break;
                    }
                case ApprovalStage.SecondaryApprover:
                    {
                        statement = allStatements.secondaryApprover;
                        subject = allSubjects.secondaryApprover;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            int roleId = Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"]);
                            reportingManager = populateReportingManager(QueryHelper.GetFinanceEmail(roleId));
                            reporting = entites.PersonInRole.Where(x => x.RoleID == roleId).FirstOrDefault().Person;
                        }
                        emailCC = string.Concat(emailCC, ",", travel.Person.PersonEmployment.FirstOrDefault().OrganizationEmail);

                        break;
                    }
                case ApprovalStage.ModuleAdmin:
                    {
                        statement = allStatements.finance;
                        subject = allSubjects.finance;
                        reportingManager = travel.Person.PersonEmployment.FirstOrDefault().OrganizationEmail;
                        reporting = travel.Person;
                        break;
                    }
                case ApprovalStage.Rejected:
                    {
                        statement = allStatements.rejected;
                        subject = allSubjects.rejected;
                        reportingManager = travel.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = travel.Person;
                        break;
                    }
                case ApprovalStage.OnHold:
                    {
                        statement = allStatements.onHold;
                        subject = allSubjects.onHold;
                        reportingManager = travel.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = travel.Person;
                        break;
                    }
                case ApprovalStage.MoneyReload:
                    {
                        statement = allStatements.moneyreload;
                        subject = allSubjects.moneyreload;
                        reportingManager = travel.Person.PersonEmployment.First().OrganizationEmail;
                        title = "Travel Card Reload";
                        break;
                    }
                case ApprovalStage.TravelAdmin:
                    {
                        statement = allStatements.traveladmin;
                        subject = allSubjects.traveladmin;
                        reportingManager = travel.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = travel.Person;
                        //To send mail for Exit Process Manager
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == travel.PrimaryApproverId).FirstOrDefault();
                            emailCC = String.Concat(emailCC, ",", reporting.PersonEmployment.First().OrganizationEmail);
                        }

                        break;
                    }
                case ApprovalStage.BehalfOfEmpByTravelAdmin:
                    {
                        statement = allStatements.BehalfOfEmpByTravelAdmin;
                        subject = allSubjects.BehalfOfEmpByTravelAdmin;
                        reportingManager = travel.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = travel.Person;
                        //To send mail for Exit Process Manager
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == travel.PrimaryApproverId).FirstOrDefault();
                            emailCC = String.Concat(emailCC, ",", reporting.PersonEmployment.First().OrganizationEmail, ";");
                        }

                        break;
                    }
            }

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{statements}}", statement);
            template = template.Replace("{{username}}", travel.Person.FirstName + " " + travel.Person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(travel.Person.ID));
            if ((travel.TravelDetails.RequestType == 1))
            {
                template = template.Replace("{{titlename}}", "Travel Request");
                subject = subject.Replace("{{titlename}}", "Travel Request");
            }
            else if ((travel.TravelDetails.RequestType == 2))
            {
                template = template.Replace("{{titlename}}", "Accommodation Request");
                subject = subject.Replace("{{titlename}}", "Accommodation Request");
            }
            else if ((travel.TravelDetails.RequestType == 3))
            {
                template = template.Replace("{{titlename}}", "Travel with Accommodation Request");
                subject = subject.Replace("{{titlename}}", "Travel with Accommodation Request");
            }
            template = template.Replace("{{comments}}", comments);
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + travel.Person.Image);
            template = template.Replace("{{requestid}}", Convert.ToString(travel.Id));
            template = template.Replace("{{seatlocation}}", travel.Person.PersonEmployment.First().SeatingLocation);
            template = template.Replace("{{extension}}", travel.Person.PersonEmployment.First().OfficeExtension);
            template = template.Replace("{{title}}", title);


            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = loggedInUser.PersonEmployment.FirstOrDefault().OrganizationEmail,
                EmailTo = reportingManager,    ///Email address of logged in user reporting manager. except for last finance stage
                Subject = subject,
                EmailCC = emailCC,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);

        }
        //TODO: Need to remove after testing
        public void SendResignationEmail(SeperationViewModel separation, string subject, string body, string emailFrom, string emailTo, Boolean isHR, Boolean isMgr, int CurrStatus, string allCC, string LogInUser)// string grpHeadEmail
        {
            //string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
            //string emailTo = string.Empty;
            string emailCC = string.Empty;
            string emailTemplate = string.Empty; //lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Separation))).First().Html;
            string reasonDesc = "";
            string deptName = "";
            string _fromDate = "";
            string _toDate = "";
            int noOfDays = 0;
            DateTime lastpresent = DateTime.Now; ;
            string years = "";
            DateTime _EmailReceivedOn = DateTime.Now;
            DateTime _ShowCauseNoticeSendOn = DateTime.Now;
            string _empReason = "";
            string deptComment = "";
            string DeptAdmin = "";
            int DeptAdminID = 0;
            DateTime PIPStartDt = DateTime.Now;
            int onPIPDays = 0;
            string salutation = "";
            List<String> lstEmailCC = new List<string>();
            List<String> lstEmailTO = new List<string>();
            string strDistinctEmailCC = "";
            string strDistinctEmailTo = "";
            string strEmailDelimiter = Convert.ToString(ConfigurationManager.AppSettings["EmailDelimiter"]);

            if (separation == null)
            {
                List<string> result = body.Split(',').ToList();

                int _personID = Convert.ToInt32(result[0]);
                int leaveID = Convert.ToInt32(result[1]);
                //SeperationViewModel separations = new SeperationViewModel();
                var personSeparation = service.First<Separation>(t => t.PersonID == _personID);
                separation = Mapper.Map<Separation, SeperationViewModel>(personSeparation);

                separation.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personSeparation.Person1);
                //separation.EmployeeProfile = separations.EmployeeProfile;

                if (leaveID != 0)
                {
                    var leave = service.First<PersonLeave>(x => x.ID == leaveID);
                    //fromDate = leave.FromDate;
                    //toDate = leave.ToDate;
                    noOfDays = Convert.ToInt32(leave.Leaves);
                    _fromDate = leave.FromDate.ToString("dd MMMM,yyyy");
                    _toDate = leave.ToDate.ToString("dd MMMM,yyyy");
                }
            }

            using (PhoenixEntities DB = new PhoenixEntities())
            {
                if (CurrStatus == 1)
                {
                    TimeSpan span = separation.ResignDate - separation.EmployeeProfile.joiningDate;
                    int days = span.Days;
                    //or
                    //years = (int)(span.Days / 365.25);

                    //double month = (separation.ResignDate - separation.EmployeeProfile.joiningDate).Days / 30;
                    //double year = (separation.ResignDate - separation.EmployeeProfile.joiningDate).Days / 365;
                    //double _yrsInMonth = year * 12;
                    //double _remainingMonths = month - _yrsInMonth - 1;
                    //years = year.ToString() + " year(s) and " + _remainingMonths.ToString() + " month(s)";


                    DateTime fromDate = Convert.ToDateTime(separation.EmployeeProfile.joiningDate);
                    DateTime toDate = Convert.ToDateTime(separation.ResignDate);
                    int year;
                    int month;
                    int day;
                    int increment = 0;

                    if (fromDate.Day > toDate.Day)
                    {
                        increment = this.monthDay[fromDate.Month - 1];
                    }

                    if (increment == -1)
                    {
                        if (DateTime.IsLeapYear(fromDate.Year))
                        {
                            increment = 29;
                        }
                        else
                        {
                            increment = 28;
                        }
                    }

                    if (increment != 0)
                    {
                        day = (toDate.Day + increment) - fromDate.Day;
                        increment = 1;
                    }
                    else
                    {
                        day = toDate.Day - fromDate.Day;
                    }

                    if ((fromDate.Month + increment) > toDate.Month)
                    {
                        month = (toDate.Month + 12) - (fromDate.Month + increment);
                        increment = 1;
                    }
                    else
                    {
                        month = (toDate.Month) - (fromDate.Month + increment);
                        increment = 0;
                    }

                    year = toDate.Year - (fromDate.Year + increment);

                    years = year.ToString() + " year(s) and " + month.ToString() + " month(s)";
                    //string text = year.ToString() + " " + month.ToString() + " " + day.ToString();
                }

                if (separation.IsTermination == 1)
                {
                    //var data = DB.SignInSignOut.Where(x => x.UserID== separation.PersonID).ToList();                   
                    var data = service.Top<SignInSignOut>(0, x => x.UserID == separation.PersonID && x.DayNotation == "A").OrderBy(x => x.SignInSignOutID).ToList();
                    if (data != null && data.Count > 0)
                    {
                        var dt = data[0].AttendanceDate;
                        lastpresent = Convert.ToDateTime(dt);
                    }

                    if (CurrStatus == 20 || CurrStatus == 21 || CurrStatus == 22 || CurrStatus == 23 || CurrStatus == 24)
                    {
                        List<string> result = body.Split(',').ToList();

                        _ShowCauseNoticeSendOn = Convert.ToDateTime(result[0]);
                        _EmailReceivedOn = Convert.ToDateTime(result[1]);
                        _empReason = result[2];

                        DateTime dt1 = Convert.ToDateTime(result[0]);
                    }
                    else if (CurrStatus == 29)//On PIP Failure
                    {
                        var _onPIPEmpData = service.Top<PersonConfirmation>(0, x => x.PersonId == separation.PersonID).OrderByDescending(x => x.ID).ToList();
                        var _onPIPPersonData = service.Top<Person>(0, x => x.ID == separation.PersonID && x.Active == true).ToList();
                        onPIPDays = _onPIPEmpData[0].PIPTill.Value;
                        PIPStartDt = Convert.ToDateTime(_onPIPEmpData[0].ReviewDate);

                        if (_onPIPPersonData[0].Gender == 1)
                            salutation = "He";
                        else
                            salutation = "She";
                    }
                }

                if (separation.IsTermination == 0)
                {
                    int code = Convert.ToInt32(separation.SeperationReason);
                    var data = DB.SeparationReasons.Where(x => x.ID == code).ToList();
                    reasonDesc = data[0].ReasonDescription;
                }

                //if (CurrStatus == 10 || CurrStatus == 13 || CurrStatus == 7 || CurrStatus == 14 || CurrStatus == 16 || CurrStatus == 26 || CurrStatus == 5 || (CurrStatus == 19 && separation.TerminationReason == 5) || (CurrStatus == 19 && separation.TerminationReason == 1))//(CurrStatus == 9 && separation.IsTermination == 1)
                //    GetEmailToCCForAllDept(out _hrseparation);

                if (CurrStatus == 8 || CurrStatus == 28)
                {
                    List<string> result = body.Split(',').ToList();
                    deptName = result[0];//body;
                    DeptAdmin = result[1];
                    DeptAdminID = Convert.ToInt32(result[2]);

                    int deptRoleID = Convert.ToInt32(result[3]);
                    deptComment = service.First<SeperationProcess>(t => t.SeperationID == separation.ID && t.RoleID == deptRoleID).Comments;

                    //int appID = service.First<SeperationProcess>(t => t.SeperationID == separation.ID && t.RoleID == deptRoleID).ID;
                    //var approvalData = DB.Approval.Where(x => x.RequestID == appID).ToList();
                    //int approvalTblID = approvalData[0].ID;
                    //DeptAdminID = service.First<ApprovalDetail>(t => t.ApprovalID == approvalTblID).ApproverID.Value;
                    //DeptAdmin = service.First<Person>(t => t.Active == true && t.ID == DeptAdminID).FirstName.ToString() + " " + service.First<Person>(t => t.Active == true && t.ID == DeptAdminID).LastName.ToString();
                }

                switch (CurrStatus)
                {
                    case 1:
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SeparationApplication))).First().Html;
                        break;
                    case 2: //HR approved
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.HRResignationApproval))).First().Html;
                        break;
                    case 3://EPM approved
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.EPMResignationApproval))).First().Html;
                        break;
                    case 4: //Emp. Withdraw
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.EmployeeWithdrawResignation))).First().Html;
                        break;
                    case 5: //EPM approved Withdraw
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.EPMApproveWithdrawRequest))).First().Html;
                        break;
                    case 6: //Emp. reject Withdraw
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.EPMRejectWithdrawRequest))).First().Html;
                        break;
                    case 7: //Job initiated
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SeparationProcessInitiated))).First().Html;
                        break;
                    case 8: //Department clearance by Dept.Admin itself
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.DepartmentClearance))).First().Html;
                        break;
                    case 9: //Fill exit interview form
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.FillExitForm))).First().Html;
                        break;
                    case 10: //ToDo list 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ToDoList))).First().Html;
                        break;
                    case 11: //NP Extend 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.NPExtension))).First().Html;
                        break;
                    case 12: //NP not extended 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.NoExtension))).First().Html;
                        break;
                    case 13: //Exit date change
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ReleaseDateChange))).First().Html;
                        break;
                    case 14: //Block Access
                        if (separation.IsTermination == 1 && separation.TerminationReason != 3)//In case of HR separation
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.TempAccessBlockForHRSeparation))).First().Html;
                        else
                            // In Case HR separation and Resignation without settelemt
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.TempAccessBlockForEmpResignation))).First().Html;

                        //else//In case of Employee resigned and not coming to office
                        //    emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.TempAccessBlockForEmpResignation))).First().Html;

                        break;
                    case 15: //Absconding SCN1 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.AbscSCN1))).First().Html;
                        break;
                    case 16: //HR Separation completed
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.HRSeparationCompleted))).First().Html;
                        break;
                    case 17: //Leave utilization during NP
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.LWPNotification))).First().Html;
                        break;
                    case 18: //Resignation w/o settlement SCN1
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.WithoutSettlementSCN1))).First().Html;
                        break;
                    case 19: //HR Separation Process Initiated
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.HRSeparationInitiation))).First().Html;
                        break;
                    case 20:
                        if (separation.TerminationReason == 4)//Resignation WO Settlement Show cause notice 2 DDL1
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type1))).First().Html;
                        else if (separation.TerminationReason == 3) //Absconding SCN2 Type -1
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.AbscSCN2Type1))).First().Html;
                        break;
                    case 21: //Show cause notice 2 DDL2
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type2))).First().Html;
                        break;
                    case 22: //Show cause notice 2 DDL3
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type3))).First().Html;
                        break;
                    case 23: //Show cause notice 2 DDL4
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type4))).First().Html;
                        break;
                    case 24: //Show cause notice 2 DDL5
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type5))).First().Html;
                        break;
                    case 25: //Exit interview form submitted by Employee
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ExitFormSubmitted))).First().Html;
                        break;
                    case 26: //Exit interview form submitted by HR
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SeparationProcessClosed))).First().Html;
                        break;
                    case 27: //Intimation to EPM when HR accepts employee resignation
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.IntimationToEPM))).First().Html;
                        break;
                    case 28: //Department clearance by HR on behalf of Dept. Admin
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.DeptClearanceByHR))).First().Html;
                        break;
                    case 29: //On PIP failure
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.OnPIP))).First().Html;
                        break;
                    case 30: //Contract conversion Process Initiated
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ContractEndInitiate))).First().Html;
                        break;
                    default:
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Separation))).First().Html;
                        break;
                }

                GetEmailToCCForSeparation(separation, out emailCC, isHR, isMgr, CurrStatus); //, grpHeadEmail
                emailCC = emailCC + ";" + allCC;
                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(';').Distinct().ToList(); // Replace ; to , when removing ; From Seperation Emails
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + strEmailDelimiter;
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(';').Distinct().ToList();  // Replace ; to , when removing ; From Seperation Emails               
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + strEmailDelimiter;
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                string exitProcessMgr = GetExitProcessManager(separation.PersonID).Name;
                string currentProjectNames = "";
                if (CurrStatus == 1)
                {
                    try
                    {
                        var userID = 0;
                        if (separation != null)
                        {
                            userID = separation.PersonID;
                        }
                        if (userID != 0)
                        {
                            List<RACurrentAllocation> currentProjectList = (from pra in DB.PMSResourceAllocation
                                                                            where pra.PersonID == userID
                                                                            orderby pra.ID descending
                                                                            select new RACurrentAllocation
                                                                            {
                                                                                ProjectName = DB.ProjectList.Where(x => x.ID == pra.ProjectID).Select(x => x.ProjectName).FirstOrDefault()
                                                                            }).ToList();

                            int i = 0;
                            do
                            {
                                if (currentProjectList[i] != null)
                                {
                                    if (i == 0)
                                    {
                                        currentProjectNames = currentProjectList[i].ProjectName;
                                    }
                                    else
                                    {
                                        currentProjectNames = currentProjectNames + ", <br> " + currentProjectList[i].ProjectName;
                                    }
                                    i++;
                                }
                            }
                            while (i < currentProjectList.Count());
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

                string template = emailTemplate; //emailTemplate.Html;
                template = template.Replace("{{date}}", DateTime.Today.ToShortDateString());
                template = template.Replace("{{username}}", separation.EmployeeProfile.FirstName + " " + separation.EmployeeProfile.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(separation.EmployeeProfile.ID));
                template = template.Replace("{{SeparationMessage}}", body);
                template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + separation.EmployeeProfile.ImagePath);
                template = template.Replace("{{resigndate}}", (separation.ResignDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{noticeperiod}}", Convert.ToString(separation.NoticePeriod));
                template = template.Replace("{{seperationreason}}", Convert.ToString(reasonDesc));
                template = template.Replace("{{ExpectedDate}}", (separation.ExpectedDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{ActualDate}}", (separation.ActualDate.ToString("dd MMMM,yyyy")));
                if (separation.Comments != "")
                    template = template.Replace("{{Comments}}", Convert.ToString(separation.Comments));
                else
                    template = template.Replace("{{Comments}}", Convert.ToString("NA"));

                template = template.Replace("{{ApprovalDate}}", (separation.ApprovalDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{RejectRemark}}", Convert.ToString(separation.RejectRemark));
                template = template.Replace("{{WithdrawRemark}}", Convert.ToString(separation.WithdrawRemark));
                template = template.Replace("{{loggedinuser}}", Convert.ToString(LogInUser));
                template = template.Replace("{{DeptName}}", Convert.ToString(deptName));
                template = template.Replace("{{Designation}}", Convert.ToString(separation.EmployeeProfile.CurrentDesignation.Contains("Consultant -") ? "Consultant" : separation.EmployeeProfile.CurrentDesignation));
                template = template.Replace("{{experience}}", years.ToString());

                if (!String.IsNullOrEmpty(currentProjectNames))
                    template = template.Replace("{{projectName}}", currentProjectNames);
                else
                    template = template.Replace("{{projectName}}", "-");

                template = template.Replace("{{exitProcessMgr}}", Convert.ToString(exitProcessMgr));
                template = template.Replace("{{abscentDate}}", _fromDate.ToString());
                template = template.Replace("{{noOfDays}}", noOfDays.ToString());
                template = template.Replace("{{lastpresent}}", (lastpresent.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{dateofjoining}}", (separation.EmployeeProfile.joiningDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{emailReceivedOn}}", (_EmailReceivedOn.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{showCauseNoticeSendOn}}", (_ShowCauseNoticeSendOn.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{empReason}}", (_empReason));
                template = template.Replace("{{lwpDate}}", (separation.LWPDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{DeptAdminID}}", Convert.ToString(DeptAdminID));
                template = template.Replace("{{DeptAdmin}}", DeptAdmin.ToString());
                template = template.Replace("{{PIPEndDt}}", separation.EmployeeProfile.probationReviewDate.ToString("dd MMMM,yyyy"));
                template = template.Replace("{{PIPStartDt}}", (PIPStartDt.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{PIPDayst}}", (onPIPDays.ToString()));
                template = template.Replace("{{PIPSalutation}}", salutation);
                template = template.Replace("{{PIPDiscussionDt}}", (Convert.ToDateTime(separation.ResignationWOSettlement).ToString("dd MMMM,yyyy")));

                if (deptComment != "" && deptComment != null)
                    template = template.Replace("{{deptComment}}", deptComment);
                else
                    template = template.Replace("{{deptComment}}", Convert.ToString("NA"));

                template = template.Replace("\"", "'");

                QueryHelper.InsertEmail(template, emailFrom, strDistinctEmailTo, strDistinctEmailCC, subject);
               
            }
        }

        public SeperationTerminationViewModel GetResignationEmail(SeperationViewModel separation, string subject, string body, string emailFrom, string emailTo, Boolean isHR, Boolean isMgr, int CurrStatus, string allCC, string LogInUser)// string grpHeadEmail
        {
            SeperationTerminationViewModel objResult = new SeperationTerminationViewModel();
            string emailCC = string.Empty;
            string emailTemplate = string.Empty; //lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Separation))).First().Html;
            string reasonDesc = "";
            string deptName = "";
            string _hrseparation = "";
            string _fromDate = "";
            string _toDate = "";
            int noOfDays = 0;
            DateTime lastpresent = DateTime.Now; ;
            string years = "";
            DateTime _EmailReceivedOn = DateTime.Now;
            DateTime _ShowCauseNoticeSendOn = DateTime.Now;
            string _empReason = "";
            string deptComment = "";
            string DeptAdmin = "";
            int DeptAdminID = 0;
            DateTime PIPStartDt = DateTime.Now;
            int onPIPDays = 0;
            string salutation = "";
            string template = "";
            List<String> lstEmailCC = new List<string>();
            List<String> lstEmailTO = new List<string>();
            string strDistinctEmailCC = "";
            string strDistinctEmailTo = "";
            string strEmailDelimiter = Convert.ToString(ConfigurationManager.AppSettings["EmailDelimiter"]);

            if (separation == null)
            {
                List<string> result = body.Split(',').ToList();

                int _personID = Convert.ToInt32(result[0]);
                int leaveID = Convert.ToInt32(result[1]);
                //SeperationViewModel separations = new SeperationViewModel();
                var personSeparation = service.First<Separation>(t => t.PersonID == _personID);
                separation = Mapper.Map<Separation, SeperationViewModel>(personSeparation);

                separation.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personSeparation.Person1);
                //separation.EmployeeProfile = separations.EmployeeProfile;

                if (leaveID != 0)
                {
                    var leave = service.First<PersonLeave>(x => x.ID == leaveID);
                    //fromDate = leave.FromDate;
                    //toDate = leave.ToDate;
                    noOfDays = Convert.ToInt32(leave.Leaves);
                    _fromDate = leave.FromDate.ToString("dd MMMM,yyyy");
                    _toDate = leave.ToDate.ToString("dd MMMM,yyyy");
                }
            }

            using (PhoenixEntities DB = new PhoenixEntities())
            {
                if (CurrStatus == 1)
                {
                    TimeSpan span = separation.ResignDate - separation.EmployeeProfile.joiningDate;
                    int days = span.Days;

                    DateTime fromDate = Convert.ToDateTime(separation.EmployeeProfile.joiningDate);
                    DateTime toDate = Convert.ToDateTime(separation.ResignDate);
                    int year;
                    int month;
                    int day;
                    int increment = 0;

                    if (fromDate.Day > toDate.Day)
                    {
                        increment = this.monthDay[fromDate.Month - 1];
                    }

                    if (increment == -1)
                    {
                        if (DateTime.IsLeapYear(fromDate.Year))
                        {
                            increment = 29;
                        }
                        else
                        {
                            increment = 28;
                        }
                    }

                    if (increment != 0)
                    {
                        day = (toDate.Day + increment) - fromDate.Day;
                        increment = 1;
                    }
                    else
                    {
                        day = toDate.Day - fromDate.Day;
                    }

                    if ((fromDate.Month + increment) > toDate.Month)
                    {
                        month = (toDate.Month + 12) - (fromDate.Month + increment);
                        increment = 1;
                    }
                    else
                    {
                        month = (toDate.Month) - (fromDate.Month + increment);
                        increment = 0;
                    }

                    year = toDate.Year - (fromDate.Year + increment);

                    years = year.ToString() + " year(s) and " + month.ToString() + " month(s)";
                    //string text = year.ToString() + " " + month.ToString() + " " + day.ToString();
                }

                if (separation.IsTermination == 1)
                {
                    //var data = DB.SignInSignOut.Where(x => x.UserID== separation.PersonID).ToList();                   
                    var data = service.Top<SignInSignOut>(0, x => x.UserID == separation.PersonID && x.DayNotation == "A").OrderByDescending(x => x.SignInSignOutID).ToList();
                    if (data != null && data.Count > 0)
                    {
                        var dt = data[0].AttendanceDate;
                        lastpresent = Convert.ToDateTime(dt);
                    }

                    if (CurrStatus == 20 || CurrStatus == 21 || CurrStatus == 22 || CurrStatus == 23 || CurrStatus == 24)
                    {
                        List<string> result = body.Split(',').ToList();

                        _ShowCauseNoticeSendOn = Convert.ToDateTime(result[0]);
                        _EmailReceivedOn = Convert.ToDateTime(result[1]);
                        _empReason = result[2];

                        DateTime dt1 = Convert.ToDateTime(result[0]);
                    }
                    else if (CurrStatus == 29)//On PIP Failure
                    {
                        var _onPIPEmpData = service.Top<PersonConfirmation>(0, x => x.PersonId == separation.PersonID).OrderByDescending(x => x.ID).ToList();
                        var _onPIPPersonData = service.Top<Person>(0, x => x.ID == separation.PersonID && x.Active == true).ToList();
                        onPIPDays = _onPIPEmpData[0].PIPTill.Value;
                        PIPStartDt = Convert.ToDateTime(_onPIPEmpData[0].ReviewDate);

                        if (_onPIPPersonData[0].Gender == 1)
                            salutation = "He";
                        else
                            salutation = "She";
                    }
                }

                if (separation.IsTermination == 0)
                {
                    int code = Convert.ToInt32(separation.SeperationReason);
                    var data = DB.SeparationReasons.Where(x => x.ID == code).ToList();
                    reasonDesc = data[0].ReasonDescription;
                }

                if (CurrStatus == 10 || CurrStatus == 13 || CurrStatus == 7 || CurrStatus == 14 || CurrStatus == 16 || CurrStatus == 26 || CurrStatus == 5 || (CurrStatus == 19 && separation.TerminationReason == 5) || (CurrStatus == 19 && separation.TerminationReason == 1))//(CurrStatus == 9 && separation.IsTermination == 1)
                    GetEmailToCCForAllDept(out _hrseparation);

                if (CurrStatus == 8 || CurrStatus == 28)
                {
                    List<string> result = body.Split(',').ToList();
                    deptName = result[0];//body;
                    DeptAdmin = result[1];
                    DeptAdminID = Convert.ToInt32(result[2]);

                    int deptRoleID = Convert.ToInt32(result[3]);
                    deptComment = service.First<SeperationProcess>(t => t.SeperationID == separation.ID && t.RoleID == deptRoleID).Comments;
                }

                switch (CurrStatus)
                {
                    case 11: //NP Extend 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.NPExtension))).First().Html;
                        break;
                    case 12: //NP not extended 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.NoExtension))).First().Html;
                        break;
                    case 13: //Exit date change
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ReleaseDateChange))).First().Html;
                        break;
                    case 14: //Block Access
                        if (separation.IsTermination == 1 && separation.TerminationReason != 3)//In case of HR separation( 3 is Resignation Without Setelment)
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.TempAccessBlockForHRSeparation))).First().Html;
                        else//In case of Employee resigned and not coming to office
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.TempAccessBlockForEmpResignation))).First().Html;
                        break;
                    case 15: //Absconding SCN1 
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.AbscSCN1))).First().Html; break;
                    case 18: //Resignation w/o settlement SCN1
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.WithoutSettlementSCN1))).First().Html;
                        break;
                    case 20:
                        if (separation.TerminationReason == 4)//Resignation WO Settlement Show cause notice 2 DDL1
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type1))).First().Html;
                        else if (separation.TerminationReason == 3) //Absconding SCN2 Type -1
                            emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.AbscSCN2Type1))).First().Html;
                        break;
                    case 21: //Show cause notice 2 DDL2
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type2))).First().Html;
                        break;
                    case 22: //Show cause notice 2 DDL3
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type3))).First().Html;
                        break;
                    case 23: //Show cause notice 2 DDL4
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type4))).First().Html;
                        break;
                    case 24: //Show cause notice 2 DDL5
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SCN2Type5))).First().Html;
                        break;
                    case 29: //On PIP failure
                        emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.OnPIP))).First().Html;
                        break;
                        //default:
                        //    emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Separation))).First().Html;
                        //    break;
                }

                GetEmailToCCForSeparation(separation, out emailCC, isHR, isMgr, CurrStatus); //, grpHeadEmail

                emailCC = emailCC + allCC;
                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(';').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + strEmailDelimiter;
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }

                // To Remove RMG Email ID Form Show Cause Notice - 1
                if (subject.Contains("Show cause notice - 1"))
                {
                    strDistinctEmailCC = strDistinctEmailCC.Replace("RMG@v2solutions.com", "");
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(';').Distinct().ToList();  // Replace ; to , when removing ; From Seperation Emails               
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + strEmailDelimiter;
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                string exitProcessMgr = GetExitProcessManager(separation.PersonID).Name;

                template = emailTemplate; //emailTemplate.Html;
                template = template.Replace("{{date}}", DateTime.Today.ToShortDateString());
                template = template.Replace("{{username}}", separation.EmployeeProfile.FirstName + " " + separation.EmployeeProfile.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(separation.EmployeeProfile.ID));
                template = template.Replace("{{SeparationMessage}}", body);
                template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + separation.EmployeeProfile.ImagePath);
                template = template.Replace("{{resigndate}}", (separation.ResignDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{noticeperiod}}", Convert.ToString(separation.NoticePeriod));
                template = template.Replace("{{seperationreason}}", Convert.ToString(reasonDesc));
                template = template.Replace("{{ExpectedDate}}", (separation.ExpectedDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{ActualDate}}", (separation.ActualDate.ToString("dd MMMM,yyyy")));
                if (separation.Comments != "")
                    template = template.Replace("{{Comments}}", Convert.ToString(separation.Comments));
                else
                    template = template.Replace("{{Comments}}", Convert.ToString("NA"));

                template = template.Replace("{{ApprovalDate}}", (separation.ApprovalDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{RejectRemark}}", Convert.ToString(separation.RejectRemark));
                template = template.Replace("{{WithdrawRemark}}", Convert.ToString(separation.WithdrawRemark));
                template = template.Replace("{{loggedinuser}}", Convert.ToString(LogInUser));
                template = template.Replace("{{DeptName}}", Convert.ToString(deptName));
                template = template.Replace("{{Designation}}", Convert.ToString(separation.EmployeeProfile.CurrentDesignation));
                template = template.Replace("{{experience}}", years.ToString());
                template = template.Replace("{{projectName}}", "-");
                template = template.Replace("{{exitProcessMgr}}", Convert.ToString(exitProcessMgr));
                template = template.Replace("{{abscentDate}}", _fromDate.ToString());
                template = template.Replace("{{noOfDays}}", noOfDays.ToString());
                template = template.Replace("{{lastpresent}}", (lastpresent.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{dateofjoining}}", (separation.EmployeeProfile.joiningDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{emailReceivedOn}}", (_EmailReceivedOn.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{showCauseNoticeSendOn}}", (_ShowCauseNoticeSendOn.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{empReason}}", (_empReason));
                template = template.Replace("{{lwpDate}}", (separation.LWPDate.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{DeptAdminID}}", Convert.ToString(DeptAdminID));
                template = template.Replace("{{DeptAdmin}}", DeptAdmin.ToString());
                template = template.Replace("{{PIPEndDt}}", separation.EmployeeProfile.probationReviewDate.ToString("dd MMMM,yyyy"));
                template = template.Replace("{{PIPStartDt}}", (PIPStartDt.ToString("dd MMMM,yyyy")));
                template = template.Replace("{{PIPDayst}}", (onPIPDays.ToString()));
                template = template.Replace("{{PIPSalutation}}", salutation);
                template = template.Replace("{{PIPDiscussionDt}}", (Convert.ToDateTime(separation.ResignationWOSettlement).ToString("dd MMMM,yyyy")));

                if (deptComment != "" && deptComment != null)
                    template = template.Replace("{{deptComment}}", deptComment);
                else
                    template = template.Replace("{{deptComment}}", Convert.ToString("NA"));

                objResult.Template = template;
                objResult.Subject = subject;
                objResult.EmailCC = strDistinctEmailCC;
                //objResult.EmailCC = emailCC + allCC + _hrseparation;
                objResult.EmailTo = strDistinctEmailTo;
                objResult.EmailFrom = emailFrom;
                objResult.isActionPerformed = true;
                objResult.message = "Successfully Get Temination Email By HR ";
            }

            return objResult;
        }

        public bool SendSCNotice(SeperationTerminationViewModel separation)// string grpHeadEmail
        {

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = separation.Template,
                Date = DateTime.Now,
                EmailFrom = separation.EmailFrom,
                EmailTo = separation.EmailTo,
                EmailCC = separation.EmailCC,
                Subject = separation.Subject,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
            return isCreated;
        }

        public void SendConfirmationEmail(PersonConfirmation confirmation, int editStyle, bool isHR)
        {
            string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
            string emailTo = string.Empty;
            string emailCC = string.Empty;

            GetEmailToCC(confirmation, out emailTo, out emailCC);

            string subject = string.Empty;
            string confirmationMessage = string.Empty;

            if (isHR) { subject = GetConfirmationEmailSubjectForHR(confirmation.ConfirmationState ?? 0, 0, confirmation); }
            else { subject = GetConfirmationEmailSubject(confirmation.ConfirmationState ?? 0, 0, confirmation); }

            if (isHR) { confirmationMessage = GetConfirmationMessageForHR(confirmation, editStyle); }
            else { confirmationMessage = GetConfirmationMessage(confirmation, editStyle); }

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Confirmation))).First();

            string template = emailTemplate.Html;
            template = template.Replace("{{date}}", DateTime.Now.ToStandardDate());
            //template = template.Replace("{{date}}", confirmation.ReviewDate.Value.Date.ToStandardDate());
            //if (confirmation.IsHRReviewDone == false && (confirmation.ConfirmationState == 1 || confirmation.ConfirmationState == 2 || confirmation.ConfirmationState == 3 || confirmation.ConfirmationState == 4 || confirmation.ConfirmationState == 5))
            //{
            //    template = template.Replace("{{date}}", GetConfirmationActionDate(confirmation.ID));
            //}
            //else
            //{
            //    template = template.Replace("{{date}}", confirmation.ReviewDate.Value.Date.ToStandardDate());
            //}
            template = template.Replace("{{username}}", confirmation.Person.FirstName + " " + confirmation.Person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(confirmation.Person.ID));
            template = template.Replace("{{ConfirmationMessage}}", confirmationMessage);
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + confirmation.Person.Image);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = emailFrom,
                EmailTo = emailTo,    ///Email address of logged in user reporting manager. except for last finance stage
                EmailCC = emailCC,
                Subject = subject,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }
        //string GetConfirmationActionDate(int ConfirmationId)
        //{
        //    DateTime ConfirmationActionDate = default(DateTime);
        //    using (PhoenixEntities entites = new PhoenixEntities())
        //    {
        //        int approvalId = entites.Approval.Where(x => x.RequestID == ConfirmationId && x.RequestType == 9).Select(x => x.ID).First();
        //        ConfirmationActionDate = Convert.ToDateTime(entites.ApprovalDetail.Where(x => x.ApprovalID == approvalId && x.Status == 1 && x.Stage == 1).Select(x => x.ApprovalDate));
        //    }
        //    return ConfirmationActionDate.Date.ToStandardDate();
        //}

        string GetEmployeesEmailId(Person person)
        {
            if (person.PersonEmployment.Count > 0)
            {
                var personEmployment = person.PersonEmployment.ToList()[0];
                return personEmployment.OrganizationEmail;
            }
            return string.Empty;
        }
        GetGroupHeadEmail_Result GetManager(int personId)
        {
            var manager = QueryHelper.GetGroupHeadEmail(personId, 4);
            return manager;
        }

        GetExitProcessManager_Result GetExitProcessManager(int personId)
        {
            var manager = QueryHelper.GetExitProcessManager(personId);
            return manager;
        }

        string GetApprover(int? personId)
        {
            PhoenixEntities context = new PhoenixEntities();
            string manager = context.PersonEmployment.Where(pe => pe.PersonID == personId).Select(pe => pe.OrganizationEmail).FirstOrDefault();
            return manager;
        }

        string GetITEmail()
        {
            PhoenixEntities context = new PhoenixEntities();
            string strItEmail = context.HelpDeskCategories.Where(hc => hc.ID == 4).Select(hc => hc.EmailGroup).FirstOrDefault();
            return strItEmail;
        }

        string GetHRGroupEmailIds()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            var hrGroupRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["HRGroupRoleId"]);
            emails = context.HelpDeskCategories.Where(t => t.ID == hrGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }
        string GetRMGGroupEmailIds()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            var rmgGroupRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["RMGGroupRoleId"]);
            emails = context.HelpDeskCategories.Where(t => t.ID == rmgGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }

        string GetProjectGroupHeadEmails(int? projectId)
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            IEnumerable<int> groupAllocationHeadIds = context.PMSResourceAllocation.Where(r => r.ProjectID == projectId && r.ProjectRole == 6).Select(x => x.PersonID);
            IEnumerable<int> groupConfigurationHeadIds = context.PMSConfiguration.Where(c => c.Project == projectId && c.Role == 6 && c.IsDeleted != true).Select(y => y.PersonID ?? 0);
            IEnumerable<int> groupHeadIds = groupAllocationHeadIds.Union(groupConfigurationHeadIds);
            foreach (int groupHeadId in groupHeadIds)
            {
                string email = string.Empty;
                email = GetApprover(groupHeadId);
                if (groupHeadIds.Count() > 1)
                {
                    emails = emails + email + ",";
                }
                else
                {
                    emails = email;
                }
            }
            return emails;
        }


        string GetProjectMngrEmails(int? projectId)
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            IEnumerable<int> projectMngrIds = context.ProjectList.Where(r => r.ID == projectId).Select(x => x.ProjectManager ?? 0).ToList();
            //IEnumerable<int> groupConfigurationHeadIds = context.PMSConfiguration.Where(c => c.Project == projectId && c.Role == 6).Select(y => y.PersonID ?? 0);
            //IEnumerable<int> groupHeadIds = groupAllocationHeadIds.Union(groupConfigurationHeadIds);
            //if(projectMngrIds[0] != null)
            //{
            foreach (int projectMngr in projectMngrIds)
            {
                string email = string.Empty;
                if (projectMngr != 0)
                {
                    email = GetApprover(projectMngr);
                }
                if (projectMngrIds.Count() > 1)
                {
                    emails = emails + email + ",";
                }
                else
                {
                    emails = email;
                }
            }
            // }            
            return emails;
        }
        string GetFinanceGroupEmailIds()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            var financeGroupRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["AccountGroupRoleId"]);
            emails = context.HelpDeskCategories.Where(t => t.ID == financeGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }
        string GetPMOGroupEmailIds()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            var pmoGroupRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["PMOGroupRoleId"]);
            emails = context.HelpDeskCategories.Where(t => t.ID == pmoGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }
        string GetHREmailIds()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            var hrRole = Convert.ToInt32(ConfigurationManager.AppSettings["HRRoleId"]);
            var list = context.PersonInRole.Where(t => t.RoleID == hrRole && t.Person.PersonEmployment.FirstOrDefault(k => k.Designation.Grade >= 4) != null);
            if (list.Count() > 0)
            {
                foreach (var hr in list.Take(1))
                {
                    var personBasic = AutoMapper.Mapper.Map<Person, Pheonix.Models.VM.EmployeeBasicProfile>(hr.Person);
                    if (string.IsNullOrEmpty(emails))
                        emails = personBasic.Email;
                    else
                        emails = string.Concat(emails, ";", personBasic.Email);
                }
            }
            return emails;
        }
        void GetEmailToCC(PersonConfirmation confirmation, out string emailTo, out string emailCC)
        {
            emailTo = string.Empty;
            emailCC = string.Empty;

            string reportingManager = GetExitProcessManager(confirmation.PersonId ?? 0).organizationemail;
            string approver = GetApprover(confirmation.ReportingManagerId);
            if (reportingManager != approver)
            {
                reportingManager = string.Concat(reportingManager, ",", approver);
            }
            string hrManagers = GetHRGroupEmailIds();
            //string reportingManager = GetManager(confirmation.PersonId ?? 0).organizationemail;
            //string hrManagers = GetHREmailIds();
            string employeeEmail = GetEmployeesEmailId(confirmation.Person);

            if (confirmation.IsHRReviewDone == true)
            {
                emailTo = employeeEmail;
                emailCC = string.Concat(reportingManager, ",", hrManagers);
            }
            else if (confirmation.ConfirmationState == null || confirmation.ConfirmationState == 0)
            {
                emailTo = reportingManager;
                emailCC = string.Concat(employeeEmail, ",", hrManagers);
            }
            else if (confirmation.ConfirmationState != 0 && !string.IsNullOrEmpty(confirmation.OverallFeedback))
            {
                emailTo = hrManagers;
                emailCC = reportingManager;
            }
        }
        string GetConfirmationMessage(PersonConfirmation confirmation, int editStyle)
        {
            using (PhoenixEntities entites = new PhoenixEntities())
            {
                var personEmployment = entites.PersonEmployment.Where(x => x.PersonID == confirmation.PersonId).FirstOrDefault();
                var finalReviewDay = Convert.ToDateTime(personEmployment.ProbationReviewDate).AddDays(1);
                var reportingManager = entites.People.Where(x => x.ID == confirmation.ReportingManagerId).FirstOrDefault();

                switch (confirmation.ConfirmationState)
                {
                    case 0:
                        {
                            return "Confirmation process for " + confirmation.Person.FirstName + " " + confirmation.Person.LastName + " (" + confirmation.Person.ID + ") has been <b>INITIATED</b>.<br>Kindly fill the confirmation form by or before due date " + string.Format("{0:dd MMMM,yyyy}", personEmployment.ProbationReviewDate) + ".";
                        }
                    default:
                        return "I have completed the confirmation process for   " + confirmation.Person.FirstName + " " + confirmation.Person.LastName + " (" + confirmation.Person.ID + ").<br><br>Regards<br>" + reportingManager.FirstName + " " + reportingManager.LastName;
                }
            }
        }
        string GetConfirmationEmailSubject(int confirmationStatus, int editStyle, PersonConfirmation confirmation)
        {
            switch (confirmationStatus)
            {
                case 0:
                    return "Confirmation Process Initiation for " + confirmation.Person.FirstName + " " + confirmation.Person.LastName;
                default:
                    return "Confirmation Process Reviewed for " + confirmation.Person.FirstName + " " + confirmation.Person.LastName;
            }
        }
        string GetConfirmationEmailSubjectForHR(int confirmationStatus, int editStyle, PersonConfirmation confirmation)
        {
            return "Confirmation Process Completed for " + confirmation.Person.FirstName + " " + confirmation.Person.LastName;
        }
        string GetConfirmationMessageForHR(PersonConfirmation confirmation, int editStyle)
        {
            switch (confirmation.ConfirmationState)
            {
                case 3:
                    return "Your confirmation process has been completed and it has been Extended till " + string.Format("{0:dd MMMM,yyyy}", confirmation.Person.PersonEmployment.FirstOrDefault().ProbationReviewDate) + ".<br><br>Schedule to collect the letter will be informed to you shortly by HR team.<br><br>Regards<br>HR";
                case 4:
                    return "Your confirmation process has been completed and Performance Improvement Program has been proposed by your manager.<br><br>Schedule to discuss the performance improvement plan will be informed to you shortly by HR team.<br><br>Regards<br>HR";
                default:
                    return "Congratulations !!<br>Your confirmation process has been completed and You have been Confirmed as on " + string.Format("{0:dd MMMM,yyyy}", confirmation.Person.PersonEmployment.FirstOrDefault().ProbationReviewDate) + ".<br><br>Schedule to collect the letter will be informed to you shortly by HR team.<br><br>Regards<br>HR";

            }
        }


        public void InitiateReminderEmails(PhoenixEntities context, IList<PersonConfirmation> confirmations)
        {
            try
            {
                string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                string emailTo = string.Empty;
                string emailCC = string.Empty;
                string subject = string.Empty;
                string confirmationMessage = string.Empty;

                GetReminderEmailToCC(confirmations, out emailTo, out emailCC);

                subject = "Pending Confirmation Process for below employees";

                confirmationMessage = GetReminderConfirmationMessage(confirmations);

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ConfrimationReminder))).First();

                string template = emailTemplate.Html;

                int? managerID = confirmations.FirstOrDefault().ReportingManagerId;

                var reportingManager = context.People.Where(x => x.ID == managerID).FirstOrDefault();

                template = template.Replace("{{userfullname}}", reportingManager.FirstName + " " + reportingManager.LastName);
                template = template.Replace("{{employeeid}}", reportingManager.ID.ToString());
                template = template.Replace("{{body}}", confirmationMessage);
                template = template.Replace("{{imageUrl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + reportingManager.Image);

                bool isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,    ///Email address of logged in user reporting manager. except for last finance stage
                    EmailCC = emailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);
            }
            catch
            {

            }
        }
        private string GetReminderConfirmationMessage(IList<PersonConfirmation> confirmations)
        {
            string result = string.Empty;
            StringBuilder sb = new StringBuilder();
            int SrNo = 1;

            foreach (var data in confirmations.OrderBy(x => x.PersonId))
            {
                var employee = service.First<Person>(x => x.ID == data.PersonId);
                int noOfDaysRemaining;
                if (employee.PersonEmployment.FirstOrDefault().ProbationReviewDate.Value.Date == DateTime.Now.Date)
                {
                    noOfDaysRemaining = 1;
                }
                else
                {
                    TimeSpan? days = employee.PersonEmployment.FirstOrDefault().ProbationReviewDate - DateTime.Now;
                    noOfDaysRemaining = days.Value.Days + 2;
                }




                sb.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"mcnBoxedTextBlock\" style=\"min-width:100%;\">");
                sb.AppendLine("<tbody class=\"mcnBoxedTextBlockOuter\"><tr><td valign=\"top\" class=\"mcnBoxedTextBlockInner\">");
                sb.AppendLine("<table align =\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"min-width:100%;\" class=\"mcnBoxedTextContentContainer\">");
                sb.AppendLine("<tbody><tr><td style =\"padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:18px;\" >");
                sb.AppendLine("<table border=\"0\" cellpadding=\"18\" cellspacing=\"0\" class=\"mcnTextContentContainer\" width=\"100%\" style=\"min-width:100 % !important; background-color:#BBD8E6;\">");
                sb.AppendLine("<tbody><tr><td valign =\"top\" class=\"mcnTextContent\" style=\"color:#020202;font-family:Helvetica;font-size:14px;font-weight:normal;\">");
                sb.AppendLine("<span style =\"font-size:16px;color:#444;\" >" + SrNo.ToString() + ". " + employee.FirstName + " " + employee.LastName + " (" + employee.ID + ") " + "&nbsp;<span><span style =\"color:#2D81AF\"><span style=\"font-size:19px\">");
                sb.AppendLine("&nbsp;</span></span></span><span style =\"color:#696969\"><span style =\"font-size:12px\"></span></span></span></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table> ");
                sb.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"mcnTextBlock\" style=\"min-width:100%;\">");
                sb.AppendLine("<tbody class=\"mcnTextBlockOuter\"><tr><td valign =\"top\" class=\"mcnTextBlockInner\" style=\"padding-top:9px;\">");
                sb.AppendLine("<table align =\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:100%; min-width:100%;\" width=\"100%\" class=\"mcnTextContentContainer\">");
                sb.AppendLine("<tbody><tr><td valign = \"top\" class=\"mcnTextContent\" style=\"padding: 0px 18px 9px; font-family: Arial, &quot;Helvetica Neue&quot;, Helvetica, sans-serif; line-height: 100%;\">");
                sb.AppendLine("<span style = \"font-size:12px\"> &nbsp; &nbsp; &nbsp;<b>" + noOfDaysRemaining + " Days remaining.</b></span></td></tr></tbody></table></td></tr></tbody></table>");



                result = sb.ToString();
                SrNo++;
            }
            return result;
        }
        void GetReminderEmailToCC(IList<PersonConfirmation> confirmation, out string emailTo, out string emailCC)
        {
            emailTo = string.Empty;
            emailCC = string.Empty;
            //string reportingManager = GetManager(confirmation[0].PersonId ?? 0).organizationemail;
            string reportingManager = GetExitProcessManager(confirmation[0].PersonId ?? 0).organizationemail;
            string approver = GetApprover(confirmation[0].ReportingManagerId);
            if (reportingManager != approver)
            {
                reportingManager = approver;
            }
            string hrManagers = GetHREmailIds();

            emailTo = reportingManager;
        }
        #region Private Methods

        private List<EmailTemplate> fillTemplates()
        {
            using (PhoenixEntities entity = new PhoenixEntities())
            {
                var data = entity.EmailTemplate.ToList();
                return data;
            }
        }

        private string CreateTemplateOnStatus(string issueId, PersonHelpDesk newHelpDeskTicket, string empName, ref string senderEmail, string approverName, ref string approvalEmail, int status, string managerName, string managerEmail, string managerComment)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Helpdesk))).First().Html;
            string statement, empEmail, subject = string.Empty;

            template = template.Replace("{{date}}", newHelpDeskTicket.IssueDate.Date.ToStandardDate());
            template = template.Replace("{{issueid}}", issueId);
            template = template.Replace("{{categories}}", newHelpDeskTicket.HelpDeskCategories.Name);
            template = template.Replace("{{subcategories}}", newHelpDeskTicket.HelpDeskSubCategories.Name);
            template = template.Replace("{{severity}}", Enum.GetName(typeof(EnumHelpers.HelpdeskSeverity), newHelpDeskTicket.Severity));
            template = template.Replace("{{description}}", newHelpDeskTicket.HelpDeskComments.First().Comments);
            template = template.Replace("{{status}}", EnumExtensions.GetEnumDescription((EnumHelpers.HelpdeskStatus)newHelpDeskTicket.Status));
            template = template.Replace("{{employeeid}}", newHelpDeskTicket.Person1.ID.ToString());
            template = template.Replace("{{username}}", newHelpDeskTicket.Person1.FirstName + " " + newHelpDeskTicket.Person1.LastName);
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + newHelpDeskTicket.Person1.Image);

            switch (status)
            {
                case 1:
                    template = template.Replace("{{toname}}", approverName);
                    template = template.Replace("{{loggedinuser}}", empName);
                    statement = "This is to inform you that,I have raised a new help desk request that requires your Approval. Kindly do the needful.";
                    template = template.Replace("{{statement}}", statement);
                    subject = "HelpDesk Approval";
                    break;
                case 2:
                    if (empName.Equals(managerName))
                    {
                        template = template.Replace("{{toname}}", approverName);
                        statement = "This is to inform you that, a new help desk request has been assigned to your department.";
                        template = template.Replace("{{statement}}", statement);
                        template = template.Replace("{{loggedinuser}}", empName);
                        subject = "HelpDesk Approval";
                    }
                    else
                    {
                        empEmail = senderEmail;
                        template = template.Replace("{{toname}}", string.Empty);
                        statement = "This is to inform you that, I have Approved the ticket raised  by '" + empName + "' with following comments '" + managerComment + "' and assigned to your department.";
                        template = template.Replace("{{statement}}", statement);
                        template = template.Replace("{{loggedinuser}}", managerName);
                        senderEmail = managerEmail;
                        approvalEmail += "," + empEmail;
                        subject = "HelpDesk Approval Status";
                    }
                    break;
                case 3:
                    empEmail = senderEmail;
                    template = template.Replace("{{toname}}", string.Empty);
                    statement = "This is to inform you that, I have Rejected the ticket raised  by '" + empName + "' with following comments '" + managerComment;
                    template = template.Replace("{{statement}}", statement);
                    template = template.Replace("{{loggedinuser}}", managerName);
                    senderEmail = managerEmail;
                    approvalEmail += "," + empEmail;
                    subject = "HelpDesk Approval Status";
                    break;
                case 8:
                    template = template.Replace("{{toname}}", approverName);
                    template = template.Replace("{{loggedinuser}}", empName);
                    statement = "Require the exact status of the ticket";
                    template = template.Replace("{{statement}}", statement);
                    subject = "HelpDesk Stat";
                    break;
                case 99:
                    template = template.Replace("{{toname}}", approverName);
                    template = template.Replace("{{loggedinuser}}", empName);
                    statement = "This ticket has been assigned to you" + " with following comments '" + managerComment + "'"; //For #146373813 on 10/08/2017
                    template = template.Replace("{{statement}}", statement);
                    subject = "HelpDesk Assigned";
                    break;
                case 98:
                    template = template.Replace("{{toname}}", approverName);
                    template = template.Replace("{{loggedinuser}}", empName);
                    statement = "This ticket has been assigned to your department.";
                    template = template.Replace("{{statement}}", statement);
                    subject = "HelpDesk Assigned To Department";
                    break;

                default:
                    empEmail = senderEmail;
                    template = template.Replace("{{toname}}", empName);
                    template = template.Replace("{{loggedinuser}}", approverName);
                    statement = "This is to inform you that, a issue status has been changed to '" + EnumExtensions.GetEnumDescription((EnumHelpers.HelpdeskStatus)status) + "' by '" + approverName + "' with following comments '" + managerComment + "'.";
                    template = template.Replace("{{statement}}", statement);
                    senderEmail = approvalEmail;
                    approvalEmail = empEmail;
                    subject = "HelpDesk Approval Status";
                    break;
            }
            return template;
        }

        private string populateReportingManager(List<string> list)
        {
            string managers = string.Empty;
            foreach (var item in list)
            {
                managers += item + ",";
            }

            return managers;
        }

        #endregion

        //TODO: Need to remove after testing
        void GetEmailToCCForSeparation(SeperationViewModel separation, out string emailCC, Boolean isHR, Boolean isMgr, int CurrStatus)
        {
            //emailTo = string.Empty;
            emailCC = string.Empty;
            Person reporting = null;
            string reportingManager = "";
            string employeeEmail = "";
            string exitProcessManager = "";
            string grpHeadEmail = "";
            string hrManagers = "";
            string projectReportingManagerEmail = "";
            string rmg = "";
            string hrseparation = "";
            string itEmail = GetITEmail();
            bool isDepartmentJobInitiated = false;
            using (PhoenixEntities entites = new PhoenixEntities())
            {
                if (isMgr == false)
                {
                    reporting = entites.People.Where(x => x.ID == separation.ApprovalID).FirstOrDefault();
                    exitProcessManager = reporting.PersonEmployment.First().OrganizationEmail;
                    reporting = null;
                }

                reporting = entites.People.Where(x => x.ID == separation.PersonID).FirstOrDefault();
                employeeEmail = reporting.PersonEmployment.First().OrganizationEmail;
                reporting = null;

                var _reportingManager = entites.PersonReporting.Where(x => x.PersonID == separation.PersonID && x.Active == true).ToList();
                int reportingTo = Convert.ToInt32(_reportingManager[0].ReportingTo);
                reporting = entites.People.Where(x => x.ID == reportingTo).FirstOrDefault();
                reportingManager = reporting.PersonEmployment.First().OrganizationEmail;
                reporting = null;
                var lstSeperationProcess = entites.SeperationProcess.Where(x => x.SeperationID == separation.ID).ToList();
                if (lstSeperationProcess.Count > 0)
                {
                    isDepartmentJobInitiated = true;
                }

                List<PMSResourceAllocation> projectReportingManager = entites.PMSResourceAllocation.Where(x => x.PersonID == separation.PersonID && x.IsDeleted == false).ToList() as List<PMSResourceAllocation>;
                for (int i = 0; i < projectReportingManager.Count; i++)
                {
                    int personReportingToId = projectReportingManager[i].ReportingTo;
                    if (i > 0)
                    {
                        projectReportingManagerEmail = projectReportingManagerEmail + ";";
                        projectReportingManagerEmail = projectReportingManagerEmail + entites.PersonEmployment.Where(x => x.PersonID == personReportingToId).Select(y => y.OrganizationEmail).FirstOrDefault();
                    }
                    else
                    {
                        projectReportingManagerEmail = entites.PersonEmployment.Where(x => x.PersonID == personReportingToId).Select(y => y.OrganizationEmail).FirstOrDefault();
                    }

                }

                GetEmailToCCForAllDept(out hrseparation);
                rmg = entites.HelpDeskCategories.Where(x => x.AssignedRole == 27).FirstOrDefault().EmailGroup;
                grpHeadEmail = QueryHelper.GetGroupHeadEmail(Convert.ToInt32(separation.PersonID),6).organizationemail;
            }

            //if (isHR == false)
            hrManagers = GetHRGroupForSeparation();//GetHRGroupEmailIds();
                                                   //string employeeEmail = separation.EmployeeProfile.Email;
                                                   //string grpHeadEmailID = grpHeadEmail;

            itEmail = GetITEmail();

            //emailTo = employeeEmail;
            switch (CurrStatus)
            {
                case 1://Employee resign into system
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", exitProcessManager, ";", rmg);
                    break;
                case 2: //HR approved
                    emailCC = string.Concat(grpHeadEmail, ";", reportingManager, ";", projectReportingManagerEmail, ";", exitProcessManager, ";", hrManagers, ";", rmg);
                    break;
                case 3://EPM approved
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", exitProcessManager, ";", rmg, hrseparation);
                    break;
                case 4: //Emp. Withdraw
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", rmg);
                    break;
                case 5: //EPM approved Withdraw
                    emailCC = string.Concat(hrManagers, ";", projectReportingManagerEmail, ";", reportingManager, ";", rmg);
                    if (isDepartmentJobInitiated) // If Deprtment is initiated 
                    {
                        emailCC = emailCC + string.Concat(";", hrseparation, ";", grpHeadEmail);
                    }
                    break;
                case 6: //Emp. reject Withdraw
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", rmg, ";", projectReportingManagerEmail);
                    break;
                case 7: //Job initiated
                    if (separation.IsTermination == 0)
                        emailCC = string.Concat(reportingManager, ";", exitProcessManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", hrseparation);
                    break;
                case 8: //Department clearance by Dept. Admin itself
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", exitProcessManager);
                    break;
                case 9://Fill exit interview form
                       //emailCC = string.Concat(grpHeadEmail, ";", reportingManager, ";", hrManagers, ";", exitProcessManager);
                    emailCC = string.Concat(hrManagers);
                    break;
                case 10://ToDo instructions triggered
                    emailCC = string.Concat(hrManagers);
                    break;
                case 11://NP extended
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", exitProcessManager, ";", projectReportingManagerEmail, ";", rmg, hrseparation);
                    break;
                case 12://NP not extended
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", exitProcessManager, ";", projectReportingManagerEmail);
                    break;
                case 13://Exit date change
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", exitProcessManager, ";", projectReportingManagerEmail, ";", rmg, hrseparation);
                    break;
                case 14://Block Access
                        //if (separation.TerminationReason == 4 || separation.TerminationReason == 2) // Absconding Case Or Absconding immediately after joining
                        //    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", employeeEmail, ";", projectReportingManagerEmail, ";", rmg, hrseparation);
                        //else
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", rmg, hrseparation);
                    break;
                case 15://Absconding SCN-1
                    emailCC = string.Concat(hrManagers);
                    break;
                case 16://process closing for HR Separation
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", exitProcessManager, ";", grpHeadEmail, ",", projectReportingManagerEmail, ",", rmg, hrseparation);
                    break;
                case 17://Leave utilization during NP
                    emailCC = string.Concat(reportingManager, ";", projectReportingManagerEmail, ";", hrManagers, ";", employeeEmail);
                    break;
                case 18://Resignation Without Settlement SCN-1
                    emailCC = string.Concat(hrManagers, ";", rmg);
                    break;
                case 19://Show cause notice 1
                    if (separation.TerminationReason != 5 && separation.TerminationReason != 1)
                        emailCC = string.Concat(hrManagers, ";", reportingManager, ";", projectReportingManagerEmail, ";", rmg);
                    else//On PIP & Separation(Asked To Leave)
                        emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", exitProcessManager, ";", rmg, hrseparation);
                    break;
                case 20://Show cause notice 2 DDL1
                    emailCC = string.Concat(hrManagers);
                    break;
                case 21://Show cause notice 2 DDL2
                    emailCC = string.Concat(hrManagers);
                    break;
                case 22://Show cause notice 2 DDL3
                    emailCC = string.Concat(hrManagers);
                    break;
                case 23://Show cause notice 2 DDL4
                    emailCC = string.Concat(hrManagers);
                    break;
                case 24://Show cause notice 2 DDL5
                    emailCC = string.Concat(hrManagers);
                    break;
                //case 25://Exit interview form completed by Employee
                //    emailCC = string.Concat(hrManagers, ";");
                //    break;
                case 26://Exit interview form completed by HR
                    emailCC = string.Concat(hrManagers, ";", grpHeadEmail, ";", reportingManager, ";", exitProcessManager, ";", rmg, hrseparation, ";", projectReportingManagerEmail);
                    break;
                //case 27://Intimation to EPM when HR accepts employee resignation
                //    emailCC = string.Concat(hrManagers);
                //    break;
                case 28: //Department clearance by HR on behalf of Dept. Admin
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", projectReportingManagerEmail, ";", exitProcessManager);
                    break;
                case 29: //On PIP failure
                    emailCC = string.Concat(reportingManager, ";", hrManagers, ";", employeeEmail);
                    break;
                case 30://Show cause notice 1                  
                    emailCC = string.Concat(hrManagers, ";", reportingManager, ";", projectReportingManagerEmail, ";", rmg, ";", itEmail);
                    break;
            }

            //emailCC = string.Concat(grpHeadEmail, ";", reportingManager, ";", hrManagers, ";", exitProcessManager);
        }

        public void SendResignationProcessEmails(SeperationViewModel separation, string subject, string body, string emailToCC)
        {
            string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
            string emailTo = separation.EmployeeProfile.Email;
            string emailCC = string.Empty;
            List<String> lstEmailCC = new List<string>();
            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.Separation))).First();

            GetEmailToCCForSeparation(separation, out emailCC, false, false, 0);

            emailCC = emailToCC + ";" + emailCC;
            if (emailCC.Contains(emailTo))
            {
                emailCC.Replace(emailTo, "");
            }
            lstEmailCC = emailCC.Split(';').Distinct().ToList();
            for (int i = 0; i < lstEmailCC.Count; i++)
            {
                if (i > 1)
                {
                    emailCC = emailCC + ";";
                }
                emailCC = emailCC + lstEmailCC[i];
            }
            string template = emailTemplate.Html;
            template = template.Replace("{{date}}", DateTime.Today.ToShortDateString());
            template = template.Replace("{{username}}", separation.EmployeeProfile.FirstName + " " + separation.EmployeeProfile.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(separation.EmployeeProfile.ID));
            template = template.Replace("{{SeparationMessage}}", body);
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + separation.EmployeeProfile.ImagePath);



            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = emailFrom,
                EmailTo = emailTo,
                EmailCC = emailCC,
                //EmailCC = emailToCC + ";" + emailCC,
                Subject = subject,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        public string GetDeptName(int roleID)
        {

            if (roleID == 12 || roleID == 24 || roleID == 38)
                return "HR";
            else if (roleID == 27 || roleID == 35)
                return "RMG";
            else if (roleID == 0)
                return "Exit Process Manager";
            else if (roleID == 25 || roleID == 37)
                return "IT";
            else if (roleID == 23 || roleID == 33)
                return "Finance";
            else if (roleID == 22 || roleID == 31 || roleID == 1)
                return "Admin";
            else if (roleID == 28 || roleID == 34)
                return "Internal";
            else if (roleID == 29 || roleID == 32)
                return "CQ";
            else if (roleID == 30 || roleID == 36)
                return "VWR";
            else if (roleID == 41 || roleID == 42)
                return "V2Hub";
            else
                return "";
        }

        //To get all dept wise ID
        void GetEmailToCCForAllDept(out string emailCC)
        {
            emailCC = string.Empty;
            using (PhoenixEntities entites = new PhoenixEntities())
            {
                var deptID = entites.HelpDeskCategories.Where(x => x.AssignedRole != 29 && x.AssignedRole != 41 && x.AssignedRole != 12 && x.AssignedRole != 24
                && x.Prefix != "HRGROUP" && x.AssignedRole != 27 && x.Prefix != "SeparationGroup" && x.AssignedRole != 28).ToList();
                foreach (var item in deptID)
                {
                    emailCC = emailCC + ";" + item.EmailGroup;
                }
            }
        }

        public void InitiateSeparationReminderEmails(PhoenixEntities context, IList<SeperationProcess> separationprocess)
        {
            try
            {
                string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                string emailTo = string.Empty;
                string emailCC = string.Empty;
                string subject = string.Empty;
                string separationMessage = string.Empty;
                string _hrseparation = string.Empty;
                Person reporting = null;
                string reportingManager = "";
                string employeeEmail = "";
                string exitProcessManager = "";
                string grpHeadEmail = "";
                string projectReportingManagerEmail = "";
                List<string> lstEmailCC = new List<string>();
                string strDistinctEmailCC = "";
                string strEmailDelimiter = Convert.ToString(ConfigurationManager.AppSettings["EmailDelimiter"]);
                GetEmailToCCForAllDept(out _hrseparation);

                var templateType = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SeparationReminderMail));
                var templateContent = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SeparationReminderContent));
                var template = context.EmailTemplate.Where(x => x.TemplateFor == templateContent).FirstOrDefault().Html;
                int sID = separationprocess[0].SeperationID.Value;

                var _separationData = context.Separation.Where(x => x.ID == sID).ToList();
                string DeptName = string.Empty;
                string templateData = "";
                var templateBody = context.EmailTemplate.Where(x => x.TemplateFor == templateContent).FirstOrDefault().Html;
                var mainTemplate = context.EmailTemplate.Where(x => x.TemplateFor == templateType).FirstOrDefault().Html;
                string baseUrl = Convert.ToString(ConfigurationManager.AppSettings["baseUrl"]);

                foreach (var pendingApproval in separationprocess.OrderBy(x => x.SeperationID))
                {
                    //var template = _service.First<EmailTemplate>(x => x.TemplateFor == templateType).Html;
                    DeptName = GetDeptName(pendingApproval.RoleID.Value);
                    string _status = string.Empty;

                    if (pendingApproval.StatusID == 1)
                        _status = "Completed";
                    else if (pendingApproval.StatusID == 0)
                        _status = "Pending";

                    templateData = templateData + template;
                    templateData = templateData.Replace("{{deptName}}", DeptName);
                    templateData = templateData.Replace("{{status}}", _status);
                }


                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    int EPM = _separationData[0].ApprovalID;
                    reporting = entites.People.Where(x => x.ID == EPM).FirstOrDefault();
                    exitProcessManager = reporting.PersonEmployment.First().OrganizationEmail;
                    reporting = null;

                    employeeEmail = _separationData[0].Person1.PersonEmployment.First().OrganizationEmail;
                    int PId = _separationData[0].PersonID;

                    var _reportingManager = entites.PersonReporting.Where(x => x.PersonID == PId && x.Active == true).ToList();
                    int reportingTo = Convert.ToInt32(_reportingManager[0].ReportingTo);
                    reporting = entites.People.Where(x => x.ID == reportingTo).FirstOrDefault();
                    reportingManager = reporting.PersonEmployment.First().OrganizationEmail;
                    reporting = null;
                    int separationPersonId = _separationData[0].PersonID;
                    List<PMSResourceAllocation> projectReportingManager = entites.PMSResourceAllocation.Where(x => x.PersonID == separationPersonId && x.IsDeleted == false).ToList() as List<PMSResourceAllocation>;
                    for (int i = 0; i < projectReportingManager.Count; i++)
                    {
                        int personReportingToId = projectReportingManager[i].ReportingTo;
                        if (i > 0)
                        {
                            projectReportingManagerEmail = ";";
                        }
                        projectReportingManagerEmail = entites.PersonEmployment.Where(x => x.PersonID == personReportingToId).Select(y => y.OrganizationEmail).FirstOrDefault();
                    }
                    grpHeadEmail = QueryHelper.GetGroupHeadEmail(Convert.ToInt32(_separationData[0].PersonID), 6).organizationemail;
                }
                string hrmanager = GetHRGroupForSeparation();
                emailCC = hrmanager + _hrseparation + ";" + reportingManager + ";" + exitProcessManager + ";" + projectReportingManagerEmail;
                if (!string.IsNullOrEmpty(emailTo))
                {
                    if (emailCC.Contains(emailTo))
                    {
                        emailCC.Replace(emailTo, "");
                    }
                }
                lstEmailCC = emailCC.Split(';').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + strEmailDelimiter;// Need to read delemeter key from web config
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                mainTemplate = mainTemplate.Replace("{{imagename}}", baseUrl + _separationData[0].Person1.Image);
                mainTemplate = mainTemplate.Replace("{{username}}", _separationData[0].Person1.FirstName + " " + _separationData[0].Person1.LastName);
                mainTemplate = mainTemplate.Replace("{{employeeid}}", _separationData[0].Person1.ID.ToString());
                mainTemplate = mainTemplate.Replace("{{ApprovalDate}}", Convert.ToDateTime(_separationData[0].ApprovalDate).ToString("dd MMMM,yyyy"));
                mainTemplate = mainTemplate.Replace("{{content}}", templateData);
                mainTemplate = mainTemplate.Replace("{{loggedinuser}}", "Vibrant Desk");

                subject = "Department clearance status for " + _separationData[0].Person1.FirstName + " " + _separationData[0].Person1.LastName + "(" + _separationData[0].PersonID + ")";

                bool isCreated = service.Create<Emails>(new Emails
                {
                    Content = mainTemplate,
                    Date = DateTime.Now,
                    EmailFrom = ConfigurationManager.AppSettings["helpdeskEmailId"].ToString(),
                    EmailTo = employeeEmail,    ///Email address of logged in user reporting manager. except for last finance stage
                    EmailCC = strDistinctEmailCC,
                    //EmailCC = hrmanager + _hrseparation + ";" + reportingManager + ";" + exitProcessManager,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);
            }
            catch
            {

            }
        }

        public void SendExpenseApprovalEmail(Expense_New expense, Person person, ApprovalStage stage, string comments)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;
            int emailTempalteType = 0;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ExpenseApproval))).First();

            string template = emailTemplate.Html;
            var allStatements = JsonConvert.DeserializeObject<ApprovalStatements>(emailTemplate.OptionalVariables);
            var allSubjects = JsonConvert.DeserializeObject<ApprovalStatements>(emailTemplate.Subjects);
            Person reporting = null;

            switch (stage)
            {
                case ApprovalStage.Submitted:
                    {
                        statement = allStatements.submitted;
                        subject = allSubjects.submitted;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == expense.PrimaryApprover).FirstOrDefault();
                            reportingManager = reporting.PersonEmployment.First().OrganizationEmail;
                            emailActionId = Convert.ToString(expense.ExpenseId);
                            emailTempalteType = Convert.ToInt32(EnumHelpers.EmailTemplateType.ExpenseApproval);
                        }
                        break;
                    }
                case ApprovalStage.PrimaryApprover:
                    {
                        statement = allStatements.primaryApprover;
                        subject = allSubjects.primaryApprover;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            reporting = entites.People.Where(x => x.ID == expense.PrimaryApprover).FirstOrDefault();
                        }
                        break;
                    }
                case ApprovalStage.SecondaryApprover:
                    {
                        statement = allStatements.secondaryApprover;
                        subject = allSubjects.secondaryApprover;
                        using (PhoenixEntities entites = new PhoenixEntities())
                        {
                            int roleId = Convert.ToInt32(ConfigurationManager.AppSettings["FinanceRoleId"]);
                            reportingManager = populateReportingManager(QueryHelper.GetFinanceEmail(roleId));
                            reporting = entites.PersonInRole.Where(x => x.RoleID == roleId).FirstOrDefault().Person;
                        }
                        emailCC = expense.Person.PersonEmployment.FirstOrDefault().OrganizationEmail;
                        //if (reportingManager.ToLower() == personCC.ToLower())
                        //emailCC = personCC;

                        break;
                    }
                case ApprovalStage.ModuleAdmin:
                    {
                        statement = allStatements.finance;
                        subject = allSubjects.finance;
                        reportingManager = expense.Person.PersonEmployment.FirstOrDefault().OrganizationEmail;
                        reporting = expense.Person;
                        break;
                    }
                case ApprovalStage.Rejected:
                    {
                        statement = allStatements.rejected;
                        subject = allSubjects.rejected;
                        reportingManager = expense.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = expense.Person;
                        break;
                    }
                case ApprovalStage.OnHold:
                    {
                        statement = allStatements.onHold;
                        subject = allSubjects.onHold;
                        reportingManager = expense.Person.PersonEmployment.First().OrganizationEmail;
                        reporting = expense.Person;
                        break;
                    }
            }

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{statements}}", statement);
            template = template.Replace("{{username}}", expense.Person.FirstName + " " + expense.Person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(expense.Person.ID));
            template = template.Replace("{{toname}}", reporting.FirstName + " " + reporting.LastName);   // logged in user reporting manager.
            template = template.Replace("{{statusfrom}}", "Open");
            template = template.Replace("{{statusto}}", "Assigned");
            template = template.Replace("{{formname}}", Convert.ToString(expense.ExpenseId));
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
            template = template.Replace("{{comments}}", comments);
            template = template.Replace("{{requestedby}}", expense.Person.FirstName + " " + expense.Person.LastName);
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + expense.Person.Image);
            template = template.Replace("{{requestid}}", Convert.ToString(expense.ExpenseId));
            template = template.Replace("{{seatlocation}}", expense.Person.PersonEmployment.First().SeatingLocation);
            template = template.Replace("{{extension}}", expense.Person.PersonEmployment.First().OfficeExtension);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                EmailTo = reportingManager,    ///Email address of logged in user reporting manager. except for last finance stage
                Subject = subject.Replace("{{formname}}", Convert.ToString(expense.ExpenseId)),
                EmailCC = emailCC,
                //EmailAction = emailActionId,
                //TemplateType = emailTempalteType
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        string GetHRGroupForSeparation()
        {
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            string hrGroupRoleId = (ConfigurationManager.AppSettings["HRGroupForSeparation"]).ToString();
            emails = context.HelpDeskCategories.Where(t => t.Prefix == hrGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }

        public bool SendExpenseReminder(ExpenseMail expReminder)
        {
            string template = string.Empty;
            if (expReminder.moduleType == 6) // 6 for travel
            {
                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    string strFromPersonId = Regex.Match(expReminder.fromMail, @"\d+").Value;
                    int fromPersonId = Int32.Parse(strFromPersonId);
                    expReminder.fromMail = entites.PersonEmployment.Where(p => p.PersonID == fromPersonId).Select(e => e.OrganizationEmail).FirstOrDefault();
                    string strToPersonId = Regex.Match(expReminder.toMail, @"\d+").Value;
                    int toPersonId = Int32.Parse(strToPersonId);
                    expReminder.toMail = entites.PersonEmployment.Where(p => p.PersonID == toPersonId).Select(e => e.OrganizationEmail).FirstOrDefault();
                }
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    template = context.EmailTemplate.Where(p => p.TemplateFor == "Pending Travel Request").Select(p => p.Html).FirstOrDefault();

                    var requestType = context.TravelDetails.Where(t => t.TravelId.ToString() == expReminder.expenseID).Select(t => t.RequestType).FirstOrDefault();
                    template = template.Replace("{{requestType}}", EnumExtensions.GetEnumDescription((EnumHelpers.RequestType)requestType));

                    var tripType = context.TravelDetails.Where(t => t.TravelId.ToString() == expReminder.expenseID).Select(t => t.TripType).FirstOrDefault();
                    template = template.Replace("{{tripType}}", EnumExtensions.GetEnumDescription((EnumHelpers.TripType)tripType));

                    var travelType = context.TravelDetails.Where(t => t.TravelId.ToString() == expReminder.expenseID).Select(t => t.TravelType).FirstOrDefault();
                    template = template.Replace("{{travelType}}", EnumExtensions.GetEnumDescription((EnumHelpers.TravelType)travelType));
                }
            }
            else if (expReminder.moduleType == 3) // 3 for expense
            {
                template = @"<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office""><head><meta charset=""UTF-8""><meta http-equiv=""X-UA-Compatible"" content=""IE=edge""><meta name=""viewport"" content=""width=device-width, initial-scale=1""><title>Expense Reminder</title><style type=""text/css""> p{margin: 10px 0; padding: 0}table{border-collapse: collapse; mso-table-lspace: 0; mso-table-rspace: 0}a img, img{border: 0; height: auto; outline: 0; text-decoration: none}#bodyCell, #bodyTable, body{height: 100%; margin: 0; padding: 0; width: 100%}img{-ms-interpolation-mode: bicubic}a, blockquote, li, p, td{mso-line-height-rule: exactly}a, blockquote, body, li, p, table, td{-ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%}.templateContainer{max-width: 600px!important}.mcnImage{vertical-align: bottom}.mcnTextContent{word-break: break-word}#templateHeader{background-image: none; background-repeat: no-repeat; background-position: 50% 50%; background-size: cover; border-top: 0; border-bottom: 0; padding-top: 15px; padding-bottom: 15px}#templateBody, .headerContainer{background-image: none; background-repeat: no-repeat; background-position: center; background-size: cover; border-top: 0; border-bottom: 0}.headerContainer{background-color: transparent; padding-top: 0; padding-bottom: 0}#templateBody{background-color: #fff; padding-top: 20px; padding-bottom: 20px}#templateFooter, .bodyContainer, .footerContainer{background-image: none; background-repeat: no-repeat; background-position: center; background-size: cover; border-top: 0; border-bottom: 0; padding-top: 0; padding-bottom: 0}.bodyContainer{background-color: transparent}.bodyContainer .mcnTextContent, .bodyContainer .mcnTextContent p{color: grey; font-family: Arial, Arial, Helvetica, sans-serif, sans-serif; font-size: 16px; line-height: 100%; text-align: left}.bodyContainer .mcnTextContent a, .bodyContainer .mcnTextContent p a{color: #00ADD8; font-weight: 400; text-decoration: underline}#templateFooter, .footerContainer{background-color: #333}.footerContainer.mcnTextContent, .footerContainer .mcnTextContent p{color: #FFF; font-family: Arial, Arial, Helvetica, sans-serif, sans-serif; font-size: 12px; line-height: 150%; text-align: center}@media only screen and (min-width:768px){.templateContainer{width: 600px!important}#mainBody{max-width: auto !important;}}@media only screen and (max-width: 480px){a, blockquote, body, li, p, table, td{-webkit-text-size-adjust: none!important}body{width: 100%!important; min-width: 100%!important}.mcnImage{width: 100%!important}.mcnBoxedTextContentContainer, .mcnCaptionBottomContent, .mcnCaptionLeftImageContentContainer, .mcnCaptionLeftTextContentContainer, .mcnCaptionRightImageContentContainer, .mcnCaptionRightTextContentContainer, .mcnCaptionTopContent, .mcnCartContainer, .mcnImageCardLeftTextContentContainer, .mcnImageCardRightTextContentContainer, .mcnImageGroupContentContainer, .mcnRecContentContainer, .mcnTextContentContainer{max-width: 100%!important; width: 100%!important}.mcnBoxedTextContentContainer{min-width: 100%!important}.mcnBoxedTextContentColumn, .mcnTextContent{padding-right: 18px!important; padding-left: 18px!important}.mcnBoxedTextContentContainer .mcnTextContent, .mcnBoxedTextContentContainer.mcnTextContent p{font-size: 14px!important; line-height: 150%!important}.bodyContainer .mcnTextContent, .bodyContainer.mcnTextContent p{font-size: 16px!important; line-height: 150%!important}.footerContainer .mcnTextContent, .footerContainer.mcnTextContent p{font-size: 14px!important; line-height: 150%!important}#mainBody{max-width: 100% !important; width: 100% !important;}}</style></head><body><center><table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" height=""100%"" width=""100%"" id=""bodyTable""><tbody><tr><td align=""center"" valign=""top"" id=""bodyCell""><table bgcolor=""#eaeaea"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background:#eaeaea;""><tbody><tr><td align=""center"" valign=""top"" id=""templateHeader"" data-template-container=""""><img align=""center"" alt="""" src=""https://my.v2solutions.com/assets/images/v2-logo.png"" width=""165"" style=""max-width:165px; padding-bottom: 0; display: inline !important; vertical-align: bottom;"" class=""mcnImage""></td></tr><tr><td bgcolor=""#f9f9f9"" align=""center"" valign=""top"" id=""templateBody"" style=""background:#f9f9f9;"" data-template-container=""""><table bgcolor=""#ffffff"" width=""580"" id=""mainBody"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width: 580px; max-width:580px; margin:0 auto;background-color: #ffffff;""><tbody><tr><td bgcolor=""#ffffff"" valign=""top"" class=""bodyContainer"" style=""background-color: #ffffff;""><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" class=""mcnBoxedTextBlock"" style=""min-width:100%;""><tbody class=""mcnBoxedTextBlockOuter""><tr><td valign=""top"" class=""mcnBoxedTextBlockInner""><table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""min-width:100%;"" class=""mcnBoxedTextContentContainer""><tbody><tr><td style=""padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:18px;""><table border=""0"" cellpadding=""18"" cellspacing=""0"" class=""mcnTextContentContainer"" width=""100%"" style=""min-width: 100% !important;background-color: #2d81af;""><tbody><tr><td valign=""top"" class="""" style=""color: #ffffff;font-family: Arial, Helvetica, sans-serif;font-size: 14px;font-weight: normal;text-align: left;""><span class=""null"" style=""text-align: right;color: #ffffff;font-size: 16px;font-weight: bold;"">Expense</span></td><td valign=""top"" class="""" style=""color: #ffffff;font-family: Arial, Helvetica, sans-serif;font-size: 14px;font-weight: normal;text-align: right;""><span class=""null"" style=""text-align: right;color: #ffffff;font-size: 16px;font-weight: bold;"">{{date}}</span></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" class=""mcnCaptionBlock""><tbody class=""mcnCaptionBlockOuter""><tr><td class=""mcnCaptionBlockInner"" valign=""top"" style=""padding:9px;""><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnCaptionBottomContent"" width=""false"" align=""center""><tbody><tr><td class=""mcnCaptionBottomImageContent"" align=""center"" valign=""top"" style=""padding:0 9px 9px 9px;""><img alt="""" src=""{{imagename}}"" width=""85"" style=""max-width:85px;"" class=""mcnImage""></td></tr><tr><td class=""mcnTextContent"" valign=""top"" style=""padding: 0px 9px;color: #000000;font-style: normal;font-weight: bold;"" width=""564""><div style=""text-align: left;""><div style=""font-size: 14px; text-align: center;padding: 3px 0px;""><span style=""font-family: Arial, Helvetica, sans-serif;color:#2d81af;font-size: 16px;"">{{username}}</span></div><div style=""font-size: 14px; text-align: center;""><span style=""font-family: Arial, Helvetica, sans-serif;color:#2d81af;font-size: 16px;"">{{employeeid}}</span></div></div></td></tr></tbody></table></td></tr></tbody></table><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnDividerBlock"" style=""min-width:100%;"" width=""100%""><tbody class=""mcnDividerBlockOuter""><tr><td class=""mcnDividerBlockInner"" style=""min-width: 100%; padding: 2px 18px;""><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnDividerContent"" style=""min-width: 100%;border-top: 2px solid #EAEAEA;"" width=""100%""><tbody><tr><td><span></span></td></tr></tbody></table></td></tr></tbody></table><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnTextBlock"" style=""min-width:100%;"" width=""100%""><tbody class=""mcnTextBlockOuter""><tr><td class=""mcnTextBlockInner"" style=""padding-top:9px;"" valign=""top""><table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnTextContentContainer"" style=""max-width:100%;min-width:100%;/* line-height: 28%; */"" width=""100%""><tbody><tr><td class=""mcnTextContent"" style=""padding-top:0;padding-right:18px;padding-bottom:9px;padding-left:18px;line-height: 150%;/* font-weight: bold; */font-size: 14px;"" valign=""top"">{{statement}}</td></tr><tr><td class=""mcnTextContent"" style=""padding-top:0;padding-right:18px;padding-bottom:9px;padding-left:18px;line-height: 150%;"" valign=""top""><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnTextBlock"" style=""min-width:100%;"" width=""100%""><tbody class=""mcnTextBlockOuter""><tr><td width=""50%"" align=""left"" class=""mcnTextBlockInner"" style=""padding-top:10px; padding-right:5px; padding-bottom:0px; padding-left:0px;"" valign=""top""><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnTextContentContainer"" style=""border-collapse: separate !important;""><tbody><tr><td class=""mcnTextContent"" style=""padding-top:0;padding-right:10px;padding-bottom:9px;padding-left:0px;line-height: 150%;font-size: 11px;"" valign=""top""><span style=""/* font-weight: bold; *//* color: #2d81af; */font-size: 14px;"">{{expenseid}}</span><br>Expense ID </td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td class=""mcnTextContent"" style=""padding-top:0;padding-right:18px;padding-bottom:24px;padding-left:18px;line-height: 150%;font-size: 11px;"" valign=""top""> Description <br><span style=""/* font-weight: bold; *//* color: #2d81af; */font-size: 14px;"">{{description}}</span></td></tr><tr><td class=""mcnTextContent"" style=""padding-top:0;padding-right:18px;padding-bottom:9px;padding-left:18px;line-height: 150%;font-size: 11px;"" valign=""top""> Regards <br><span style=""/* font-weight: bold; *//* color: #2d81af; */font-size: 14px;"">{{loggedinuser}}</span></td></tr></tbody></table></td></tr></tbody></table><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnDividerBlock"" style=""min-width:100%;"" width=""100%""><tbody class=""mcnDividerBlockOuter""><tr><td class=""mcnDividerBlockInner"" style=""min-width: 100%; padding: 2px 18px;""><table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""mcnDividerContent"" style=""min-width: 100%;border-top: 2px solid #EAEAEA;"" width=""100%""><tbody><tr><td><span></span></td></tr></tbody></table></td></tr></tbody></table><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" class=""mcnBoxedTextBlock"" style=""min-width:100%;""><tbody class=""mcnBoxedTextBlockOuter""><tr><td valign=""top"" class=""mcnBoxedTextBlockInner""><table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""min-width:100%;"" class=""mcnBoxedTextContentContainer""><tbody><tr><td style=""padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:18px;""><table border=""0"" cellpadding=""18"" cellspacing=""0"" class=""mcnTextContentContainer"" width=""100%"" style=""min-width: 100% !important;background-color: #F7F7F7;""><tbody><tr><td valign=""top"" class=""mcnTextContent"" style=""line-height: 100%; text-align: center;""><div style=""text-align: left;""><span style=""font-size:12px"">Note: <br>*&nbsp;This is a system generated email. Please do not reply. <br>*&nbsp;If you have any queries contact <a href=""mailto:helpdesk@v2solutions.com"">Helpdesk</a></span></div></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr><tr><td align=""center"" valign=""top"" id=""templateFooter"" data-template-container=""""><table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" class=""templateContainer""><tbody><tr><td valign=""top"" class=""mcnTextContent mcnTextContentContainer mcnTextBlockInner"" style=""padding-top: 9px;padding-right:18px;padding-bottom:9px;padding-left:18px;font-style: normal;font-size: 12px;font-family: Arial, Helvetica, sans-serif;font-size: 12px;line-height: 150%;""><p style=""text-align: center;""><span style=""color:#808080"">© 2018 V2Solutions, Inc</span></p></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";
                template = template.Replace("{{statement}}", "Action pending for expense reimbursement request raised by you.");
            }
            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + expReminder.empId + ".jpg");
            template = template.Replace("{{username}}", expReminder.userName);
            template = template.Replace("{{employeeid}}", expReminder.empId);

            if (expReminder.moduleType == 6)
                template = template.Replace("{{expenseid}}", "TR"+expReminder.expenseID);
            else
                template = template.Replace("{{expenseid}}", expReminder.expenseID);
            template = template.Replace("{{description}}", expReminder.body);
            template = template.Replace("{{loggedinuser}}", expReminder.loggedInuser);
            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = expReminder.fromMail,
                EmailTo = expReminder.toMail,
                Subject = expReminder.subject,
                EmailCC = expReminder.ccMail
            }, e => e.Id == 0);

            if (isCreated)
            {
                service.Finalize(true);
                return true;
            }
            else
            {
                return false;
            }

        }

        #region Customer
        public bool SendCustomerMails(CustomerMailAction action, Customer customer, int userID)
        {
            bool isSent = false;

            switch (action)
            {
                case CustomerMailAction.Creation:
                    CustomerCreationEmail(customer, userID);
                    break;
                case CustomerMailAction.Update:
                    CustomerUpdateEmail(customer, userID);
                    break;
                case CustomerMailAction.CustomerContractEndDatereminder:
                    CustomerContractEndDatereminderEmail(customer, userID);
                    break;
            }

            return isSent;
        }

        #region Customer Private Methods
        private bool CustomerCreationEmail(Customer customer, int userID)
        {
            try
            {

                bool isCreated;
                string statement = string.Empty;
                string subject = string.Empty;
                string mailTo = GetFinanceGroupEmailIds();
                string emailCC = GetPMOGroupEmailIds() + "," + GetRMGGroupEmailIds() + "," + GetApprover(userID);
                string emailActionId = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.CustomerCreation))).First();

                string template = emailTemplate.Html;
                var person = service.First<Person>(x => x.ID == userID);

                subject = emailTemplate.Subjects.Replace("{{customername}}", customer.Name).Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
                template = template.Replace("{{customername}}", customer.Name);
                template = template.Replace("{{customercode}}", customer.CustomerCode);
                if (customer.CustomerContract.FirstOrDefault() != null)
                {
                    template = template.Replace("{{contracttype}}", this.GetTextForCustomerContractType(customer.CustomerContract.FirstOrDefault().ContractType ?? 0));
                }
                else
                {
                    template = template.Replace("{{contracttype}}", "");
                }
                template = template.Replace("{{contractstartdate}}", customer.ContractEffectiveDate.ToStandardDate());
                template = template.Replace("{{contractenddate}}", customer.ValidTill.ToStandardDate());
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);



                //emailCC = emailCC;
                if (emailCC.Contains(mailTo))
                {
                    emailCC = emailCC.Replace(mailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = mailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    Subject = subject,
                    EmailCC = strDistinctEmailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetTextForCustomerContractType(int contractType)
        {
            switch (contractType)
            {
                case 1:
                    return "MSA";
                case 2:
                    return "SOW";
                case 3:
                    return "MOM";
                case 4:
                    return "Email";
                case 5:
                    return "Others";
                default:
                    return string.Empty;
            }
        }

        private bool CustomerUpdateEmail(Customer customer, int userID)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.UpdateCustomer))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userID);

            subject = emailTemplate.Subjects.Replace("{{customername}}", customer.Name).Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
            template = template.Replace("{{customername}}", customer.Name);
            template = template.Replace("{{customercode}}", customer.CustomerCode);
            if (customer.CustomerContract.FirstOrDefault() != null)
            {
                template = template.Replace("{{contracttype}}", this.GetTextForCustomerContractType(customer.CustomerContract.FirstOrDefault().ContractType ?? 0));
            }
            else
            {
                template = template.Replace("{{contracttype}}", "");
            }
            template = template.Replace("{{contractsigningdate}}", customer.ContractEffectiveDate.ToStandardDate());
            template = template.Replace("{{contractvaliditydate}}", customer.ValidTill.ToStandardDate());
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = reportingManager,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool CustomerContractEndDatereminderEmail(Customer customer, int userID)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.CustomerContractEndDatereminder))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userID);

            subject = emailTemplate.Subjects;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
            template = template.Replace("{{customername}}", customer.Name);
            template = template.Replace("{{customercode}}", customer.CustomerCode);
            if (customer.CustomerContract.FirstOrDefault() != null)
            {
                template = template.Replace("{{contracttype}}", this.GetTextForCustomerContractType(customer.CustomerContract.FirstOrDefault().ContractType ?? 0));
            }
            else
            {
                template = template.Replace("{{contracttype}}", "");
            }
            template = template.Replace("{{contractsigningdate}}", customer.ContractEffectiveDate.ToStandardDate());
            template = template.Replace("{{contractvaliditydate}}", customer.ValidTill.ToStandardDate());
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = reportingManager,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #endregion// Customer region ends

        #region Project
        public bool SendProjectMails(ProjectMailAction action, ProjectList project, int userID)
        {
            bool isSent = false;

            switch (action)
            {
                case ProjectMailAction.Creation:
                    isSent = ProjectCreationEmail(project, userID);
                    break;
                case ProjectMailAction.Update:
                    isSent = ProjectUpdateEmail(project, userID);
                    break;
                case ProjectMailAction.ProjectEndDatereminder:
                    isSent = ProjectEndDatereminderEmail(project, userID);
                    break;
            }

            return isSent;
        }



        #region Project Private Methods

        private bool ProjectCreationEmail(ProjectList project, int userID)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(project.DeliveryManager);
            string emailCC = GetPMOGroupEmailIds() + "," + GetFinanceGroupEmailIds() + "," + GetRMGGroupEmailIds() + "," + GetApprover(userID);
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ProjectCreation))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userID);
            using (var _db = _phoenixEntity)
            {
                var projectDetails = (from p in _db.ProjectList
                                      join c in _db.Customer on p.CustomerID equals c.ID
                                      where p.ID == project.ID
                                      select new
                                      {
                                          projectName = p.ProjectName,
                                          startDate = p.ActualStartDate,
                                          endDate = p.ActualEndDate,
                                          parentprojectName = _db.ProjectList.Where(x => x.ID == p.ParentProjId).Select(x => x.ProjectName).FirstOrDefault(),
                                          customer = c.Name
                                      }).First();

                subject = emailTemplate.Subjects.Replace("{{projectname}}", projectDetails.projectName).Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
                template = template.Replace("{{projectname}}", projectDetails.projectName);
                template = template.Replace("{{startdate}}", projectDetails.startDate.ToStandardDate());
                template = template.Replace("{{enddate}}", projectDetails.endDate.ToStandardDate());
                template = template.Replace("{{parentproject}}", projectDetails.parentprojectName);
                template = template.Replace("{{customername}}", projectDetails.customer);
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
            }

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ProjectUpdateEmail(ProjectList project, int userID)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ProjectUpdate))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userID);


            using (var _db = _phoenixEntity)
            {
                var projectDetails = (from p in _db.ProjectList
                                      join c in _db.Customer on p.CustomerID equals c.ID
                                      where p.ID == project.ID
                                      select new
                                      {
                                          projectName = p.ProjectName,
                                          startDate = p.ActualStartDate,
                                          endDate = p.ActualEndDate,
                                          parentproject = p.ParentProjId,
                                          customer = c.Name
                                      }).First();

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
                template = template.Replace("{{projectname}}", projectDetails.projectName);
                template = template.Replace("{{startdate}}", projectDetails.startDate.ToStandardDate());
                template = template.Replace("{{enddate}}", projectDetails.endDate.ToStandardDate());
                template = template.Replace("{{parentproject}}", Convert.ToString(projectDetails.parentproject ?? 0));
                template = template.Replace("{{customer}}", projectDetails.customer);
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
            }


            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = reportingManager,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ProjectEndDatereminderEmail(ProjectList project, int userID)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ProjectEndDatereminder))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userID);


            using (var _db = _phoenixEntity)
            {
                var projectDetails = (from p in _db.ProjectList
                                      join c in _db.Customer on p.CustomerID equals c.ID
                                      where p.ID == project.ID
                                      select new
                                      {
                                          projectName = p.ProjectName,
                                          startDate = p.ActualStartDate,
                                          endDate = p.ActualEndDate,
                                          parentproject = p.ParentProjId,
                                          customer = c.Name
                                      }).First();

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
                template = template.Replace("{{projectname}}", projectDetails.projectName);
                template = template.Replace("{{startdate}}", projectDetails.startDate.ToStandardDate());
                template = template.Replace("{{enddate}}", projectDetails.endDate.ToStandardDate());
                template = template.Replace("{{parentproject}}", Convert.ToString(projectDetails.parentproject ?? 0));
                template = template.Replace("{{customer}}", projectDetails.customer);
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
            }


            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = reportingManager,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        #endregion//Project region ends

        #region Invoice
        public bool SendInvoiceMails(InvoiceMailAction action, PMSInvoice model, bool isApprover, int userId)
        {
            bool isSent = false;

            switch (action)
            {
                case InvoiceMailAction.Submitted:
                    isSent = InvoiceSubmittedtionEmail(model, isApprover, userId);
                    break;
                case InvoiceMailAction.Approved:
                    isSent = InvoiceApprovedEmail(model, isApprover, userId);
                    break;
                case InvoiceMailAction.Rejected:
                    isSent = InvoiceRejectedEmail(model, isApprover, userId);
                    break;
                case InvoiceMailAction.OnHold:
                    isSent = InvoiceOnHoldEmail(model, isApprover, userId);
                    break;
            }

            return isSent;
        }



        #region Invoice Private Methods

        private bool InvoiceSubmittedtionEmail(PMSInvoice model, bool isApprover, int userId)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = string.Empty;
            if (isApprover)
            {
                emailTo = GetFinanceGroupEmailIds();
            }
            else
            {
                emailTo = GetProjectGroupHeadEmails(model.Project);
            }
            string[] emailsToList = emailTo.Split(',').Select(sValue => sValue.Trim()).ToArray();

            emailTo = emailsToList[0];
            string emailCC = GetApprover(model.RaisedBy);

            StringBuilder sb = new StringBuilder();

            if (emailsToList.Length > 1)
            {
                for (int i = 1; i < emailsToList.Length; i++)
                {
                    if (emailsToList[i].Trim() != "")
                    {
                        sb.Append(",");
                        sb.Append(emailsToList[i]);
                    }
                }
                emailCC = emailCC + sb.ToString();
            }

            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.MangersubmittedIR))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);
            var raisedBy = service.First<Person>(x => x.ID == model.RaisedBy);
            var project = service.First<ProjectList>(x => x.ID == model.Project);
            var customer = service.First<Customer>(x => x.ID == project.CustomerID);
            var invoiceSalesPeriod = service.First<InvoiceSalesPeriod>(x => x.Id == model.SalesPeriod);

            subject = emailTemplate.Subjects.Replace("{{irid}}", "").Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", raisedBy.FirstName + " " + raisedBy.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(raisedBy.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + raisedBy.Image);
            template = template.Replace("{{irid}}", string.Concat("#", model.Id));
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{projectname}}", project.ProjectName);
            template = template.Replace("{{customername}}", customer.Name);
            template = template.Replace("{{salesperiod}}", string.Concat(invoiceSalesPeriod.Month, "/", invoiceSalesPeriod.Year));
            template = template.Replace("{{amount}}", Convert.ToString(model.TotalAmt));

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool InvoiceApprovedEmail(PMSInvoice model, bool isFinanceApprover, int userId)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = string.Empty;
            string emailCC = string.Empty;
            if (isFinanceApprover)
            {
                emailTo = GetApprover(model.RaisedBy);
                emailCC = GetProjectGroupHeadEmails(model.Project) + "," + GetFinanceGroupEmailIds();
            }
            else
            {
                emailTo = GetFinanceGroupEmailIds();
                emailCC = GetProjectGroupHeadEmails(model.Project) + "," + GetApprover(model.RaisedBy);
            }

            string emailActionId = string.Empty;
            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.IRApproves))).First();
            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);
            var raisedBy = service.First<Person>(x => x.ID == model.RaisedBy);
            var project = service.First<ProjectList>(x => x.ID == model.Project);
            var customer = service.First<Customer>(x => x.ID == project.CustomerID);
            var invoiceSalesPeriod = service.First<InvoiceSalesPeriod>(x => x.Id == model.SalesPeriod);

            subject = emailTemplate.Subjects.Replace("{{irid}}", "").Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", raisedBy.FirstName + " " + raisedBy.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(raisedBy.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + raisedBy.Image);
            template = template.Replace("{{irid}}", string.Concat("#", model.Id));
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{projectname}}", project.ProjectName);
            template = template.Replace("{{customername}}", customer.Name);
            template = template.Replace("{{salesperiod}}", string.Concat(invoiceSalesPeriod.Month, "/", invoiceSalesPeriod.Year));
            template = template.Replace("{{amount}}", Convert.ToString(model.TotalAmt));

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool InvoiceRejectedEmail(PMSInvoice model, bool isFinanceApprover, int userId)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(model.RaisedBy);
            string emailCC = string.Empty;
            if (isFinanceApprover)
            {
                emailCC = GetProjectGroupHeadEmails(model.Project) + "," + GetFinanceGroupEmailIds();
            }
            else
            {
                emailCC = GetProjectGroupHeadEmails(model.Project);
            }
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.IRRejects))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);
            var raisedBy = service.First<Person>(x => x.ID == model.RaisedBy);

            var project = service.First<ProjectList>(x => x.ID == model.Project);
            var customer = service.First<Customer>(x => x.ID == project.CustomerID);
            var invoiceSalesPeriod = service.First<InvoiceSalesPeriod>(x => x.Id == model.SalesPeriod);

            subject = emailTemplate.Subjects.Replace("{{irid}}", "").Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", raisedBy.FirstName + " " + raisedBy.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(raisedBy.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + raisedBy.Image);
            template = template.Replace("{{irid}}", string.Concat("#", model.Id));
            template = template.Replace("{{comments}}", model.Comment);
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{projectname}}", project.ProjectName);
            template = template.Replace("{{customername}}", customer.Name);
            template = template.Replace("{{salesperiod}}", string.Concat(invoiceSalesPeriod.Month, "/", invoiceSalesPeriod.Year));
            template = template.Replace("{{amount}}", Convert.ToString(model.TotalAmt));

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool InvoiceOnHoldEmail(PMSInvoice model, bool isFinanceApprover, int userId)
        {
            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(model.RaisedBy);
            string emailCC = null;
            if (isFinanceApprover)
            {
                emailCC = GetProjectGroupHeadEmails(model.Project) + "," + GetFinanceGroupEmailIds();
            }
            else
            {
                emailCC = GetProjectGroupHeadEmails(model.Project);
            }
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.IROnHold))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);
            var raisedBy = service.First<Person>(x => x.ID == model.RaisedBy);

            var project = service.First<ProjectList>(x => x.ID == model.Project);
            var customer = service.First<Customer>(x => x.ID == project.CustomerID);
            var invoiceSalesPeriod = service.First<InvoiceSalesPeriod>(x => x.Id == model.SalesPeriod);

            subject = emailTemplate.Subjects.Replace("{{irid}}", "").Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", raisedBy.FirstName + " " + raisedBy.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(raisedBy.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + raisedBy.Image);
            template = template.Replace("{{irid}}", string.Concat("#", model.Id));
            template = template.Replace("{{comments}}", model.Comment);
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{projectname}}", project.ProjectName);
            template = template.Replace("{{customername}}", customer.Name);
            template = template.Replace("{{salesperiod}}", string.Concat(invoiceSalesPeriod.Month, "/", invoiceSalesPeriod.Year));
            template = template.Replace("{{amount}}", Convert.ToString(model.TotalAmt));

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion
        #endregion//Invoice region ends


        #region Resource Allocation

        public bool SendResourceAllocationRaisedEmail(RARaisedRequest model, int userId)
        {
            try
            {
                //Todo: Project requestFor properly for email attachment with paramerter.
                string attachments = string.Empty;

                string statement = string.Empty;
                string subject = string.Empty;
                string emailTo = GetRMGGroupEmailIds();
                string emailCC = GetApprover(model.Request.RequestedBy);
                string emailActionId = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";
                bool isCreated;

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RARequestRaised))).First();

                string template = emailTemplate.Html;
                var person = service.First<Person>(x => x.ID == userId);

                subject = emailTemplate.Subjects.Replace("{{requesttype}}", GetRequestTypeText(model.Request.RequestType)).Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
                template = template.Replace("{{requesttype}}", GetRequestTypeText(model.Request.RequestType));
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

                attachments = GetResourceAllocationRaisedEmailAttachments(model);
                //emailCC = emailCC +"," + projectMngrs;
                emailCC = emailCC;
                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    Subject = subject,
                    EmailCC = strDistinctEmailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SendResourceAllocationUpdatedEmail(RAGetRaisedRequest model, int userId)
        {
            string attachments = string.Empty;

            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RARequestUpdated))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);

            subject = emailTemplate.Subjects.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
            template = template.Replace("{{requestnumber}}", model.ID.ToString());
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            attachments = GetResourceAllocationRequestUpdatedEmailAttachments(model);

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = reportingManager,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SendResourceAllocationActionEmail(RAGetRaisedRequest model, int userId, string comments)
        {
            try
            {
                string attachments = string.Empty;

                string statement = string.Empty;
                string subject = string.Empty;
                string emailTo = GetApprover(model.RequestedBy);
                string emailCC = GetRMGGroupEmailIds();
                string emailActionId = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RAAction))).First();

                string template = emailTemplate.Html;
                var person = service.First<Person>(x => x.ID == userId);
                bool isCreated;
                subject = emailTemplate.Subjects.Replace("{{requesttype}}", GetRequestTypeText(model.RequestType));

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
                template = template.Replace("{{requestnumber}}", Convert.ToString(model.ID));
                template = template.Replace("{{requeststatus}}", GetRequestStatusText(model.Status));
                template = template.Replace("{{comments}}", comments);
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

                attachments = GetResourceAllocationActionsAttachments(model);
                emailCC = emailCC;
                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    Subject = subject,
                    EmailCC = strDistinctEmailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SendResourceAllocationActionStatusEmail(int userId, int requestedBy, int requestId, int requestType, int requestStatus, string comments)
        {
            string attachments = string.Empty;

            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(requestedBy);
            string emailCC = GetRMGGroupEmailIds();
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RAAction))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);

            subject = emailTemplate.Subjects.Replace("{{requesttype}}", GetRequestTypeText(requestType));

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{requestnumber}}", Convert.ToString(requestId));
            template = template.Replace("{{requeststatus}}", GetRequestStatusText(requestStatus));
            template = template.Replace("{{comments}}", comments);
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            //attachments = GetResourceAllocationActionsAttachments(model);

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ResourceAllocationEndReminderEmail(RARaisedRequest model, int userId)
        {
            string attachments = string.Empty;

            string statement = string.Empty;
            string subject = string.Empty;
            string reportingManager = string.Empty;
            string emailCC = null;
            string emailActionId = string.Empty;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RAEndDateReminder))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == userId);

            subject = emailTemplate.Subjects.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
            template = template.Replace("{{employeeid}}", Convert.ToString(person.ID));
            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + person.Image);
            template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);

            attachments = GetResourceAllocationEndDateReminderEmailAttachments(model);

            bool isCreated;
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = reportingManager,
                    Subject = subject,
                    EmailCC = emailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public bool ResourceAllocationAllocationUpdateEmail(RAGetRaisedRequest model, int userId, string comments)
        //{
        //    bool isSend = false;
        //    if (model.RequestType == 1)
        //    {
        //        foreach (var resource in model.RANewRequest)
        //        {
        //            using (PhoenixEntities dbContext = new PhoenixEntities())
        //            {
        //                isSend = EmployeeUpdateEmail(userId, resource.EmpID, model.ProjectName, resource.ProjectReporting, resource.ProjectReportingName, resource.RoleName, resource.BillabilityName,
        //                                       resource.StartDate, resource.EndDate, resource.Allocation, comments);
        //            }
        //        }
        //    }
        //    else if (model.RequestType == 2)
        //    {
        //        foreach (var resource in model.RAUpdateRequest)
        //        {
        //            using (PhoenixEntities dbContext = new PhoenixEntities())
        //            {
        //                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
        //                                            where x.ProjectID == model.ProjectID && x.PersonID == resource.EmpID && x.IsDeleted == false
        //                                            select x).FirstOrDefault();
        //                string projectReportingName = dbContext.People.Where(x => x.ID == ca.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //                string roleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == ca.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault();
        //                string billabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == ca.BillbleType).Select(x => x.Discription).FirstOrDefault();
        //                isSend = EmployeeUpdateEmail(userId, resource.EmpID, model.ProjectName, ca.ReportingTo, projectReportingName, roleName, billabilityName,
        //                    ca.FromDate, ca.ToDate, ca.percentage, comments);
        //            }
        //        }
        //    }
        //    else if (model.RequestType == 3)
        //    {
        //        foreach (var resource in model.RAExtentionRequest)
        //        {
        //            using (PhoenixEntities dbContext = new PhoenixEntities())
        //            {
        //                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
        //                                            where x.ProjectID == model.ProjectID && x.PersonID == resource.EmpID && x.IsDeleted == false
        //                                            select x).FirstOrDefault();
        //                string projectReportingName = dbContext.People.Where(x => x.ID == ca.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //                string roleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == ca.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault();
        //                string billabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == ca.BillbleType).Select(x => x.Discription).FirstOrDefault();
        //                isSend = EmployeeUpdateEmail(userId, resource.EmpID, model.ProjectName, ca.ReportingTo, projectReportingName, roleName, billabilityName,
        //                    ca.FromDate, ca.ToDate, ca.percentage, comments);
        //            }
        //        }
        //    }
        //    else if (model.RequestType == 4)
        //    {
        //        foreach (var resource in model.RAReleaseRequest)
        //        {
        //            using (PhoenixEntities dbContext = new PhoenixEntities())
        //            {
        //                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
        //                                            where x.ProjectID == model.ProjectID && x.PersonID == resource.EmpID && x.IsDeleted == false
        //                                            select x).FirstOrDefault();
        //                string projectReportingName = dbContext.People.Where(x => x.ID == ca.ReportingTo).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //                string roleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == ca.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault();
        //                string billabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == ca.BillbleType).Select(x => x.Discription).FirstOrDefault();
        //                isSend = EmployeeUpdateEmail(userId, resource.EmpID, model.ProjectName, ca.ReportingTo, projectReportingName, roleName, billabilityName,
        //                    ca.FromDate, ca.ToDate, ca.percentage, comments);
        //            }
        //        }
        //    }
        //    return isSend;
        //}

        public bool EmployeeUpdateEmail(RAResource model)
        {
            try
            {
                string statement = string.Empty;
                string subject = string.Empty;
                //string reportingManager = string.Empty;
                string emailTo = GetApprover(model.EmpID);
                string reportingMngr = GetApprover(model.ProjectReporting);
                string orgReportingMngr = GetOrgReportingTo(model.EmpID);
                string emailCC = GetRMGGroupEmailIds();
                string EmailIT = GetITEmail();
                //string projectMngr = GetProjectManager(projectID);
                string emailActionId = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";
                bool isCreated;

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RAAlloocationUpdate))).First();

                string template = emailTemplate.Html;
                var person = service.First<Person>(x => x.ID == model.StatusBy);
                var resource = service.First<Person>(x => x.ID == model.EmpID);
                var reportingTo = service.First<Person>(x => x.ID == model.ProjectReporting);

                using (var _db = _phoenixEntity)
                {
                    subject = emailTemplate.Subjects;

                    template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                    template = template.Replace("{{username}}", resource.FirstName + " " + resource.LastName);
                    template = template.Replace("{{employeeid}}", Convert.ToString(resource.ID));
                    template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + resource.Image);
                    template = template.Replace("{{projectname}}", GetProjectName(model.ProjectID));
                    template = template.Replace("{{reportingto}}", reportingTo.FirstName + " " + reportingTo.LastName);
                    template = template.Replace("{{projectrole}}", GetProjectRole(model.ProjectRole));
                    template = template.Replace("{{billability}}", GetProjectBillability(model.Billability));
                    template = template.Replace("{{allocationstartdate}}", model.StartDate.ToShortDateString());
                    template = template.Replace("{{allocationenddate}}", model.EndDate.ToShortDateString());
                    template = template.Replace("{{allocated}}", string.Concat(model.Allocation, "%"));
                    template = template.Replace("{{comments}}", model.Comments);
                    template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
                }
                //if (model.ProjectRole == 1 || model.ProjectRole == 2 || model.ProjectRole == 3)
                //{
                emailCC = emailCC + "," + reportingMngr + "," + orgReportingMngr + "," + GetProjectMngrEmails(model.ProjectID) + "," + EmailIT;
                //}
                //else
                //{
                //    emailCC = emailCC + "," + reportingMngr + "," + orgReportingMngr;
                //}              
                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    Subject = subject,
                    EmailCC = strDistinctEmailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SendBGVerificationToHR(RAResource model, List<int> RequiredBGCList)
        {
            try
            {
                string emailTo = GetHRGroupEmailIds();
                string emailCC = GetRMGGroupEmailIds();
                bool isCreated;
                string strRequiredBGCList = string.Empty;
                string strCompletedBGParameter = string.Empty;
                List<int> CompletedBGParameter = new List<int>();
                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.BGUPdatetoHR))).First();

                string template = emailTemplate.Html;
                string subject = emailTemplate.Subjects;
                var person = service.First<Person>(x => x.ID == model.StatusBy);
                var resource = service.First<Person>(x => x.ID == model.EmpID);

                using (var _db = _phoenixEntity)
                {
                    subject = subject.Replace("{{username}}", resource.FirstName + " " + resource.LastName);
                    subject = subject.Replace("{{employeeid}}", Convert.ToString(resource.ID));

                    template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                    template = template.Replace("{{EmployeeName}}", resource.FirstName + " " + resource.LastName);
                    template = template.Replace("{{EmployeeCode}}", Convert.ToString(resource.ID));
                    template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + resource.Image);
                    template = template.Replace("{{projectname}}", GetProjectName(model.ProjectID));
                    template = template.Replace("{{allocationstartdate}}", model.StartDate.ToShortDateString());
                    template = template.Replace("{{allocationenddate}}", model.EndDate.ToShortDateString());
                    using (PhoenixEntities dbContext = new PhoenixEntities())
                    {
                        List<int> projectBGParameters = (from m in dbContext.CustomerBGMapping
                                                         where m.CustomerID == (from c in dbContext.ProjectList where c.ID == model.ProjectID select c.CustomerID).FirstOrDefault()
                                                         select m.BGParameterID).ToList();

                        List<int> resourceBGParameters = (from m in dbContext.PersonBGMapping
                                                          where m.PersonID == resource.ID && m.BGStatus == 2
                                                          select m.BGParameterID).ToList();

                        CompletedBGParameter = projectBGParameters.Where(p => resourceBGParameters.Any(p2 => p2 == p)).ToList();
                    }

                    foreach (var item in CompletedBGParameter)
                    {
                        strCompletedBGParameter = strCompletedBGParameter + "<li>" + GetBGParameterName(item) + "</li>";
                    }
                    template = template.Replace("{{CompletedBGParameter}}", strCompletedBGParameter);

                    foreach (var item in RequiredBGCList)
                    {
                        strRequiredBGCList = strRequiredBGCList + "<li>" + GetBGParameterName(item) + "</li>";
                    }
                    template = template.Replace("{{RequiredBGParameter}}", strRequiredBGCList);
                    template = template.Replace("{{LoggedInUser}}", person.FirstName + " " + person.LastName);
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = emailTo,
                    Subject = subject,
                    EmailCC = emailCC
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public bool SendBGVerificationToRMG(int StatusBy, int ProjectID, int PersonID)
        //{
        //    try
        //    {
        //        string emailTo = GetRMGGroupEmailIds();
        //        string emailCC = GetHRGroupEmailIds();
        //        bool isCreated;
        //        string BGParameter = string.Empty;
        //        string BGSTatus = string.Empty;
        //        var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.BGUPdatetoRMG))).First();
        //        string template = emailTemplate.Html;
        //        string subject = emailTemplate.Subjects;
        //        var person = service.First<Person>(x => x.ID == StatusBy);
        //        var resource = service.First<Person>(x => x.ID == PersonID);

        //        using (var _db = _phoenixEntity)
        //        {
        //            PMSResourceAllocation currentAllocation = _db.PMSResourceAllocation.Where(x => x.PersonID == resource.ID && x.ProjectID == ProjectID).FirstOrDefault();
        //            subject = subject.Replace("{{username}}", resource.FirstName + " " + resource.LastName);
        //            subject = subject.Replace("{{employeeid}}", Convert.ToString(resource.ID));

        //            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
        //            template = template.Replace("{{EmployeeName}}", resource.FirstName + " " + resource.LastName);
        //            template = template.Replace("{{EmployeeCode}}", Convert.ToString(resource.ID));
        //            template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + resource.Image);
        //            template = template.Replace("{{projectname}}", GetProjectName(currentAllocation.ProjectID));
        //            template = template.Replace("{{allocationstartdate}}", currentAllocation.FromDate.ToShortDateString());
        //            template = template.Replace("{{allocationenddate}}", currentAllocation.ToDate.ToShortDateString());

        //            List<int> projectBGParameters = (from m in _db.CustomerBGMapping
        //                                             where m.CustomerID == (from c in _db.ProjectList where c.ID == ProjectID select c.CustomerID).FirstOrDefault()
        //                                             select m.BGParameterID).ToList();
        //            foreach (var item in projectBGParameters)
        //            {
        //                BGParameter = BGParameter + "<li>" + GetBGParameterName(item) + ": Completed </li>";
        //            }
        //            template = template.Replace("{{BGParameterWithStatus}}", BGParameter);
        //            template = template.Replace("{{LoggedInUser}}", person.FirstName + " " + person.LastName);
        //        }

        //        isCreated = service.Create<Emails>(new Emails
        //        {
        //            Content = template,
        //            Date = DateTime.Now,
        //            EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
        //            EmailTo = emailTo,
        //            Subject = subject,
        //            EmailCC = emailCC
        //        }, e => e.Id == 0);

        //        if (isCreated)
        //            service.Finalize(true);

        //        return isCreated;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private string GetOrgReportingTo(int personId)
        {
            string orgReportingTo = string.Empty;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int orgReportingToId = dbContext.PersonReporting.Where(x => x.PersonID == personId && x.Active == true).Select(x => x.ReportingTo).FirstOrDefault();
                orgReportingTo = GetApprover(orgReportingToId); //dbContext.PersonEmployment.Where(x => x.PersonID == personId).Select(x => x.OrganizationEmail).FirstOrDefault();
            }
            return orgReportingTo;
        }

        private string GetProjectName(int? projectId)
        {
            string projectName = string.Empty;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                projectName = dbContext.ProjectList.Where(x => x.ID == projectId).Select(x => x.ProjectName).FirstOrDefault();
            }
            return projectName;
        }
        private string GetBGParameterName(int bgID)
        {
            string BGName = string.Empty;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                BGName = dbContext.BGParameterList.Where(x => x.ID == bgID).Select(x => x.Name).FirstOrDefault();
            }
            return BGName;
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

        private string GetProjectBillability(int billability)
        {
            string projectRole = string.Empty;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                projectRole = dbContext.PMSAllocationBillableType.Where(x => x.ID == billability).Select(x => x.Discription).FirstOrDefault();
            }
            return projectRole;
        }

        //public bool SendResourceAllocationReleaseEmail(RAGetRaisedRequest model, int userId, string comments)
        //{
        //    bool isSend = false;
        //    if (model.RequestType == 4)
        //    {
        //        using (PhoenixEntities dbContext = new PhoenixEntities())
        //        {
        //            foreach (var resource in model.RAReleaseRequest)
        //            {

        //                int reportingTo = (from x in dbContext.PMSResourceAllocation
        //                                   where x.ProjectID == model.ProjectID && x.PersonID == resource.EmpID && x.IsDeleted == false
        //                                   select x).OrderByDescending(d => d.ModifyDate).FirstOrDefault().ReportingTo;
        //                isSend = EmployeeReleaseEmail(userId, resource.EmpID, model.ProjectName, reportingTo, resource.EndDate, comments);
        //            }
        //        }
        //    }
        //    return isSend;
        //}

        public bool EmployeeReleaseEmail(RAResource model)
        {
            string attachments = string.Empty;

            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(model.EmpID);
            string reportingMngr = GetApprover(model.ProjectReporting);
            string orgReportingMngr = GetOrgReportingTo(model.EmpID);
            string emailCC = GetRMGGroupEmailIds();
            string emailActionId = string.Empty;
            List<String> lstEmailCC = new List<string>();
            List<String> lstEmailTO = new List<string>();
            string strDistinctEmailCC = "";
            string strDistinctEmailTo = "";
            string EmailIT = GetITEmail();
            bool isCreated;


            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RARelease))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == model.StatusBy);
            var resource = service.First<Person>(x => x.ID == model.EmpID);

            subject = emailTemplate.Subjects;
            using (var _db = _phoenixEntity)
            {
                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", resource.FirstName + " " + resource.LastName);
                template = template.Replace("{{employeeid}}", Convert.ToString(resource.ID));
                template = template.Replace("{{imageurl}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + resource.Image);
                template = template.Replace("{{projectname}}", GetProjectName(model.ProjectID));
                template = template.Replace("{{releasedate}}", (model.ActionDate.Value.AddDays(-1).Date).ToStandardDate());
                template = template.Replace("{{comments}}", model.Comments);
                template = template.Replace("{{loggedinuser}}", person.FirstName + " " + person.LastName);
            }
            //if (model.ProjectRole == 1 || model.ProjectRole == 2)
            //{
            emailCC = emailCC + "," + reportingMngr + "," + orgReportingMngr + "," + GetProjectMngrEmails(model.ProjectID) + "," + EmailIT;
            //}
            //else
            //{
            //    emailCC = emailCC + "," + reportingMngr + "," + orgReportingMngr;
            //}
            if (emailCC.Contains(emailTo))
            {
                emailCC = emailCC.Replace(emailTo, "");
            }
            lstEmailCC = emailCC.Split(',').Distinct().ToList();
            for (int i = 0; i < lstEmailCC.Count; i++)
            {
                if (i > 0)
                {
                    strDistinctEmailCC = strDistinctEmailCC + ",";
                }
                strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
            }
            strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
            lstEmailTO = emailTo.Split(',').Distinct().ToList();
            for (int i = 0; i < lstEmailTO.Count; i++)
            {
                if (i > 0)
                {
                    strDistinctEmailTo = strDistinctEmailTo + ",";
                }
                strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
            }
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo.Replace(@"old_", string.Empty), // Old is remove to handle seperation cases 
                    Subject = subject,
                    EmailCC = strDistinctEmailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Resource Allocation Private Methods


        string GetRequestTypeText(int requestType)
        {
            switch (requestType)
            {
                case 1:
                    return "New allocation";
                case 2:
                    return "Update allocation";
                case 3:
                    return "Extention";
                case 4:
                    return "Release";
            }
            return string.Empty;
        }

        string GetRequestStatusText(int requestType)
        {
            switch (requestType)
            {
                case 0:
                    return "Pending";
                case 1:
                    return "Approved";
                case 2:
                    return "Reject";
            }
            return string.Empty;
        }

        #region Resource allocation Email Attachments

        private string GetResourceAllocationRaisedEmailAttachments(RARaisedRequest model)
        {
            string fileName = string.Empty;
            if (model.Request.RequestType == 1)
            {
                fileName = "V2Solutions_VW_NewAllocation_Request";
            }
            if (model.Request.RequestType == 2)
            {
                fileName = "V2Solutions_VW_UpdateAllocation_Request";
            }
            if (model.Request.RequestType == 3)
            {
                fileName = "V2Solutions_VW_Extension_Request";
            }
            if (model.Request.RequestType == 4)
            {
                fileName = "V2Solutions_VW_Release_Request";
            }
            // return FormatCSVBodyForApprovalActions(fileName, model);
            return FormatCSVBody(fileName, model);
        }

        private string GetResourceAllocationRequestUpdatedEmailAttachments(RAGetRaisedRequest model)
        {
            string fileName = string.Empty;
            if (model.RequestType == 1)
            {
                fileName = FormatCSVBodyForNewApprovalActions("V2Solutions_VW_NewAllocation_Request", model);
            }
            if (model.RequestType == 2)
            {
                fileName = FormatCSVBodyForUpdateApprovalActions("V2Solutions_VW_UpdateAllocation_Request", model);
            }
            if (model.RequestType == 3)
            {
                fileName = FormatCSVBodyForExtensionApprovalActions("V2Solutions_VW_Extension_Request", model);
            }
            if (model.RequestType == 4)
            {
                fileName = FormatCSVBodyForReleaseApprovalActions("V2Solutions_VW_Release_Request", model);
            }
            return fileName;
        }

        private string GetResourceAllocationEndDateReminderEmailAttachments(RARaisedRequest model)
        {
            return FormatCSVBody("RAEndDateReminder", model);
        }

        private string GetResourceAllocationActionsAttachments(RAGetRaisedRequest model)
        {
            string attachement = string.Empty;
            if (model.RequestType == 1)
            {
                attachement = FormatCSVBodyForNewApprovalActions("V2Solutions_VW_NewAllocation_Request", model);
            }
            if (model.RequestType == 2)
            {
                attachement = FormatCSVBodyForUpdateApprovalActions("V2Solutions_VW_UpdateAllocation_Request", model);
            }
            if (model.RequestType == 3)
            {
                attachement = FormatCSVBodyForExtensionApprovalActions("V2Solutions_VW_Extension_Request", model);
            }
            if (model.RequestType == 4)
            {
                attachement = FormatCSVBodyForReleaseApprovalActions("V2Solutions_VW_Release_Request", model);
            }
            return attachement;
        }

        //private string FormatCSVBody(string contentPrefix, RARaisedRequest model)
        //{
        //    PhoenixEntities dbContext = new PhoenixEntities();
        //    string fileName = string.Concat(contentPrefix, "_", model.Request.ID, ".csv");
        //    string filePath = string.Concat(GetFolderPath(), fileName);
        //    StringBuilder stringBuilder = new StringBuilder();
        //    stringBuilder.AppendLine("Request ID, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments, RMGComments");
        //    foreach (var resource in model.Resource)
        //    {
        //        resource.FullName = dbContext.People.Where(x => x.ID == resource.Id).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //        resource.BillabilityName = dbContext.PMSAllocationBillableType.Where(x => x.ID == resource.Billability).Select(x => x.Discription).FirstOrDefault();
        //        resource.ProjectReportingName = dbContext.People.Where(x => x.ID == resource.ProjectReporting).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
        //        resource.RoleName = dbContext.PMSRoles.Where(x => x.PMSRoleID == resource.ProjectRole).Select(x => x.PMSRoleDescription).FirstOrDefault();
        //        if (model.Request.IsRmg)
        //        {
        //            stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
        //            model.Request.ID,
        //            GetRequestTypeText(model.Request.RequestType),
        //            resource.Id,
        //            resource.FullName,
        //            resource.StartDate,
        //            resource.EndDate,
        //            resource.ProjectReporting,
        //            resource.ProjectReportingName,
        //            resource.RoleName,
        //            resource.BillabilityName,
        //            resource.Allocation,
        //            GetRequestStatusText(model.Request.Status),
        //            model.RequestDetail[0].Comments,
        //            model.RequestDetail[0].RMGComments
        //            ));
        //        }
        //        else
        //        {
        //            stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
        //            model.Request.ID,
        //            GetRequestTypeText(model.Request.RequestType),
        //            resource.Id,
        //            resource.FullName,
        //            resource.StartDate,
        //            resource.EndDate,
        //            resource.ProjectReporting,
        //            resource.ProjectReportingName,
        //            resource.RoleName,
        //            resource.BillabilityName,
        //            resource.Allocation,
        //            GetRequestStatusText(model.Request.Status),
        //            model.RequestDetail[0].Comments
        //            ));
        //        }
        //    }

        //    this.WriteFileToCSV(filePath, stringBuilder.ToString());

        //    return fileName;
        //}

        private string FormatCSVBody(string contentPrefix, RARaisedRequest model)
        {
            bool isCsvCreated = false;
            string fileName = string.Concat(contentPrefix, "_", model.Request.ID, ".csv");
            string filePath = string.Concat(GetFolderPath(), fileName);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(BindHeadersForCsv(model.Request.RequestType, model.Request.IsRmg));
            foreach (var resource in model.Resource)
            {
                isCsvCreated = BindRequestCsvContent(model, resource, stringBuilder, filePath);
            }
            return fileName;
        }

        private string BindHeadersForCsv(int requestType, bool isRMG)
        {
            if (isRMG)
            {
                return ("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments, RMGComments");
            }
            else
            {
                return ("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments");
            }
        }

        private bool BindRequestCsvContent(RARaisedRequest model, RAResource resource, StringBuilder stringBuilder, string filePath)
        {
            bool isCsvCreated = false;
            try
            {
                var emp = service.First<Person>(x => x.ID == resource.Id);
                var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);
                if (model.Request.IsRmg)
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    emp.FirstName + " " + emp.LastName,
                    resource.StartDate,
                    resource.EndDate,
                    resource.ProjectReporting,
                    reportingTo.FirstName + " " + reportingTo.LastName,
                    GetProjectRole(resource.ProjectRole),
                    GetProjectBillability(resource.Billability),
                    resource.Allocation,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments,
                    model.RequestDetail[0].RMGComments
                    ));
                }
                else
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    emp.FirstName + " " + emp.LastName,
                    resource.StartDate,
                    resource.EndDate,
                    resource.ProjectReporting,
                    reportingTo.FirstName + " " + reportingTo.LastName,
                    GetProjectRole(resource.ProjectRole),
                    GetProjectBillability(resource.Billability),
                    resource.Allocation,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments
                    ));
                }
                this.WriteFileToCSV(filePath, stringBuilder.ToString());
                isCsvCreated = true;
            }
            catch
            {
                isCsvCreated = false;
            }
            return isCsvCreated;
        }

        private bool BindUpdateRequestCsvContent(RARaisedRequest model, RAResource resource, StringBuilder stringBuilder, string filePath)
        {
            bool isCsvCreated = false;
            PhoenixEntities dbContext = new PhoenixEntities();
            // PMSResourceAllocation currentAllocation = dbContext.PMSResourceAllocation.Where(pra => pra.PersonID == resource.Id && pra.ProjectID == model.Request.ProjectID);                                     
            try
            {
                if (model.Request.IsRmg)
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    resource.FullName,
                    resource.StartDate,
                    resource.EndDate,
                    resource.ProjectReporting,
                    resource.ProjectReportingName,
                    resource.RoleName,
                    resource.BillabilityName,
                    resource.Allocation,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments,
                    model.RequestDetail[0].RMGComments
                    ));
                }
                else
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    resource.FullName,
                    resource.StartDate,
                    resource.EndDate,
                    resource.ProjectReporting,
                    resource.ProjectReportingName,
                    resource.RoleName,
                    resource.BillabilityName,
                    resource.Allocation,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments
                    ));
                }
                this.WriteFileToCSV(filePath, stringBuilder.ToString());
                isCsvCreated = true;
            }
            catch
            {
                isCsvCreated = false;
            }
            return isCsvCreated;

        }

        private bool BindExtensionRequestCsvContent(RARaisedRequest model, RAResource resource, StringBuilder stringBuilder, string filePath)
        {
            bool isCsvCreated = false;
            try
            {
                if (model.Request.IsRmg)
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    resource.FullName,
                    resource.EndDate,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments,
                    model.RequestDetail[0].RMGComments
                    ));
                }
                else
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    resource.FullName,
                    resource.EndDate,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments
                    ));
                }
                this.WriteFileToCSV(filePath, stringBuilder.ToString());
                isCsvCreated = true;
            }
            catch
            {
                isCsvCreated = false;
            }
            return isCsvCreated;

        }

        private bool BindReleaseRequestCsvContent(RARaisedRequest model, RAResource resource, StringBuilder stringBuilder, string filePath)
        {
            bool isCsvCreated = false;
            try
            {
                if (model.Request.IsRmg)
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    resource.FullName,
                    resource.EndDate,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments,
                    model.RequestDetail[0].RMGComments
                    ));
                }
                else
                {
                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    model.Request.ID,
                    GetRequestTypeText(model.Request.RequestType),
                    resource.Id,
                    resource.FullName,
                    resource.EndDate,
                    GetRequestStatusText(model.Request.Status),
                    model.RequestDetail[0].Comments
                    ));
                }
                this.WriteFileToCSV(filePath, stringBuilder.ToString());
                isCsvCreated = true;
            }
            catch
            {
                isCsvCreated = false;
            }
            return isCsvCreated;

        }

        private string FormatCSVBodyForNewApprovalActions(string contentPrefix, RAGetRaisedRequest model)
        {
            string fileName = string.Concat(contentPrefix, "_", model.ID, ".csv");
            string filePath = string.Concat(GetFolderPath(), fileName);
            StringBuilder stringBuilder = new StringBuilder();

            if (model.IsRmg)
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments, RMGComments");
                foreach (var resource in model.RANewRequest)
                {
                    // var person = service.First<Person>(x => x.ID == model.StatusBy);
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole),
                        GetProjectBillability(resource.Billability),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments,
                        resource.RMGComments
                        ));
                }
            }
            else
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments");
                foreach (var resource in model.RANewRequest)
                {
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole),
                        GetProjectBillability(resource.Billability),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments
                        ));
                }
            }
            this.WriteFileToCSV(filePath, stringBuilder.ToString());
            return fileName;
        }

        private string FormatCSVBodyForUpdateApprovalActions(string contentPrefix, RAGetRaisedRequest model)
        {
            string fileName = string.Concat(contentPrefix, "_", model.ID, ".csv");
            string filePath = string.Concat(GetFolderPath(), fileName);
            StringBuilder stringBuilder = new StringBuilder();

            if (model.IsRmg)
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments, RMGComments");
                foreach (var resource in model.RAUpdateRequest)
                {
                    // var person = service.First<Person>(x => x.ID == model.StatusBy);
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole ?? 0),
                        GetProjectBillability(resource.Billability ?? 0),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments,
                        resource.RMGComments
                        ));
                }
            }
            else
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments");
                foreach (var resource in model.RAUpdateRequest)
                {
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole ?? 0),
                        GetProjectBillability(resource.Billability ?? 0),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments
                        ));
                }
            }
            this.WriteFileToCSV(filePath, stringBuilder.ToString());
            return fileName;
        }

        private string FormatCSVBodyForExtensionApprovalActions(string contentPrefix, RAGetRaisedRequest model)
        {
            string fileName = string.Concat(contentPrefix, "_", model.ID, ".csv");
            string filePath = string.Concat(GetFolderPath(), fileName);
            StringBuilder stringBuilder = new StringBuilder();

            if (model.IsRmg)
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments, RMGComments");
                foreach (var resource in model.RAExtentionRequest)
                {
                    // var person = service.First<Person>(x => x.ID == model.StatusBy);
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole),
                        GetProjectBillability(resource.Billability),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments,
                        resource.RMGComments
                        ));
                }
            }
            else
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments");
                foreach (var resource in model.RAExtentionRequest)
                {
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole),
                        GetProjectBillability(resource.Billability),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments
                        ));
                }
            }
            this.WriteFileToCSV(filePath, stringBuilder.ToString());
            return fileName;
        }

        private string FormatCSVBodyForReleaseApprovalActions(string contentPrefix, RAGetRaisedRequest model)
        {
            string fileName = string.Concat(contentPrefix, "_", model.ID, ".csv");
            string filePath = string.Concat(GetFolderPath(), fileName);
            StringBuilder stringBuilder = new StringBuilder();

            if (model.IsRmg)
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments, RMGComments");
                foreach (var resource in model.RAReleaseRequest)
                {
                    // var person = service.First<Person>(x => x.ID == model.StatusBy);
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole),
                        GetProjectBillability(resource.Billability),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments,
                        resource.RMGComments
                        ));
                }
            }
            else
            {
                stringBuilder.AppendLine("Request ID, Request Type, Resource ID, Full Name, Start Date, End Date, Reporting To, Reporting To Name, Project Role, Billability, Allocation, Staus, Comments");
                foreach (var resource in model.RAReleaseRequest)
                {
                    var emp = service.First<Person>(x => x.ID == resource.EmpID);
                    var reportingTo = service.First<Person>(x => x.ID == resource.ProjectReporting);

                    stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                        resource.RequestID,
                        GetRequestTypeText(1),
                        resource.EmpID,
                        emp.FirstName + " " + emp.LastName,
                        resource.StartDate,
                        resource.EndDate,
                        resource.ProjectReporting,
                        reportingTo.FirstName + " " + reportingTo.LastName,
                        GetProjectRole(resource.ProjectRole),
                        GetProjectBillability(resource.Billability),
                        resource.Allocation,
                        GetRequestStatusText(resource.Status),
                        resource.Comments
                        ));
                }
            }
            this.WriteFileToCSV(filePath, stringBuilder.ToString());
            return fileName;
        }

        private void WriteFileToCSV(string filePath, string content)
        {
            System.IO.File.AppendAllText(filePath, content);
        }

        /// <summary>
        /// Get reports folder path
        /// </summary>
        /// <returns></returns>
        string GetFolderPath()
        {
            try
            {
                string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
                path = string.Concat(path, @"\allocation\");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion Email Attachments
        #endregion
        #endregion//Resource Allocation  region ends

        #region RRF
        public bool SendNewEmployeeEmail(CandidateToEmployee candidateToEmployee, TARRF modelTARRF = null, TARRFDetail modelTARRFDetail = null)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = string.Empty;
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strCandidateCCRecipients = "";
                string strDistinctEmailTo = "";

                if (string.IsNullOrEmpty(candidateToEmployee.RrfNumber))
                {
                    emailTo = GetRecruitmentTeamEmail();
                }
                else
                {
                    emailTo = GetPersonOrgEmail(candidateToEmployee.RrfRequestor);
                }
                for (int i = 0; i < candidateToEmployee.CandidateCCRecipients.Count; i++)
                {
                    strCandidateCCRecipients = strCandidateCCRecipients + GetPersonOrgEmail(candidateToEmployee.CandidateCCRecipients[i]) + ",";
                }
                GetEmailCCForAllDeptRRF(out emailCC);
                emailCC = emailCC + "," + "anjan.chatterjee@v2solutions.com, ob@v2solutions.com" + "," + GetPersonOrgEmail(candidateToEmployee.ReportingTo) + "," + GetPersonOrgEmail(candidateToEmployee.ExitProcessManager) + "," + strCandidateCCRecipients;
                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.NewEmployee))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects.Replace("{{EmployeeCode}}", Convert.ToString(candidateToEmployee.EmployeeCode));
                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RRFNO}}", candidateToEmployee.RrfNumber);
                template = template.Replace("{{EmployeeCode}}", Convert.ToString(candidateToEmployee.EmployeeCode));
                template = template.Replace("{{EmployeeName}}", candidateToEmployee.FirstName + " " + candidateToEmployee.LastName);
                template = template.Replace("{{EmploymentStatus}}", GetEmploymentStatus(candidateToEmployee.EmploymentStatus));
                template = template.Replace("{{ReportingTo}}", GetPersonName(candidateToEmployee.ReportingTo));
                template = template.Replace("{{ExitProcessManager}}", GetPersonName(candidateToEmployee.ExitProcessManager));
                template = template.Replace("{{DeliveryUnit}}", GetDeliveryUnit(candidateToEmployee.DeliveryUnit));
                template = template.Replace("{{EmployeeType}}", Convert.ToString(candidateToEmployee.EmployeeType));
                template = template.Replace("{{JoiningDate}}", candidateToEmployee.JoiningDate.Value.Date.ToString("M/d/yyyy"));
                template = template.Replace("{{ProbationReviewDate}}", Convert.ToString(candidateToEmployee.ProbationReviewDate));
                template = template.Replace("{{ReJoined}}", (Convert.ToBoolean(candidateToEmployee.RejoinedWithinYear) ? "Yes" : "No"));
                template = template.Replace("{{Designation}}", GetDesignation(candidateToEmployee.DesignationID));
                template = template.Replace("{{OfficeLocation}}", GetLocation(candidateToEmployee.OfficeLocation));
                template = template.Replace("{{WorkLocation}}", GetLocation(candidateToEmployee.WorkLocation));
                template = template.Replace("{{Skills}}", GetSkills(candidateToEmployee.CandidateSkillMappingVM));
                template = template.Replace("{{EmailID}}", candidateToEmployee.OrganizationEmail);
                var person = service.First<Person>(x => x.ID == candidateToEmployee.LoggedInUserID);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(candidateToEmployee.LoggedInUserID));


                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated && !string.IsNullOrEmpty(candidateToEmployee.RrfNumber) && modelTARRF != null && modelTARRFDetail != null)
                {
                    service.Finalize(true);
                    if (candidateToEmployee.IsRRFDesignationChanged)
                    {
                        isCreated = SendNewEmployeeDesignationChangeEmail(candidateToEmployee);
                    }
                    SendCloseRRFHREmail(modelTARRF, modelTARRFDetail, candidateToEmployee.FirstName + " " + candidateToEmployee.LastName, candidateToEmployee.DesignationID, candidateToEmployee.LoggedInUserID);
                }
                if (isCreated && string.IsNullOrEmpty(candidateToEmployee.RrfNumber))
                {

                    service.Finalize(true);
                }

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SendNewEmployeeDesignationChangeEmail(CandidateToEmployee candidateToEmployee)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = GetPersonOrgEmail(candidateToEmployee.RrfRequestor);
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                emailCC = GetRecruitmentTeamEmail() + "," + GetPersonOrgEmail(candidateToEmployee.ReportingTo) + "," + GetPersonOrgEmail(candidateToEmployee.ExitProcessManager);

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.JoiningUpdate))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects.Replace("{{EmployeeCode}}", GetPersonName(candidateToEmployee.EmployeeCode));

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RequestorName}}", GetPersonName(candidateToEmployee.RrfRequestor));
                template = template.Replace("{{RRFNo}}", candidateToEmployee.RrfNumber);
                template = template.Replace("{{EmployeeCode}}", Convert.ToString(candidateToEmployee.EmployeeCode));
                template = template.Replace("{{EmployeeName}}", candidateToEmployee.FirstName + " " + candidateToEmployee.LastName);
                template = template.Replace("{{CurrentDesignation}}", GetDesignation(candidateToEmployee.RRFRequestedDesignationID));
                template = template.Replace("{{RequestedDesignation}}", GetDesignation(candidateToEmployee.DesignationID));

                var person = service.First<Person>(x => x.ID == candidateToEmployee.LoggedInUserID);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(candidateToEmployee.LoggedInUserID));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SendNewRRFRequestEmail(TARRFViewModel model)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = GetPersonOrgEmail(model.PrimaryApprover);
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                emailCC = GetRecruitmentTeamEmail() + "," + GetPersonOrgEmail(model.CreatedBy);

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.SendForApproval))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects;

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{ApproverName}}", GetPersonName(model.PrimaryApprover));
                template = template.Replace("{{RRFNO}}", model.RRFNo);
                template = template.Replace("{{NoOfPosition}}", Convert.ToString(model.Position));
                template = template.Replace("{{Skills}}", GetRRFSkills(model.SkillIds));
                template = template.Replace("{{Designation}}", GetDesignation(model.Designation));
                template = template.Replace("{{Comments}}", Convert.ToString(model.RequestorComments));


                var person = service.First<Person>(x => x.ID == model.CreatedBy);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(model.CreatedBy));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SendApproveRRFEmail(TARRFViewModel model)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = GetRecruitmentTeamEmail();
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                emailCC = GetPersonOrgEmail(model.CreatedBy) + "," + GetPersonOrgEmail(model.PrimaryApprover);

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ApproveRRF))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo)).Replace("{{LogedInUser}}", GetPersonName(model.PrimaryApprover));

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RequestorName}}", GetPersonName(model.CreatedBy));
                template = template.Replace("{{RRFNO}}", model.RRFNo);
                template = template.Replace("{{NoOfPosition}}", Convert.ToString(model.Position));
                template = template.Replace("{{Skills}}", GetRRFSkills(model.SkillIds));
                template = template.Replace("{{Designation}}", GetDesignation(model.Designation));
                template = template.Replace("{{Comments}}", model.PrimaryApproverComments);


                var person = service.First<Person>(x => x.ID == model.PrimaryApprover);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(model.PrimaryApprover));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SendRejectRRFEmail(TARRFViewModel model)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = string.Empty;
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RejectRRF))).First();
                string template = emailTemplate.Html;
                if (model.HRApprover == 0)
                {
                    emailTo = GetRecruitmentTeamEmail();
                    emailCC = GetPersonOrgEmail(model.CreatedBy) + "," + GetPersonOrgEmail(model.PrimaryApprover);
                    subject = emailTemplate.Subjects.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo)).Replace("{{LogedInUser}}", GetPersonName(model.PrimaryApprover));
                }
                else
                {
                    emailTo = GetPersonOrgEmail(model.CreatedBy);
                    emailCC = GetRecruitmentTeamEmail() + "," + GetPersonOrgEmail(model.PrimaryApprover);
                    subject = emailTemplate.Subjects.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo)).Replace("{{LogedInUser}}", GetPersonName(model.HRApprover));
                }

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RequestorName}}", GetPersonName(model.CreatedBy));
                template = template.Replace("{{RRFNO}}", model.RRFNo);
                template = template.Replace("{{NoOfPosition}}", Convert.ToString(model.Position));
                template = template.Replace("{{Skills}}", GetRRFSkills(model.SkillIds));
                template = template.Replace("{{Designation}}", GetDesignation(model.Designation));
                template = template.Replace("{{Comments}}", model.PrimaryApproverComments);


                var person = service.First<Person>(x => x.ID == model.PrimaryApprover);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(model.PrimaryApprover));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool SendApprovedRRFHREmail(TARRFViewModel model)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = GetPersonOrgEmail(model.CreatedBy);
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                emailCC = GetRecruitmentTeamEmail();

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ApprovedRRFHR))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo)).Replace("{{LogedInUser}}", GetPersonName(model.HRApprover));

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RequestorName}}", GetPersonName(model.CreatedBy));
                template = template.Replace("{{RRFNO}}", model.RRFNo);
                template = template.Replace("{{NoOfPosition}}", Convert.ToString(model.Position));
                template = template.Replace("{{Skills}}", GetRRFSkills(model.SkillIds));
                template = template.Replace("{{Designation}}", GetDesignation(model.Designation));
                template = template.Replace("{{Expectedcloserdt}}", Convert.ToString(model.ExpectedClosureDate));
                template = template.Replace("{{SLA}}", GetSLA(model.SLA));
                template = template.Replace("{{Comments}}", model.HRApproverComments);


                var person = service.First<Person>(x => x.ID == model.HRApprover);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(model.HRApprover));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetSLA(int slaID)
        {
            switch (slaID)
            {
                case 1:
                    return "30";
                case 2:
                    return "45";
                case 3:
                    return "60";
                case 4:
                    return "90";
                case 5:
                    return "120";
                default:
                    return "";
            }

        }
        public bool SendCanceledRRFHREmail(TARRFDetailViewModel model, TARRF modeltarrf)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = GetPersonOrgEmail(Convert.ToInt32(modeltarrf.CreatedBy));
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                emailCC = GetRecruitmentTeamEmail() + "," + GetPersonOrgEmail(Convert.ToInt32(modeltarrf.PrimaryApprover));

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.CancelRRF))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo + "." + model.RRFNumber));

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RequestorName}}", GetPersonName(Convert.ToInt32(modeltarrf.CreatedBy)));
                template = template.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo + "." + model.RRFNumber));
                template = template.Replace("{{NoOfPosition}}", Convert.ToString(1));
                template = template.Replace("{{Skills}}", GetRRFSkills(modeltarrf.TASkills.Select(x => x.SkillId).ToList()));
                template = template.Replace("{{Designation}}", GetDesignation(modeltarrf.Designation));
                //template = template.Replace("{{Expectedcloserdt}}", Convert.ToString(modeltarrf.ExpectedClosureDate));
                //template = template.Replace("{{SLA}}", Convert.ToString(modeltarrf.SLA));
                template = template.Replace("{{Comments}}", model.comments);


                var person = service.First<Person>(x => x.ID == modeltarrf.HRApprover);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(Convert.ToInt32(modeltarrf.HRApprover)));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool SendCloseRRFHREmail(TARRF modeltarrf, TARRFDetail model, string candidateName, int? currentDesignation, int loggedInUser)
        {
            bool isCreated;
            try
            {
                string subject = string.Empty;
                string emailTo = GetPersonOrgEmail(Convert.ToInt32(modeltarrf.CreatedBy));
                string emailCC = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";


                emailCC = GetRecruitmentTeamEmail();

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.CloseRRF))).First();
                string template = emailTemplate.Html;
                subject = emailTemplate.Subjects.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo + "." + model.RRFNumber));

                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RequestorName}}", GetPersonName(Convert.ToInt32(modeltarrf.CreatedBy)));
                template = template.Replace("{{RRFNO}}", Convert.ToString(model.RRFNo + "." + model.RRFNumber));
                template = template.Replace("{{NoOfPosition}}", Convert.ToString(1));
                template = template.Replace("{{Skills}}", GetRRFSkills(modeltarrf.TASkills.Select(x => x.SkillId).ToList()));
                template = template.Replace("{{Designation}}", GetDesignation(currentDesignation));
                //template = template.Replace("{{Expectedcloserdt}}", Convert.ToString(modeltarrf.ExpectedClosureDate));
                //template = template.Replace("{{SLA}}", Convert.ToString(modeltarrf.SLA));
                template = template.Replace("{{CandidateName}}", candidateName);


                var person = service.First<Person>(x => x.ID == loggedInUser);
                template = template.Replace("{{LoggedInUser}}", GetPersonName(Convert.ToInt32(loggedInUser)));

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    EmailCC = strDistinctEmailCC,
                    Subject = subject,
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetPersonName(int ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.People.Where(pe => pe.ID == ID).Select(pe => pe.FirstName + " " + pe.LastName).FirstOrDefault();
            return Name;
        }

        private string GetPersonOrgEmail(int ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string email = context.PersonEmployment.Where(pe => pe.PersonID == ID).Select(pe => pe.OrganizationEmail).FirstOrDefault();
            return email;
        }
        private string GetEmploymentStatus(int? ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.EmploymentStatus.Where(pe => pe.Id == ID).Select(pe => pe.Description).FirstOrDefault();
            return Name;
        }

        private string GetDeliveryUnit(int? ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.DeliveryUnit.Where(pe => pe.ID == ID).Select(pe => pe.Name).FirstOrDefault();
            return Name;
        }

        private string GetDesignation(int ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.Designations.Where(pe => pe.ID == ID).Select(pe => pe.Name).FirstOrDefault();
            return Name;
        }

        private string GetDesignation(int? ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.Designations.Where(pe => pe.ID == ID).Select(pe => pe.Name).FirstOrDefault();
            return Name;
        }

        private string GetLocation(int? ID)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.WorkLocation.Where(pe => pe.ID == ID).Select(pe => pe.LocationName).FirstOrDefault();
            return Name;
        }

        private string GetSkills(List<CandidateSkillMappingViewModel> CandidateSkillMappingVM)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = "";
            string Skills = "";
            if (CandidateSkillMappingVM != null)
            {
                for (int i = 0; i < CandidateSkillMappingVM.Count(); i++)
                {
                    int skillID = CandidateSkillMappingVM[i].SkillID;
                    Name = context.SkillMatrices.Where(pe => pe.ID == skillID).Select(pe => pe.Name).FirstOrDefault();
                    if (CandidateSkillMappingVM.Count() > 1)
                    {
                        if (i == 0)
                        {
                            Skills = Name;
                        }
                        else
                        {
                            Skills = Skills + "," + Name;
                        }

                    }
                    else
                    {
                        Skills = Name;
                    }

                }
            }
            return Skills;
        }

        private string GetRRFSkills(List<int> skillIDs)
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = "";
            string Skills = "";
            for (int i = 0; i < skillIDs.Count(); i++)
            {
                int skillID = skillIDs[i];
                Name = context.SkillMatrices.Where(pe => pe.ID == skillID).Select(pe => pe.Name).FirstOrDefault();
                if (skillIDs.Count() > 1)
                {
                    if (i == 0)
                    {
                        Skills = Name;
                    }
                    else
                    {
                        Skills = Skills + "," + Name;
                    }

                }
                else
                {
                    Skills = Name;
                }

            }

            return Skills;
        }

        void GetEmailCCForAllDeptRRF(out string emailCC)
        {
            emailCC = string.Empty;
            using (PhoenixEntities entites = new PhoenixEntities())
            {
                var deptID = entites.HelpDeskCategories.Where(x => x.AssignedRole != 29 && x.AssignedRole != 41
                && x.Prefix != "HRGROUP" && x.Prefix != "SeparationGroup" && x.AssignedRole != 28).ToList();
                foreach (var item in deptID)
                {
                    emailCC = emailCC + "," + item.EmailGroup;
                }
            }
        }

        private string GetRecruitmentTeamEmail()
        {
            PhoenixEntities context = new PhoenixEntities();
            string Name = context.HelpDeskCategories.Where(pe => pe.AssignedRole == 46).Select(pe => pe.EmailGroup).FirstOrDefault();
            return Name;
        }

        public bool RrfSwapNotification(int sender, int admin, string rrfList)
        {
            string attachments = string.Empty;

            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(admin);
            string emailCC = GetRecruitmentTeamEmail();
            List<String> lstEmailCC = new List<string>();
            List<String> lstEmailTO = new List<string>();
            string strDistinctEmailCC = "";
            string strDistinctEmailTo = "";
            bool isCreated;


            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RRFSwap))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == admin);

            subject = emailTemplate.Subjects;
            using (var _db = _phoenixEntity)
            {
                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{RecruitmentHead}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{RRFList}}", rrfList);
            }

            if (emailCC.Contains(emailTo))
            {
                emailCC = emailCC.Replace(emailTo, "");
            }
            lstEmailCC = emailCC.Split(',').Distinct().ToList();
            for (int i = 0; i < lstEmailCC.Count; i++)
            {
                if (i > 0)
                {
                    strDistinctEmailCC = strDistinctEmailCC + ",";
                }
                strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
            }
            strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
            lstEmailTO = emailTo.Split(',').Distinct().ToList();
            for (int i = 0; i < lstEmailTO.Count; i++)
            {
                if (i > 0)
                {
                    strDistinctEmailTo = strDistinctEmailTo + ",";
                }
                strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
            }
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo.Replace(@"old_", string.Empty), // Old is remove to handle seperation cases 
                    Subject = subject,
                    EmailCC = strDistinctEmailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        public bool SendContractCompletionEmail(CandidateViewModel model, int nextPersonID)
        {
            string attachments = string.Empty;

            string statement = string.Empty;
            string subject = string.Empty;
            string emailTo = GetApprover(nextPersonID);
            string emailCC = GetHRGroupEmailIds();
            List<String> lstEmailCC = new List<string>();
            List<String> lstEmailTO = new List<string>();
            string strDistinctEmailCC = "";
            string strDistinctEmailTo = "";
            bool isCreated;


            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ContractComplete))).First();

            string template = emailTemplate.Html;
            var person = service.First<Person>(x => x.ID == nextPersonID);

            subject = emailTemplate.Subjects;
            subject = subject.Replace("{{username}}", person.FirstName + " " + person.LastName);
            subject = subject.Replace("{{oldemployeeid}}", Convert.ToString(model.OldPersonID));
            using (var _db = _phoenixEntity)
            {
                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{username}}", person.FirstName + " " + person.LastName);
                template = template.Replace("{{oldemployeeid}}", Convert.ToString(model.OldPersonID));
                template = template.Replace("{{joiningdate}}", Convert.ToString(model.JoiningDate.Value.Date.ToString("M/d/yyyy")));
                template = template.Replace("{{newemployeeid}}", Convert.ToString(person.ID));
                template = template.Replace("{{newdesignation}}", Convert.ToString(GetDesignation(model.DesignationID)));

            }

            if (emailCC.Contains(emailTo))
            {
                emailCC = emailCC.Replace(emailTo, "");
            }
            lstEmailCC = emailCC.Split(',').Distinct().ToList();
            for (int i = 0; i < lstEmailCC.Count; i++)
            {
                if (i > 0)
                {
                    strDistinctEmailCC = strDistinctEmailCC + ",";
                }
                strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
            }
            strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
            lstEmailTO = emailTo.Split(',').Distinct().ToList();
            for (int i = 0; i < lstEmailTO.Count; i++)
            {
                if (i > 0)
                {
                    strDistinctEmailTo = strDistinctEmailTo + ",";
                }
                strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
            }
            try
            {
                isCreated = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = person.PersonEmployment.FirstOrDefault().OrganizationEmail,
                    EmailTo = strDistinctEmailTo.Replace(@"old_", string.Empty), // Old is remove to handle seperation cases 
                    Subject = subject,
                    EmailCC = strDistinctEmailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreated)
                    service.Finalize(true);

                return isCreated;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SendValuePortalCommentUpdate(int senderEmpId, int receiverEmpId, VCIdeaMasterViewModel ideaVM, string Comment)
        {
            var vpCommentUpdateEmailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.VCFNewCommentAdded))).First();
            string subject = vpCommentUpdateEmailTemplate.Subjects;
            string template = vpCommentUpdateEmailTemplate.Html;
            string SenderName = GetPersonName(senderEmpId);
            string ReceiverName = GetPersonName(receiverEmpId);
            string copyToMail = string.Empty;
            copyToMail = prepareEmailCCListForIdeaUpdate(ideaVM, false, false, false, true, senderEmpId, receiverEmpId);
            //For: #181069584 Dynamic email subject when reviewer/approver has added some comments
            if (ideaVM != null && subject != null)
            {
                //string tempIdeaHeadline = ideaVM.IdeaHeadline;
                //if (tempIdeaHeadline.Length > 20)
                //{
                //    tempIdeaHeadline = tempIdeaHeadline.Substring(0, 20);
                //}
                // subject = subject.Replace("{{truncatedtitle}}", tempIdeaHeadline);
                subject = subject.Replace("{{username}}", SenderName);
                subject = subject.Replace("{{employeeid}}", senderEmpId.ToString());
                subject = subject.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
            }

            //var buDBRow = service.First<DeliveryUnit>(x => x.Active.Value && x.ID == ideaVM.BusinessUnit);
            //if(buDBRow != null)
            //{
            //    ideaVM.BusinessUnitName = buDBRow.Name;
            //}

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
            template = template.Replace("{{IdeaHeadline}}", ideaVM.IdeaHeadline);
            template = template.Replace("{{status}}", ideaVM.StatusName);
            template = template.Replace("{{loggedinuser}}", SenderName);

            //template = template.Replace("{{uniquenessQuotient}}", ideaVM.UniquenessQuotient);
            //template = template.Replace("{{benefits}}", ideaVM.BenefitValue);
            //template = template.Replace("{{Cost}}", ideaVM.CostDesc);
            //template = template.Replace("{{priority}}", ideaVM.PriorityName);
            //template = template.Replace("{{businessUnit}}", ideaVM.BusinessUnitName);

            template = template.Replace("{{action}}", "New Comment has been Added");
            template = template.Replace("{{reaction}}", "");
            template = template.Replace("{{comment}}", "<b>" + Comment + "</b>");

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = GetPersonOrgEmail(senderEmpId),
                EmailTo = GetPersonOrgEmail(receiverEmpId),
                Subject = subject,
                EmailCC = copyToMail
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        public void SendValuePortalIdeaUpdate(VCIdeaMasterViewModel ideaVM, int SenderEmpId, int ReceiverEmpId, bool isGlobalApprover, bool isBUApprover, string[] DirtyValuesList)
        {
            string ReceiverName = GetPersonName(ReceiverEmpId);
            string SenderName = GetPersonName(SenderEmpId);
            var vpIdeaUpdateEmailRow = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ValuePortal))).First();
            string subject = string.Empty;
            subject = vpIdeaUpdateEmailRow.Subjects;

            //For: #VCF-181123405 Dynamic email subject updation Email subject placeholder replacement
            if (subject != null && ideaVM != null)
            {
                if (ideaVM.IdeaHeadline != null && ideaVM.IdeaHeadline.Length > 20)
                {
                    subject = subject.Replace("{{truncatedtitle}}", ideaVM.IdeaHeadline.Substring(0, 20));
                }
                else
                {
                    subject = subject.Replace("{{truncatedtitle}}", ideaVM.IdeaHeadline);
                }
                subject = subject.Replace("{{username}}", SenderName);
                subject = subject.Replace("{{employeeid}}", SenderEmpId.ToString());
            }

            var buDBRow = service.First<DeliveryUnit>(x => x.Active.Value && x.ID == ideaVM.BusinessUnit);
            if (buDBRow != null)
            {
                ideaVM.BusinessUnitName = buDBRow.Name;
            }

            string template = CreateTemplateForValuePortalIdeaUpdate(ReceiverName, ReceiverEmpId, SenderName, SenderEmpId, ideaVM, DirtyValuesList);
            string copyToMail = string.Empty;
            bool isStatusUpdate = false;
            if (DirtyValuesList != null && DirtyValuesList.Length > 0)
            {
                isStatusUpdate = DirtyValuesList.Contains("7");
            }

            copyToMail = prepareEmailCCListForIdeaUpdate(ideaVM, isGlobalApprover, isBUApprover, isStatusUpdate, false, SenderEmpId, ReceiverEmpId);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = GetPersonOrgEmail(SenderEmpId),
                EmailTo = GetPersonOrgEmail(ReceiverEmpId),
                Subject = subject,
                EmailCC = copyToMail,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        private string CreateTemplateForValuePortalIdeaUpdate(string ReceiverName, int ReceiverEmpId, string SenderName, int SenderEmpId, VCIdeaMasterViewModel ideavVM, string[] DirtyValuesList)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ValuePortal))).First().Html;
            //string statement;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{IdeaHeadline}}", ideavVM.IdeaHeadline);
            template = template.Replace("{{loggedinuser}}", SenderName);
            template = template.Replace("{{IdeaID}}", Convert.ToString(ideavVM.ID));

            template = makePlaceholderBoldForUpdatedValues(ideavVM, template, DirtyValuesList);

            //template = template.Replace("{{ReceiverName}}", ReceiverName);
            //template = template.Replace("{{ReceiverEmpId}}", ReceiverEmpId.ToString());
            //template = template.Replace("{{SenderName}}", SenderName);
            //template = template.Replace("{{SenderEmpId}}", " (" + SenderEmpId + ") ");

            //statement = " has updated on your idea.";
            //template = template.Replace("{{statement}}", statement);

            return template;
        }

        private string makePlaceholderBoldForUpdatedValues(VCIdeaMasterViewModel ideavVM, string template, string[] DirtyValuesList)
        {
            if (DirtyValuesList.Contains("1"))
            {
                template = template.Replace("{{uniquenessQuotient}}", "<b>" + ideavVM.UniquenessQuotient + "</b>");
            }
            else
            {
                template = template.Replace("{{uniquenessQuotient}}", ideavVM.UniquenessQuotient);
            }

            if (DirtyValuesList.Contains("2"))
            {
                template = template.Replace("{{benefits}}", "<b>" + ideavVM.BenefitValue + "</b>");
            }
            else
            {
                template = template.Replace("{{benefits}}", ideavVM.BenefitValue);
            }
            if (DirtyValuesList.Contains("3"))
            {
                template = template.Replace("{{Cost}}", "<b>" + ideavVM.CostDesc + "</b>");
            }
            else
            {
                template = template.Replace("{{Cost}}", ideavVM.CostDesc);
            }
            if (DirtyValuesList.Contains("5"))
            {
                template = template.Replace("{{priority}}", "<b>" + ideavVM.PriorityName + "</b>");
            }
            else
            {
                template = template.Replace("{{priority}}", ideavVM.PriorityName);
            }
            if (DirtyValuesList.Contains("6"))
            {
                template = template.Replace("{{businessUnit}}", "<b>" + ideavVM.BusinessUnitName + "</b>");
            }
            else
            {
                template = template.Replace("{{businessUnit}}", ideavVM.BusinessUnitName);
            }

            if (DirtyValuesList.Contains("7"))
            {
                if (ideavVM.StatusID == 6)
                {
                    template = template.Replace("{{action}}", "Congratulations!! It's an innovative thought. Your proposal is <b>shortlisted</b>");
                    template = template.Replace("{{reaction}}", "Please proceed and fill Phase-II form.");
                }
                if (ideavVM.StatusID == 2)
                {
                    template = template.Replace("{{action}}", "Congratulations!! Your proposal is <b>approved</b>");
                    template = template.Replace("{{reaction}}", "We will evaluate it with other similar proposals and will update you.");
                }
                else if (ideavVM.StatusID == 18 || ideavVM.StatusID == 15 || ideavVM.StatusID == 3)
                {
                    template = template.Replace("{{action}}", "This proposal can be improved further, hence it is <b>rejected</b> for now");
                    template = template.Replace("{{reaction}}", "Please re-evaluate it and you can resubmit it after adding missing factors.");
                }
                else if (ideavVM.StatusID == 14 || ideavVM.StatusID == 17)
                {
                    template = template.Replace("{{action}}", "We need additional details to proceed further, hence it is <b>on hold</b> for now");
                    template = template.Replace("{{reaction}}", "Please refer to comments and share required details.");
                }
                else if (ideavVM.StatusID == 16 || ideavVM.StatusID == 19)
                {
                    template = template.Replace("{{action}}", "Your proposal is kept <b>on hold</b> for now");
                    template = template.Replace("{{reaction}}", "Please refer to comments. You will recieve further updates/notification from system.");
                }
                else if (ideavVM.StatusID == 13)
                {
                    template = template.Replace("{{action}}", "This idea is pushed back for review");
                    template = template.Replace("{{reaction}}", "Please refer to the comment and do the needful.");
                }
                else if (ideavVM.StatusID == 4)
                {
                    template = template.Replace("{{action}}", "Congratulations!!Your proposal has been <b> sponsored </b>");
                    template = template.Replace("{{reaction}}", "Please connect with your BU Approver to plan the implementation.");
                }
                else if (ideavVM.StatusID == 9)
                {
                    template = template.Replace("{{action}}", "This proposal can be improved further, hence it is <b>rejected/deprecated</b> for now");
                    template = template.Replace("{{reaction}}", "");
                }
                else if (ideavVM.StatusID == 5 || ideavVM.StatusID == 10 || ideavVM.StatusID == 11 || ideavVM.StatusID == 12)
                {
                    template = template.Replace("{{action}}", "The status of idea has been changed to <b>" + ideavVM.StatusName + "</b>");
                    template = template.Replace("{{reaction}}", "");
                }

                template = template.Replace("{{status}}", "<b>" + ideavVM.StatusName + "</b>");
            }
            else
            {
                template = template.Replace("{{action}}", " Your proposal has some <b> Updates </b>");
                template = template.Replace("{{reaction}}", "Please refer to comments.");
                template = template.Replace("{{status}}", ideavVM.StatusName);
            }

            if (DirtyValuesList.Contains("8"))
            {
                template = template.Replace("{{comment}}", "<b>" + ideavVM.UserComment + "</b>");
            }
            else
            {
                template = template.Replace("{{comment}}", ideavVM.UserComment);
            }

            return template;
        }

        public void SendValuePortalIdeaSubmitted(VPSubmittedIdeaViewModel vpSubmittedIdeaViewModel, VCIdeaMasterViewModel ideaVM)
        {
            string subject = string.Empty;
            string copyToMail = string.Empty;
            copyToMail = prepareEmailCCListForIdeaSubmit(ideaVM);
            //var statusName = "Submitted";
            var vpIdeaSubmitEmailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.VCFSubmitPhaseTwo))).First();
            string template = vpIdeaSubmitEmailTemplate.Html;
            subject = vpIdeaSubmitEmailTemplate.Subjects;

            //For: #181069544 Dynamic email subject updation Email subject placeholder replacement
            if (subject != null)
            {
                subject = subject.Replace("{{username}}", vpSubmittedIdeaViewModel.submitterName);
                subject = subject.Replace("{{employeeid}}", vpSubmittedIdeaViewModel.EmployeeId.ToString());
                subject = subject.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
            }

            var businessUnit = service.First<DeliveryUnit>(x => x.Active == true && x.ID == ideaVM.BusinessUnit);
            string businessUnitName = businessUnit.Name;

            // Email Body placeholder replacement
            if (ideaVM != null && vpSubmittedIdeaViewModel != null)
            {
                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
                template = template.Replace("{{IdeaHeadline}}", ideaVM.IdeaHeadline);
                template = template.Replace("{{benefitScopeName}}", ideaVM.benefitScopeValue);
                template = template.Replace("{{benefitFactor}}", ideaVM.BenefitFactor);
                template = template.Replace("{{status}}", ideaVM.StatusName);
                template = template.Replace("{{businessUnit}}", businessUnitName);
                template = template.Replace("{{uniquenessQuotient}}", ideaVM.UniquenessQuotient);
                template = template.Replace("{{Cost}}", vpSubmittedIdeaViewModel.CostDesc);
                template = template.Replace("{{requiredEfforts}}", vpSubmittedIdeaViewModel.EffortRequired);
                template = template.Replace("{{submittedByName}}", ideaVM.SubmittedByName);
                template = template.Replace("{{loggedinuser}}", vpSubmittedIdeaViewModel.submitterName);
            }

            // previous code date till 28th March 2022
            //template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            //template = template.Replace("{{username}}", vpSubmittedIdeaViewModel.submitterName);
            //template = template.Replace("{{employeeid}}", vpSubmittedIdeaViewModel.EmployeeId.ToString());
            //template = template.Replace("{{ideaheadline}}", vpSubmittedIdeaViewModel.IdeaHeadline);
            //template = template.Replace("{{ideabrief}}", vpSubmittedIdeaViewModel.IdeaBrief);
            //template = template.Replace("{{ideabenefits}}", vpSubmittedIdeaViewModel.IdeaBenefits);
            //template = template.Replace("{{effortRequired}}", vpSubmittedIdeaViewModel.EffortRequired);
            //template = template.Replace("{{resourcesRequired}}", vpSubmittedIdeaViewModel.ResourcesRequired);
            //template = template.Replace("{{technologiesRequired}}", vpSubmittedIdeaViewModel.ResourcesRequired);
            //template = template.Replace("{{executionapproach}}", vpSubmittedIdeaViewModel.ExecutionApproach);
            //template = template.Replace("{{ideaurl}}", Convert.ToString(ConfigurationManager.AppSettings["AppUrl"]) + "/#/valueportal/details/" + vpSubmittedIdeaViewModel.Id); 

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = vpSubmittedIdeaViewModel.personOrganizationEmail,
                EmailTo = vpSubmittedIdeaViewModel.approverOrganizationEmail,
                Subject = subject,
                EmailCC = copyToMail,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        public string prepareEmailCCListForIdeaSubmit(VCIdeaMasterViewModel ideaVM)
        {
            // set email CC groups
            // BU approver email list
            string finalCopyToEmailList = null;
            List<int> finalEmailCCList = null;
            List<int> buApproverCCEmailList = null;
            string teamMembersCCEmailList = string.Empty;
            List<int> teammemberIdIntList = null;
            List<string> teammemberIdList = null;
            // Proceed only If TeammemberIds is not null.
            if (!String.IsNullOrEmpty(ideaVM.TeammemberIds))
            {
                teammemberIdList = ideaVM.TeammemberIds.Split(',').ToList();
                if (teammemberIdList?.Count > 0)
                {
                    teammemberIdIntList = teammemberIdList.Select(int.Parse).Distinct().ToList();
                }
            }
            buApproverCCEmailList = getListOfGloblaAndBUApprovers(true, ideaVM.BusinessUnit);
            if (teammemberIdIntList?.Count > 0)
            {
                finalEmailCCList = buApproverCCEmailList?.Concat(teammemberIdIntList).Distinct().ToList();
            }
            else
            {
                if (buApproverCCEmailList?.Count() > 0)
                {
                    finalEmailCCList = buApproverCCEmailList?.Distinct().ToList();
                }
            }
            if (finalEmailCCList?.Count() > 0)
            {
                finalCopyToEmailList = getEmpEmailIDsFromEmpIDList(finalEmailCCList);
            }

            return finalCopyToEmailList;
        }

        public string prepareEmailCCListForIdeaUpdate(VCIdeaMasterViewModel ideaVM, bool isGlobalApprover, bool isBUApprover,
            bool isStatusUpdate, bool isGeneralUser, int SenderEmpId, int receiverEmpId)
        {
            // set email CC groups
            // BU approver email list
            string finalCopyToEmailList = null;
            List<int> finalEmailCCList = null;
            List<int> buApproverCCEmailList = null;
            string teamMembersCCEmailList = string.Empty;
            List<int> teammemberIdIntList = null;
            List<string> teammemberIdList = null;
            if (!String.IsNullOrEmpty(ideaVM.TeammemberIds))
            {
                teammemberIdList = ideaVM.TeammemberIds.Split(',').ToList();
                if (teammemberIdList?.Count > 0)
                {
                    teammemberIdIntList = teammemberIdList.Select(int.Parse)?.Distinct().ToList();
                }
            }
            if (isStatusUpdate)
            {
                buApproverCCEmailList = getListOfGloblaAndBUApprovers(true, ideaVM.BusinessUnit);
                if (buApproverCCEmailList?.Count > 0)
                {
                    if (teammemberIdIntList != null)
                    {
                        finalEmailCCList = teammemberIdIntList?.Concat(buApproverCCEmailList).Distinct().ToList();
                    }
                    else
                    {
                        finalEmailCCList = buApproverCCEmailList.Distinct().ToList();
                    }
                }
                else
                {
                    finalEmailCCList = teammemberIdIntList?.Distinct().ToList();
                }
                if (isGlobalApprover || ideaVM.StatusID == 2 || ideaVM.StatusID == 4) // Added on 12th April 22 As on Idea Approval by BA user Email should shoot to All GA users also
                {
                    List<int> globalApproverCCEmailList = null;
                    globalApproverCCEmailList = getListOfGloblaAndBUApprovers(false, ideaVM.BusinessUnit);

                    if (globalApproverCCEmailList?.Count > 0)
                    {
                        if (finalEmailCCList != null)
                        {
                            finalEmailCCList = finalEmailCCList?.Concat(globalApproverCCEmailList).Distinct().ToList();
                        }
                        else
                        {
                            finalEmailCCList = globalApproverCCEmailList.Distinct().ToList();
                        }
                    }
                }
            }
            else if (teammemberIdIntList?.Count > 0)
            {
                teammemberIdIntList = removeDublicateEmailIdsFromCCList(teammemberIdIntList, SenderEmpId, receiverEmpId);
                finalEmailCCList = teammemberIdIntList;
            }
            if (finalEmailCCList != null)
            {
                finalEmailCCList = removeDublicateEmailIdsFromCCList(finalEmailCCList, SenderEmpId, receiverEmpId);
                finalCopyToEmailList = getEmpEmailIDsFromEmpIDList(finalEmailCCList);
            }
            return finalCopyToEmailList;
        }

        public void SendValuePortalIdeaStatus(VPSubmittedIdeaViewModel vpSubmittedIdeaViewModel)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.ValuePortalIdeaStatus))).First().Html;
            string copyToMail = string.Empty;

            template = template.Replace("{{username}}", vpSubmittedIdeaViewModel.submitterName);
            template = template.Replace("{{employeeid}}", vpSubmittedIdeaViewModel.EmployeeId.ToString());
            template = template.Replace("{{status}}", vpSubmittedIdeaViewModel.StatusID.ToString());
            template = template.Replace("{{additionaltext}}", "Idea ID:" + vpSubmittedIdeaViewModel.Id);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = vpSubmittedIdeaViewModel.personOrganizationEmail,
                EmailTo = vpSubmittedIdeaViewModel.approverOrganizationEmail,
                // Subject = subject,
                EmailCC = copyToMail,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        public List<int> getListOfGloblaAndBUApprovers(bool isBUApprover, int bussinessUnitId)
        {
            List<int> approverEmailList = new List<int>();
            if (isBUApprover)
            {
                List<int> buApproversList = new List<int>();
                List<VCFApprover> vcfBUApproversList = service.Top<VCFApprover>(0, v => v.IsDeleted == 0 && v.DeliveryUnitID == bussinessUnitId).ToList();
                if (vcfBUApproversList != null && vcfBUApproversList.Count() > 0)
                {
                    foreach (var vcfBUApproverRowObj in vcfBUApproversList)
                    {
                        if (!buApproversList.Contains(vcfBUApproverRowObj.ReviewerId.Value))
                        {
                            buApproversList.Add(vcfBUApproverRowObj.ReviewerId.Value);
                        }
                    }
                    approverEmailList = buApproversList;
                }

            }
            else
            {
                List<int> globalApproversList = new List<int>();
                int globalUserRoleId = service.Top<Role>(0, v => v.IsDeleted == false && v.Name == "VCF Reviewer").FirstOrDefault().ID;
                List<PersonInRole> vcfGlobalApproversList = service.Top<PersonInRole>(0, v => v.IsDeleted == false && v.RoleID == globalUserRoleId).ToList();
                if (vcfGlobalApproversList != null && vcfGlobalApproversList.Count() > 0)
                {
                    foreach (var vcfGlobalApproverObj in vcfGlobalApproversList)
                    {
                        if (!globalApproversList.Contains(vcfGlobalApproverObj.PersonID))
                        {
                            globalApproversList.Add(vcfGlobalApproverObj.PersonID);
                        }
                    }
                    approverEmailList = globalApproversList;
                }

            }
            //service.Finalize(true);
            return approverEmailList;
        }

        public string getEmpEmailIDsFromEmpIDList(List<int> employeeIDList)
        {
            string commaSeperatedEmailIds = string.Empty;
            List<string> emailIdList = new List<string>();
            if (employeeIDList != null && employeeIDList.Count() > 0)
            {
                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    emailIdList = entites.PersonEmployment.Where(person => person.IsDeleted == false && person.Person.Active == true && employeeIDList.Contains(person.PersonID.Value)).Select(person => person.OrganizationEmail).ToList();
                }
                if (emailIdList != null && emailIdList.Count() > 0)
                {
                    commaSeperatedEmailIds = string.Join(",", emailIdList);
                }
            }

            return commaSeperatedEmailIds;
        }

        // If Logged in user & email receiver is present in ccList remove duplicate from EmailCC list
        public List<int> removeDublicateEmailIdsFromCCList(List<int> emailCCList, int SenderEmpId, int receiverEmpId)
        {
            if (emailCCList != null && emailCCList.Count() > 0)
            {
                if (emailCCList.Contains(SenderEmpId))
                {
                    emailCCList.Remove(SenderEmpId);
                }
                if (emailCCList.Contains(receiverEmpId))
                {
                    emailCCList.Remove(receiverEmpId);
                }
            }
            return emailCCList;
        }

        public void SendVCFPIIdeaUpdate(VCIdeaMasterViewModel ideaVM, int SenderEmpId, int ReceiverEmpId, bool isGlobalApprover, bool isBUApprover, string[] DirtyValuesList)
        {
            string ReceiverName = GetPersonName(ReceiverEmpId);
            string SenderName = GetPersonName(SenderEmpId);
            var vpIdeaUpdateEmailRow = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.VCFUpdate))).First();
            string subject = string.Empty;
            subject = vpIdeaUpdateEmailRow.Subjects;

            //For: #VCF-181123405 Dynamic email subject updation Email subject placeholder replacement
            if (subject != null && ideaVM != null)
            {
                //if (ideaVM.IdeaHeadline != null && ideaVM.IdeaHeadline.Length > 20)
                //{
                //    subject = subject.Replace("{{truncatedtitle}}", ideaVM.IdeaHeadline.Substring(0, 20));
                //}
                //else
                //{
                //    subject = subject.Replace("{{truncatedtitle}}", ideaVM.IdeaHeadline);
                //}
                subject = subject.Replace("{{username}}", SenderName);
                subject = subject.Replace("{{employeeid}}", SenderEmpId.ToString());
                subject = subject.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
            }

            var buDBRow = service.First<DeliveryUnit>(x => x.Active.Value && x.ID == ideaVM.BusinessUnit);
            if (buDBRow != null)
            {
                ideaVM.BusinessUnitName = buDBRow.Name;
            }

            string template = CreateTemplateForVCFPIIdeaUpdate(ReceiverName, ReceiverEmpId, SenderName, SenderEmpId, ideaVM, DirtyValuesList);
            string copyToMail = string.Empty;
            bool isStatusUpdate = false;
            if (DirtyValuesList != null && DirtyValuesList.Length > 0)
            {
                isStatusUpdate = DirtyValuesList.Contains("7");
            }

            copyToMail = prepareEmailCCListForIdeaUpdate(ideaVM, isGlobalApprover, isBUApprover, isStatusUpdate, false, SenderEmpId, ReceiverEmpId);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = GetPersonOrgEmail(SenderEmpId),
                EmailTo = GetPersonOrgEmail(ReceiverEmpId),
                Subject = subject,
                EmailCC = copyToMail,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        private string CreateTemplateForVCFPIIdeaUpdate(string ReceiverName, int ReceiverEmpId, string SenderName, int SenderEmpId, VCIdeaMasterViewModel ideavVM, string[] DirtyValuesList)
        {
            string template = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.VCFUpdate))).First().Html;
            //string statement;

            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{IdeaHeadline}}", ideavVM.IdeaHeadline);
            template = template.Replace("{{loggedinuser}}", SenderName);
            template = template.Replace("{{IdeaID}}", Convert.ToString(ideavVM.ID));

            template = makePlaceholderBoldForUpdatedValues(ideavVM, template, DirtyValuesList);

            return template;
        }

        public void sendEmailOnVCFPhaseISubmit(VPSubmittedIdeaViewModel vpSubmittedIdeaViewModel, VCIdeaMasterViewModel ideaVM)
        {
            string subject = string.Empty;
            string copyToMail = string.Empty;
            copyToMail = prepareEmailCCListForIdeaSubmit(ideaVM);
            //var statusName = "Submitted";
            var vpIdeaSubmitEmailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.VPSubmitPhaseOne))).First();
            string template = vpIdeaSubmitEmailTemplate.Html;
            subject = vpIdeaSubmitEmailTemplate.Subjects;

            //For: #181069544 Dynamic email subject updation Email subject placeholder replacement
            if (subject != null)
            {
                subject = subject.Replace("{{username}}", vpSubmittedIdeaViewModel.submitterName);
                subject = subject.Replace("{{employeeid}}", vpSubmittedIdeaViewModel.EmployeeId.ToString());
                subject = subject.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
            }

            //var businessUnit = service.First<DeliveryUnit>(x => x.Active == true && x.ID == ideaVM.BusinessUnit);
            //string businessUnitName = businessUnit.Name;

            // Email Body placeholder replacement
            template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
            template = template.Replace("{{IdeaHeadline}}", ideaVM.IdeaHeadline);
            template = template.Replace("{{benefitFactor}}", ideaVM.BenefitFactor);
            template = template.Replace("{{benefitScopeName}}", ideaVM.benefitScopeValue);
            template = template.Replace("{{loggedinuser}}", vpSubmittedIdeaViewModel.submitterName);
            template = template.Replace("{{IdeaID}}", Convert.ToString(ideaVM.ID));
            //template = template.Replace("{{status}}", statusName);
            //template = template.Replace("{{businessUnit}}", businessUnitName);
            // template = template.Replace("{{submittedByName}}", vpSubmittedIdeaViewModel.submitterName);

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,
                EmailFrom = vpSubmittedIdeaViewModel.personOrganizationEmail,
                EmailTo = vpSubmittedIdeaViewModel.approverOrganizationEmail,
                Subject = subject,
                EmailCC = copyToMail,
            }, e => e.Id == 0);

            if (isCreated)
                service.Finalize(true);
        }

        public bool SendOrgDetailsUpdateStatus(int personId, int id, EmployeeOrganizaionDetails model)
        {
            try
            {
                string attachments = string.Empty;
                string statement = string.Empty;
                string subject = string.Empty;
                string emailTo = GetApprover(model.ID);
                string orgReportingMngr = GetOrgReportingTo(model.ID);
                string emailCC = GetRMGGroupEmailIds();
                string emailActionId = string.Empty;
                List<String> lstEmailCC = new List<string>();
                List<String> lstEmailTO = new List<string>();
                string strDistinctEmailCC = "";
                string strDistinctEmailTo = "";

                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.RMandEMUpdate))).First();

                string template = emailTemplate.Html;
                var person = service.First<Person>(x => x.ID == model.ID);
                var resource = service.First<Person>(x => x.ID == model.ID);

                var orgReportingManager = service.First<PersonReporting>(x => x.PersonID == person.ID).ReportingTo;
                var orgReportingManagerName = $"{service.First<Person>(x => x.ID == orgReportingManager).FirstName} {service.First<Person>(x => x.ID == orgReportingManager).LastName}";

                var exitProcessManagerid = service.First<PersonEmployment>(x => x.PersonID == person.ID).ExitProcessManager;
                var exitProcessManagerName = $"{service.First<Person>(x => x.ID == exitProcessManagerid).FirstName} {service.First<Person>(x => x.ID == exitProcessManagerid).LastName}";
                var exitProcessManagerEmail = service.First<PersonEmployment>(x => x.PersonID == exitProcessManagerid).OrganizationEmail;

                var logginuserid = service.First<Person>(x => x.ID == personId);
                var logginusername = $"{logginuserid.FirstName} {logginuserid.LastName}";

                using (var _db = _phoenixEntity)
                {
                    subject = emailTemplate.Subjects;

                    template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                    template = template.Replace("{{reportingto}}", orgReportingManagerName);
                    template = template.Replace("{{exitprocessmanager}}", exitProcessManagerName);
                    template = template.Replace("{{loggedinuser}}", logginusername);
                }

                emailCC = $"{emailCC},{orgReportingMngr},{exitProcessManagerEmail}";

                if (emailCC.Contains(emailTo))
                {
                    emailCC = emailCC.Replace(emailTo, "");
                }
                lstEmailCC = emailCC.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailCC.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailCC = strDistinctEmailCC + ",";
                    }
                    strDistinctEmailCC = strDistinctEmailCC + lstEmailCC[i];
                }
                strDistinctEmailCC = Regex.Replace(strDistinctEmailCC, ",+", ",").Trim(',');
                lstEmailTO = emailTo.Split(',').Distinct().ToList();
                for (int i = 0; i < lstEmailTO.Count; i++)
                {
                    if (i > 0)
                    {
                        strDistinctEmailTo = strDistinctEmailTo + ",";
                    }
                    strDistinctEmailTo = strDistinctEmailTo + lstEmailTO[i];
                }

                bool isCreate = service.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = DateTime.Now,
                    EmailFrom = service.First<PersonEmployment>(x => x.PersonID == personId).OrganizationEmail,
                    EmailTo = strDistinctEmailTo,
                    Subject = "RM & EM Update Details",
                    EmailCC = strDistinctEmailCC,
                    Attachments = attachments
                }, e => e.Id == 0);

                if (isCreate)
                    service.Finalize(true);

                return isCreate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendLeaveCancellationStatus(int empID,string empName, string senderEmail, string startDate, string endDate, int noOfDays,string Narration, string status, string imageName, int? LeaveType)
        {       
            string statement = string.Empty;
            string subject = string.Empty;         
            string emailCC = string.Empty;
            string emailTo = string.Empty;
            string hrManagers =  GetHRGroupEmailIds();
            string leaveTypeText="";

            var reportingManagerId = service.First<PersonReporting>(x => x.PersonID == empID).ReportingTo;
            var reportingManagerEmailId = service.First<PersonEmployment>(x => x.PersonID == reportingManagerId).OrganizationEmail;

            var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.LeaveCancellationUpdate))).FirstOrDefault();
            string template = emailTemplate.Html;

            switch (LeaveType)
            {
                case 0:
                        leaveTypeText = "Comp Off";
                        break;
                case 1:
                        leaveTypeText = "Privilege Leave";
                    break;
                case 2:
                        leaveTypeText = "LWP";
                        break;
                case 3:
                        leaveTypeText = "Maternity Leave";
                        break;
                case 4:
                        leaveTypeText = "Paternity Leave";
                        break;
                case 5:
                        leaveTypeText = "Long Leave";
                        break;
                case 6:
                        leaveTypeText = "Birthday Leave";
                        break;
                case 7:
                        leaveTypeText = "Floating Holiday";
                        break;
                case 8:
                        leaveTypeText = "Special Floating Holiday";
                        break;
                case 9:
                        leaveTypeText = "Casual Leave";
                        break;
                case 10:
                        leaveTypeText = "MTP";
                        break;
                case 11:
                        leaveTypeText = "Sick Leave";
                        break;
                default:
                        leaveTypeText = "";
                        break;
            }

            using (var _db = _phoenixEntity)
            {
                template = template.Replace("{{date}}", DateTime.Now.Date.ToStandardDate());
                template = template.Replace("{{toname}}", empName);
                template = template.Replace("{{username}}", empName);
                template = template.Replace("{{employeeid}}", empID.ToString());
                template = template.Replace("{{status}}", status);
                template = template.Replace("{{imagename}}", Convert.ToString(ConfigurationManager.AppSettings["baseurl"]) + imageName);
                template = template.Replace("{{loggedinuser}}", empName);
                template = template.Replace("{{startdate}}", "(" + startDate + ")");
                template = template.Replace("{{enddate}}", "(" + endDate + ")");
                template = template.Replace("{{leavetype}}", leaveTypeText);
                template = template.Replace("{{noofdays}}", noOfDays.ToString());
                template = template.Replace("{{narration}}", Narration);         
            }

            bool isCreated = service.Create<Emails>(new Emails
            {
                Content = template,
                Date = DateTime.Now,                
                EmailFrom = senderEmail,
                EmailTo = $"{reportingManagerEmailId},{hrManagers}",
                Subject = "Leave Cancellation Details",
            }, e => e.Id == 0);

            if (isCreated)
                 service.Finalize(true);
        }                 
    }
}

public enum ApprovalStage
{
    Submitted,
    PrimaryApprover,
    SecondaryApprover,
    ModuleAdmin,
    Rejected,
    OnHold,
    MoneyReload,
    TravelAdmin,
    BehalfOfEmpByTravelAdmin
}

public class ApprovalStatements
{
    public string submitted { get; set; }
    public string primaryApprover { get; set; }
    public string secondaryApprover { get; set; }
    public string finance { get; set; }
    public string rejected { get; set; }
    public string onHold { get; set; }
    public string moneyreload { get; set; }
    public string traveladmin { get; set; }
    public string BehalfOfEmpByTravelAdmin { get; set; }

}

public enum CustomerMailAction
{
    Creation,
    Update,
    CustomerContractEndDatereminder
}

public enum ProjectMailAction
{
    Creation,
    Update,
    ProjectEndDatereminder
}


public enum InvoiceMailAction
{
    Submitted,
    Approved,
    Rejected,
    OnHold
}

public enum ResourceAllocationMailAction
{
    Raised,
    Updated,
    AllocationUpdate,
    Action,
    Release,
    EndDateRemider
}



using log4net;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using System.Data.Entity;

namespace AbsenteeTracingJob
{
    class AbsentMailSender
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;

        static void Main(string[] args)
        {
            _container = UnityRegister.LoadContainer();

            var opsService = _container.Resolve<BasicOperationsService>();
            PhoenixEntities entites = new PhoenixEntities();
            //IBasicOperationsService _service=new IBasicOperationsService();

            Log4Net.Debug("Absent Tracing job started: =" + DateTime.Now);

            try
            {
                var empList = entites.CheckConsecutiveAbsentee().ToList();

                foreach (var item in empList)
                {
                    string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                    var personData = opsService.First<Person>(x => x.ID == item.PersonID && x.Active == true);
                    int EPMId = personData.PersonEmployment.FirstOrDefault().ExitProcessManager.Value;

                    var reportingMgr = opsService.First<PersonReporting>(x => x.PersonID == item.PersonID && x.Active == true && x.IsDeleted == false).ReportingTo;
                    var reportingMgrData = opsService.First<Person>(x => x.ID == reportingMgr && x.Active == true);

                    DateTime DOJ = Convert.ToDateTime(personData.PersonEmployment.FirstOrDefault().JoiningDate);
                    string imagePath = personData.Image;
                    DateTime currDate = DateTime.Now.Date;

                    var epmData = opsService.First<Person>(x => x.ID == EPMId && x.Active == true);
                    string baseUrl = Convert.ToString(ConfigurationManager.AppSettings["baseUrl"]);

                    DateTime? dt = opsService.Top<SignInSignOut>(0, x => x.UserID == item.PersonID && x.DayNotation == "P" && DbFunctions.TruncateTime(x.AttendanceDate) < currDate).OrderByDescending(x => x.SignInSignOutID).Select(x => x.AttendanceDate).FirstOrDefault();
                    //DateTime? dt = data[0].AttendanceDate;
                    DateTime? lastpresentdt;
                    
                    if (dt != null)
                    {
                        lastpresentdt = opsService.Top<SignInSignOut>(0, x => x.UserID == item.PersonID && x.DayNotation == "A" && x.AttendanceDate.Value > dt.Value).OrderBy(x => x.AttendanceDate).Select(x => x.AttendanceDate).FirstOrDefault();
                    }
                    else
                    {
                        lastpresentdt = opsService.Top<SignInSignOut>(0, x => x.UserID == item.PersonID && x.DayNotation == "A").OrderBy(x => x.AttendanceDate).Select(x => x.AttendanceDate).FirstOrDefault();
                    }
                    DateTime lastpresent = Convert.ToDateTime(lastpresentdt);

                    var isEmployeeResign = opsService.Top<Separation>(0, c => c.PersonID == item.PersonID && c.StatusID != 1 && c.StatusID != 5).ToList();


                    //var isEmployeeResign = opsService.First<Separation>(c => c.PersonID == item.PersonID && c.StatusID != 1 && c.StatusID != 5).ResignDate;

                    string emailTo = reportingMgrData.PersonEmployment.First().OrganizationEmail;
                    string emailCC = epmData.PersonEmployment.First().OrganizationEmail;
                    string rmg = "";
                    rmg = entites.HelpDeskCategories.Where(x => x.AssignedRole == 27).FirstOrDefault().EmailGroup;
                    string employeeEmail = "";
                    employeeEmail = personData.PersonEmployment.First().OrganizationEmail;
                    emailCC = emailCC + "," + rmg + "," + employeeEmail;
                    var templateContent = EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.EmployeeNotReporting));

                    var template1 = opsService.First<EmailTemplate>(x => x.TemplateFor == templateContent);
                    string template = template1.Html;
                    template = template.Replace("{{date}}", DateTime.Today.ToShortDateString());
                    template = template.Replace("{{username}}", personData.FirstName + " " + personData.LastName);
                    template = template.Replace("{{employeeid}}", Convert.ToString(personData.ID));
                    template = template.Replace("{{imagename}}", baseUrl + imagePath);

                    if (isEmployeeResign.Count() == 0)
                        template = template.Replace("{{dateofjoining}}", "Joined On: " + (DOJ.ToString("dd MMMM,yyyy")));
                    //template = template.Replace("{{dateofjoining}}", "(" + (DOJ.ToString("dd MMMM,yyyy") + ")"));
                    else
                        template = template.Replace("{{dateofjoining}}", "Resigned On: " + (isEmployeeResign[0].ResignDate.ToString("dd MMMM,yyyy")));

                    template = template.Replace("{{lastpresent}}", (lastpresent.ToString("dd MMMM,yyyy")));
                    template = template.Replace("{{loggedinuser}}", "Vibrant Desk");

                    bool isCreated = opsService.Create<Emails>(new Emails
                    {
                        Content = template,
                        Date = DateTime.Now,
                        EmailFrom = emailFrom,
                        EmailTo = emailTo,
                        EmailCC = emailCC,
                        Subject = personData.FirstName + " " + personData.LastName + "-" + personData.ID + " is not reporting to office",
                    }, e => e.Id == 0);

                    if (isCreated)
                        opsService.Finalize(true);

                }
                //entites.Dispose();
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
                //entites.Dispose();
            }

            Log4Net.Debug("Absent Tracing job finished: =" + DateTime.Now);
        }
    }
}



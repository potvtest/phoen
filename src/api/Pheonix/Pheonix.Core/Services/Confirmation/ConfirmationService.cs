using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.Confirmation;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace Pheonix.Core.Services.Confirmation
{
    public class ConfirmationService : IConfirmationService
    {
        private IEmailService _EmailService;
        private IBasicOperationsService _BasicService;
        private IContextRepository _repo;
        private IPrintReportInPDF _PrintReport;
        public int _UserId { get; set; }

        public ConfirmationService(IContextRepository repository, IEmailService emailService, IBasicOperationsService basicService, IPrintReportInPDF printReport, IApprovalService approvalService)
        {
            _repo = repository;
            _EmailService = emailService;
            _BasicService = basicService;
            _PrintReport = printReport;
        }

        /// <summary>
        /// Get the list of confirmations applicable to logged in user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="additionalParam"></param>
        /// <returns></returns>
        public async Task<PersonConfirmationViewModel> List(int userId, string additionalParam = "")
        {
            PersonConfirmationViewModel viewModel = new PersonConfirmationViewModel() { EditStyle = GetEditStyle(userId) };
            List<int> approvalIds = new List<int>();
            List<PersonInRole> personRole = _repo.GetAll<PersonInRole>().Where(p => p.PersonID == userId).ToList();
            int ConfirmationRole = Convert.ToInt32((ConfigurationManager.AppSettings["ConfirmationRole"]).ToString());
            bool hasConfirmationRole = personRole.Any(p => p.RoleID == ConfirmationRole);
            if (hasConfirmationRole)
            {
                approvalIds = GetPersonConfirmationIdForConfirmationRole(userId);
            }
            else
            {
                approvalIds = GetPersonConfirmationIdFromApproval(userId);
            }


            var getPersonsConfirmation = await Task.Run(() => _repo.GetAll<PersonConfirmation>()
                .Where(t => t.IsHRReviewDone == false && approvalIds.Contains(t.ID)
                    && t.Person != null && (additionalParam == "" || t.Person.FirstName == additionalParam || t.Person.LastName == additionalParam))
                .ToList());

            foreach (var person in getPersonsConfirmation)
            {
                Confirmations confirmation = Mapper.Map<PersonConfirmation, Confirmations>(person);
                confirmation.ProbationReviewDate = GetProbationReviewDate(person.Person);
                confirmation.EditStyle = GetEditStyle(userId, person.ID);
                confirmation.Feedback = Mapper.Map<PersonConfirmation, ConfirmationFeedback>(person);
                confirmation.Employee = Mapper.Map<Person, EmployeeBasicProfile>(person.Person);
                viewModel.Confirmations.Add(confirmation);
            }
            return viewModel;
        }

        DateTime GetProbationReviewDate(Person person)
        {
            var personEmployment = person.PersonEmployment.FirstOrDefault();
            if (personEmployment != null && personEmployment.ProbationReviewDate != null)
                return personEmployment.ProbationReviewDate.Value;
            return default(DateTime);
        }

        List<int> GetPersonConfirmationIdFromApproval(int userId)
        {
           // PhoenixEntities context = new PhoenixEntities();
           // var approvalList = QueryHelper.GetApprovalsForUser2(context, userId, 9);
            var approvalList = QueryHelper.GetApprovalsForUser2(userId, 9);
            return approvalList;
        }

        List<int> GetPersonConfirmationIdForConfirmationRole(int userId)
        {
            //PhoenixEntities context = new PhoenixEntities();
            var approvalList = QueryHelper.GetApprovalsForConfirmationRole(9);
            return approvalList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        async Task<ConfirmationFeedback> GetFeedback(Person person)
        {
            var feedback = await Task.Run(() => _repo.GetAll<PersonConfirmation>()
                    .FirstOrDefault(t => t.PersonId == person.ID));

            return Mapper.Map<PersonConfirmation, ConfirmationFeedback>(feedback);
        }

        /// <summary>
        /// Provides Edit style based on logged in user.
        /// </summary>
        /// <returns>
        /// 0 - Not Applicable
        /// 1 - Manager
        /// 2 - HR
        /// 3 - HR as Manager
        /// </returns>
        int GetEditStyle(int userId, int recordId = 0)
        {
            var personEmplyment = _repo.GetAll<PersonEmployment>().FirstOrDefault(p => p.PersonID == userId);
            int ConfirmationRole = Convert.ToInt32((ConfigurationManager.AppSettings["ConfirmationRole"]).ToString());
            if (personEmplyment != null && personEmplyment.Designation != null)
            {
                //bool isManager = (personEmplyment.Designation.Grade ?? 0) >= 4;
                var personConfirmation = _repo.GetAll<PersonConfirmation>().FirstOrDefault(p => p.ReportingManagerId == userId);
                bool isManager = false;
                if (personConfirmation != null)
                {
                    if (personConfirmation.ReportingManagerId != null && personConfirmation.ReportingManagerId > 0)
                    {
                        isManager = true;
                    }
                }
                var role = _repo.GetAll<PersonInRole>().Where(t => t.PersonID == userId).Select(t => t.RoleID).ToList();
                if (role != null)
                {
                    if (recordId != 0)
                    {
                        if (role.Contains(ConfirmationRole) && isManager)
                        {
                            var approvalDetails = _repo.GetAll<ApprovalDetail>().Where(t => t.ApproverID == userId && t.Approval.RequestID == recordId && t.Stage > 1).ToList();
                            if (approvalDetails.Count > 0)
                            {
                                return 2;
                            }
                        }
                    }

                    if (role.Contains(ConfirmationRole) && !isManager)
                    {
                        return 2;
                    }
                    else if (role.Contains(ConfirmationRole) && isManager)
                    {
                        return 3;
                    }
                    else if (!role.Contains(ConfirmationRole) && isManager)
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Get no of days confirmation review process should starts.
        /// </summary>
        /// <returns></returns>
        int GetNoOfDaysToCheck()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["ConfirmationCheckInDays"].ToString());
        }

        public async Task<Dictionary<int, string>> GetRecommendations()
        {
            Dictionary<int, string> recommendations = new Dictionary<int, string>();
            recommendations.Add(0, "--Select--");
            recommendations.Add(1, "Accept Confirmation");
            recommendations.Add(2, "Reject Confirmation");
            recommendations.Add(3, "Extend Confirmation");
            recommendations.Add(4, "PIP");
            return recommendations;
        }
        public async Task<object> Initiate()
        {

            int editStyle = GetEditStyle(0);

            DateTime dateToCheck = DateTime.Today.AddDays(GetNoOfDaysToCheck());

            DateTime startDateToCheck = DateTime.Today;


            DateTime configStartDate = DateTime.Now;// Convert.ToDateTime(ConfigurationManager.AppSettings["ConfirmationCheckInStartDate"].ToString());

            if (startDateToCheck <= configStartDate && dateToCheck >= configStartDate)
            {
                startDateToCheck = configStartDate;
            }
            else if (dateToCheck == configStartDate)
            {
                startDateToCheck = configStartDate;
                dateToCheck = configStartDate.AddDays(GetNoOfDaysToCheck());
            }

            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var getPersons = _repo.GetAll<Person>()
                            .Where(t => t.Active == true && t.PersonEmployment
                                .FirstOrDefault(k => (k.EmploymentStatus == 2 || k.EmploymentStatus == 12 || k.EmploymentStatus == 13)
                                  && k.ProbationReviewDate.Value >= startDateToCheck.Date && k.ProbationReviewDate.Value <= dateToCheck.Date) != null).ToList();

                        if (getPersons.Count > 0)
                        {
                            foreach (var person in getPersons)
                            {
                                if (person.PersonConfirmation.FirstOrDefault(t => !t.IsHRReviewDone.Value) == null)
                                {
                                    //var approver = GetManager(person.ID);
                                    var approver = GetExitProcessManager(person.ID);

                                    Confirmations confirmation = new Confirmations();
                                    confirmation.ProbationReviewDate = GetProbationReviewDate(person);
                                    confirmation.PersonId = person.ID;
                                    confirmation.Employee = Mapper.Map<Person, EmployeeBasicProfile>(person);
                                    confirmation.JoiningDate = person.PersonEmployment.FirstOrDefault().JoiningDate;

                                    confirmation.ReportingManagerId = approver.ID;
                                    confirmation.ReportingManager = approver.Name;

                                    var PIC = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                                    PIC = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, PIC);
                                    PIC.Person = person;

                                    _repo.Create<PersonConfirmation>(PIC, x => x.ID == 0);
                                    try
                                    {
                                        _repo.Save();
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }

                                    await InitiateIntimations(PIC, editStyle);
                                }
                            }
                            transaction.Commit();
                        }
                    }
                    catch
                    { }
                }

            }
            return "Success";
        }
        async Task InitiateIntimations(PersonConfirmation confirmation, int editStyle)
        {
            if (confirmation.Person != null)
            {
                try
                {
                    if (editStyle == 0 && string.IsNullOrEmpty(confirmation.BehaviourFeedback))
                        await InitiateApprovalProcess(confirmation);
                }
                catch
                {

                }
                await InitiateEmails(confirmation, editStyle, false);
            }
        }

        async Task<int> InitiateApprovalProcess(PersonConfirmation confirmation)
        {
            var data = 0;
            await Task.Run(() =>
            {
                //var approver = GetManager(confirmation.PersonId ?? 0);
                var approver = GetExitProcessManager(confirmation.PersonId ?? 0);
                int[] approvalList = new int[] { approver.ID ?? 0 };

                ApprovalService service = new ApprovalService(this._BasicService);

                data = service.SendForApproval((confirmation.PersonId ?? 0), 9, confirmation.ID, approvalList);
                return data;
            });

            return data;
        }
        async Task InitiateEmails(PersonConfirmation confirmation, int editStyle, bool isHR)
        {
            _EmailService.SendConfirmationEmail(confirmation, editStyle, isHR);
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

        public async Task<object> Confirm(Confirmations confirmation)
        {
            return await UpdateConfirmation(confirmation);
        }
        public async Task<object> Reject(Confirmations confirmation)
        {
            return await UpdateConfirmation(confirmation);
        }
        public async Task<object> PIP(Confirmations confirmation)
        {
            return await UpdateConfirmation(confirmation);
        }
        public async Task<object> Extend(Confirmations confirmation)
        {
            return await UpdateConfirmation(confirmation);
        }
        public async Task<bool> UpdateConfirmation(Confirmations confirmation)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var personConfirmation = _repo.GetAll<PersonConfirmation>().FirstOrDefault(t => t.ID == confirmation.ID);
                    var confirmationModel = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                    confirmationModel = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, confirmationModel);
                    confirmationModel.Person = personConfirmation.Person;
                    confirmationModel.PersonId = personConfirmation.PersonId;

                    _repo.Update<PersonConfirmation>(confirmationModel, personConfirmation);

                    _repo.Save();
                    transaction.Commit();

                    ApprovalService service = new ApprovalService(this._BasicService);

                    var data = await Task.Run(() => service.UpdateMultiLevelApproval(confirmation.PersonId, 9, confirmation.ID,
                        GetApprovalStatusID(confirmation.Feedback.ConfirmationState), GetApprovalStatus(confirmation.Feedback.ConfirmationState), _UserId));

                    if (confirmation.EditStyle != 2)
                        await InitiateEmails(personConfirmation, confirmation.EditStyle, false);

                    return true;
                }
            }
        }

        public async Task<bool> UpdateConfirmationByHr(Confirmations confirmation)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var personConfirmation = _repo.GetAll<PersonConfirmation>().FirstOrDefault(t => t.ID == confirmation.ID);
                    var confirmationModel = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                    confirmationModel = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, confirmationModel);
                    confirmationModel.Person = personConfirmation.Person;
                    confirmationModel.PersonId = personConfirmation.PersonId;

                    _repo.Update<PersonConfirmation>(confirmationModel, personConfirmation);

                    _repo.Save();
                    transaction.Commit();

                    if (confirmation.EditStyle != 2)
                        await InitiateEmails(personConfirmation, confirmation.EditStyle, false);

                    return true;
                }
            }
        }

        async Task UpdateEmployeeStatus(Confirmations confirmation)
        {
            switch (confirmation.Feedback.ConfirmationState)
            {
                case 1:
                    await ConfirmEmplyee(confirmation);
                    break;
                case 2:
                    await RejectEmployee(confirmation);
                    break;
                case 3:
                    await ExtendEmployee(confirmation);
                    break;
                case 4:
                    await PIPEmployee(confirmation);
                    break;
                default:
                    break;
            }
        }
        async Task ConfirmEmplyee(Confirmations confirmation)
        {
            var employee = _repo.GetAll<PersonEmployment>().FirstOrDefault(t => t.PersonID == confirmation.PersonId);
            employee.ConfirmationDate = employee.ProbationReviewDate;
            employee.EmploymentStatus = 1;
            await UpdateEmploymentStatusDate(confirmation, employee);
            _repo.Update<PersonEmployment>(employee);
            _repo.Save();
        }
        async Task RejectEmployee(Confirmations confirmation)
        {

        }
        async Task ExtendEmployee(Confirmations confirmation)
        {
            var employee = _repo.GetAll<PersonEmployment>().FirstOrDefault(t => t.PersonID == confirmation.PersonId);
            if (employee.ProbationReviewDate != null)
            {
                employee.ProbationReviewDate = confirmation.ProbationReviewDate.Value.AddDays(confirmation.Feedback.ExtendedTill);
                employee.ProbationReviewDate = employee.ProbationReviewDate.Value.AddDays(-1);
            }
            employee.EmploymentStatus = 13;
            await UpdateEmploymentStatusDate(confirmation, employee);
            _repo.Update<PersonEmployment>(employee);
            _repo.Save();
        }
        async Task PIPEmployee(Confirmations confirmation)
        {
            var employee = _repo.GetAll<PersonEmployment>().FirstOrDefault(t => t.PersonID == confirmation.PersonId);
            if (employee.ProbationReviewDate != null)
            {
                employee.ProbationReviewDate = confirmation.ProbationReviewDate.Value.AddDays(confirmation.Feedback.PIPTill);
                employee.ProbationReviewDate = employee.ProbationReviewDate.Value.AddDays(-1);
            }
            employee.EmploymentStatus = 12;
            await UpdateEmploymentStatusDate(confirmation, employee);
            _repo.Update<PersonEmployment>(employee);
            _repo.Save();
        }
        async Task UpdateEmploymentStatusDate(Confirmations confirmation, PersonEmployment employee)
        {
            if ((employee.EmploymentStatusDate == null || employee.EmploymentStatusDate == new DateTime(0)) && (employee.ProbationReviewDate != null))
                employee.EmploymentStatusDate = employee.ProbationReviewDate.Value.AddDays(1);
            else if (employee.EmploymentStatusDate > employee.ProbationReviewDate)
                employee.EmploymentStatusDate = employee.EmploymentStatusDate.Value.AddDays(1);
            else if ((employee.EmploymentStatusDate <= employee.ProbationReviewDate) && (employee.ProbationReviewDate != null))
                employee.EmploymentStatusDate = employee.ProbationReviewDate.Value.AddDays(1);
            else
                employee.EmploymentStatusDate = DateTime.Today.Date;
        }
        int GetApprovalStatusID(int confirmationStatus)
        {
            return 1;
            //switch (confirmationStatus)
            //{
            //    case 1:
            //        return 4;
            //    case 2:
            //        return 5;
            //    case 3:
            //        return 6;
            //    case 4:
            //        return 7;
            //}
            //return 0;
        }
        string GetApprovalStatus(int confirmationStatus)
        {
            switch (confirmationStatus)
            {
                case 1:
                    return "Confirmed";
                case 2:
                    return "Rejected";
                case 3:
                    return "Extended";
                case 4:
                    return "PIP";
            }
            return string.Empty;
        }
        public async Task<HttpResponseMessage> Print(Confirmations confirmation, System.Web.HttpResponse resp)
        {
            if (!confirmation.Feedback.IsHRReviewDone)
            {
                confirmation.Feedback.IsHRReviewDone = true;
                //await UpdateConfirmation(confirmation);
                await UpdateConfirmationByHr(confirmation);
                await UpdateEmployeeStatus(confirmation);

                var confirmationModel = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                confirmationModel = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, confirmationModel);
                var personConfirmation = _repo.GetAll<PersonConfirmation>().FirstOrDefault(t => t.ID == confirmation.ID);
                confirmationModel.Person = personConfirmation.Person;
                confirmationModel.PersonId = personConfirmation.PersonId;

                await InitiateEmails(confirmationModel, confirmation.EditStyle, true);

            }
            return await GenerateDocument(confirmation, resp);
            //return data;
        }
        public async Task<HttpResponseMessage> PrintDoc(Confirmations confirmation, int confirmationId)
        {
            #region dummydata
            var personConfirmation = _repo.FirstOrDefault<PersonConfirmation>(t => (confirmationId == 0 || t.ID == confirmationId) && !string.IsNullOrEmpty(t.BehaviourFeedback));
            if (personConfirmation != null)
            {
                confirmation = Mapper.Map<PersonConfirmation, Confirmations>(personConfirmation);

                confirmation.Feedback = Mapper.Map<PersonConfirmation, ConfirmationFeedback>(personConfirmation);
                confirmation.Employee = Mapper.Map<Person, EmployeeBasicProfile>(personConfirmation.Person);
            }

            if (personConfirmation.IsHRReviewDone == false)
                return null;
            #endregion

            var doc = await GenerateDocument(confirmation, null);
            return doc;
        }
        async Task<HttpResponseMessage> GenerateDocument(Confirmations confirmation, System.Web.HttpResponse resp)
        {
            if (confirmation != null)
            {
                string reportName = string.Empty;
                switch (confirmation.Feedback.ConfirmationState)
                {
                    case 1:
                        reportName = "ConfirmationLetter";
                        break;
                    case 2:
                        reportName = "ProbationExtensionLetter";
                        break;
                    case 3:
                        reportName = "ProbationExtensionLetter";
                        break;
                    case 4:
                        reportName = "ProbationExtensionLetter";
                        break;
                    case 5:
                        reportName = "ProbationExtensionLetter";
                        break;
                }

                return await _PrintReport.GetPDFPrint(_repo, confirmation, reportName);
            }
            return null;
        }
        public async Task<AdminActionResult> SubmitPIP(Confirmations confirmation)
        {
            AdminActionResult result = new AdminActionResult();
            if (!confirmation.Feedback.IsHRReviewDone)
            {
                confirmation.Feedback.IsHRReviewDone = true;
                //await UpdateConfirmation(confirmation);
                await UpdateConfirmationByHr(confirmation);
                await UpdateEmployeeStatus(confirmation);

                var confirmationModel = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                var personConfirmation = _repo.GetAll<PersonConfirmation>().FirstOrDefault(t => t.ID == confirmation.ID);
                confirmationModel.Person = personConfirmation.Person;
                confirmationModel.PersonId = personConfirmation.PersonId;
                confirmationModel = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, confirmationModel);


                await InitiateEmails(confirmationModel, confirmation.EditStyle, true);
            }
            result.isActionPerformed = true;
            result.message = "success";
            return result;
        }
        public async Task<object> AutoConfirmEmployee()
        {
            int days = CheckDates();

            for (int i = 1; i <= days; i++)
            {
                DateTime daytoCheck = DateTime.Now.Date.AddDays(-i);

                using (PhoenixEntities context = new PhoenixEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        var getPersons = _repo.GetAll<Person>().Where(t => t.Active == true && t.PersonEmployment.FirstOrDefault(k => k.EmploymentStatus == 2 && k.ProbationReviewDate.Value == daytoCheck) != null).ToList();

                        if (getPersons.Count > 0)
                        {
                            foreach (var person in getPersons)
                            {
                                var personConfirmation = context.PersonConfirmation.Where(x => x.PersonId == person.ID).FirstOrDefault(t => !t.IsHRReviewDone.Value && t.ConfirmationState != 3 && t.ConfirmationState != 4);
                                if (personConfirmation != null)
                                {
                                    //Update Confirmation record
                                    personConfirmation.ConfirmationState = 1;
                                    var confirmation = Mapper.Map<PersonConfirmation, Confirmations>(personConfirmation);
                                    confirmation.Feedback.BehaviourFeedback = "Deemed Confirmation";
                                    confirmation.Feedback.TrainingFeedback = "Deemed Confirmation";
                                    confirmation.Feedback.OverallFeedback = "Deemed Confirmation";
                                    confirmation.Feedback.ConfirmationState = 1;
                                    confirmation.Feedback.IsHRReviewDone = true;
                                    confirmation.ReviewDate = DateTime.Today;
                                    _UserId = confirmation.ReportingManagerId ?? 0;
                                    confirmation.EditStyle = 2;

                                    var personConfirmation1 = _repo.GetAll<PersonConfirmation>().FirstOrDefault(t => t.ID == confirmation.ID);
                                    var confirmationModel = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                                    confirmationModel = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, confirmationModel);
                                    confirmationModel.Person = personConfirmation1.Person;
                                    confirmationModel.PersonId = personConfirmation1.PersonId;

                                    _repo.Update<PersonConfirmation>(confirmationModel, personConfirmation1);
                                    _repo.Save();
                                    //transaction.Commit();


                                    //Update First Level approval
                                    ApprovalService service = new ApprovalService(this._BasicService);
                                    var data = service.UpdateMultiLevelApproval(confirmation.PersonId, 9, confirmation.ID,
                                        GetApprovalStatusID(confirmation.Feedback.ConfirmationState), GetApprovalStatus(confirmation.Feedback.ConfirmationState), _UserId);



                                    //Update Second Level approval
                                    UpdateSecondLevelAprovalConfirmation(context, person, confirmation);



                                    //Update Employment status
                                    var employee = _repo.GetAll<PersonEmployment>().FirstOrDefault(t => t.PersonID == confirmation.PersonId);
                                    employee.ConfirmationDate = employee.ProbationReviewDate;
                                    employee.EmploymentStatus = 1;
                                    UpdateEmploymentStatusDate(confirmation, employee).Wait();
                                    _repo.Update<PersonEmployment>(employee);
                                    _repo.Save();



                                    //Making model Initiating Emails and Document.
                                    var confirmationModel1 = Mapper.Map<Confirmations, PersonConfirmation>(confirmation);
                                    confirmationModel1 = Mapper.Map<ConfirmationFeedback, PersonConfirmation>(confirmation.Feedback, confirmationModel1);
                                    var personConfirmationn = _repo.GetAll<PersonConfirmation>().FirstOrDefault(t => t.ID == confirmation.ID);
                                    confirmationModel1.Person = personConfirmationn.Person;
                                    confirmationModel1.PersonId = personConfirmationn.PersonId;



                                    //Initiate Email
                                    InitiateEmails(confirmationModel1, confirmation.EditStyle, true).Wait();



                                    //Generate Document on Server
                                    GenerateDeemedDocument(confirmation).Wait();
                                }

                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            return "Success";
        }
        public int CheckDates()
        {
            DateTime daytoCheck = DateTime.Now.Date;
            int count = 1;
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var holidayList = context.HolidayList.Where(x => x.Location == 0);


                foreach (var holiday in holidayList)
                {
                    if (daytoCheck.AddDays(-count).Date.CompareTo(holiday.Date) == 0)
                        count++;
                }


                if (daytoCheck.AddDays(-count).DayOfWeek == DayOfWeek.Saturday || daytoCheck.AddDays(-count).DayOfWeek == DayOfWeek.Sunday)
                {
                    count++;

                    if (daytoCheck.AddDays(-count).DayOfWeek == DayOfWeek.Saturday || daytoCheck.AddDays(-count).DayOfWeek == DayOfWeek.Sunday)
                    {
                        count++;

                        foreach (var holiday in holidayList)
                        {
                            if (daytoCheck.AddDays(-count).Date.CompareTo(holiday.Date) == 0)
                                count++;
                        }

                    }

                }
            }

            return count;
        }
        public Task<object> GenerateDeemedDocument(Confirmations confirmation)
        {
            if (confirmation != null)
            {
                string reportName = string.Empty;
                switch (confirmation.Feedback.ConfirmationState)
                {
                    case 1:
                        reportName = "ConfirmationLetter";
                        break;
                }

                return _PrintReport.SaveDeemedPDF(_repo, confirmation, reportName);
            }
            return null;
        }

        public bool UpdateSecondLevelAprovalConfirmation(PhoenixEntities context, Person person, Confirmations confirmation)
        {
            var old = context.Approval.Where(x => x.Person.ID == person.ID && x.RequestType == 9 && x.RequestID == confirmation.ID && x.IsDeleted == false).FirstOrDefault();
            var oldDetail = context.ApprovalDetail.Where(x => x.ApprovalID == old.ID).ToList();
            var secondApproverID = oldDetail.Where(x => x.ApprovalID == old.ID && x.Status == 0).FirstOrDefault();

            ApprovalService service = new ApprovalService(this._BasicService);
            var data = service.UpdateMultiLevelApproval(confirmation.PersonId, 9, confirmation.ID, GetApprovalStatusID(confirmation.Feedback.ConfirmationState), GetApprovalStatus(confirmation.Feedback.ConfirmationState), Convert.ToInt32(secondApproverID.ApproverID));
            return true;
        }
        public async Task<object> SendConfirmationReminderMail()
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var reportingManager = context.PersonEmployment.Where(x => x.Designation.Grade >= 3 && x.Person.Active == true).Distinct().OrderByDescending(x => x.ID).ToList();

                    foreach (var result in reportingManager)
                    {
                        var pendingConfirmation = context.PersonConfirmation.Where(x => x.ConfirmationState == 0 && x.InitiatedDate < DateTime.Now && x.ReportingManagerId == result.PersonID).ToList();
                        if (pendingConfirmation.Count != 0)
                        {
                            InitiateReminderEmails(context, pendingConfirmation);
                        }
                    }
                    transaction.Commit();
                }
            }

            return "Success";
        }
        public void InitiateReminderEmails(PhoenixEntities context, List<PersonConfirmation> confirmation)
        {
            _EmailService.InitiateReminderEmails(context, confirmation);
        }
        public async Task<PersonConfirmationViewModel> ConfrimationHistory(int userId, bool isHR)
        {
            return await Task.Run(() =>
            {
                IEnumerable<PersonConfirmation> confirmations;
                PersonConfirmationViewModel viewModel = new PersonConfirmationViewModel();

                if (isHR)
                {
                    confirmations = _repo.GetAll<PersonConfirmation>().Where(x => x.IsHRReviewDone == true).ToList();
                }
                else
                {
                    confirmations = _repo.GetAll<PersonConfirmation>().Where(x => x.ReportingManagerId == userId && x.ConfirmationState != 0).ToList();
                }

                var getPersonsConfirmation = confirmations;

                foreach (var person in getPersonsConfirmation)
                {
                    Confirmations confirmation = Mapper.Map<PersonConfirmation, Confirmations>(person);
                    confirmation.ProbationReviewDate = GetProbationReviewDate(person.Person);
                    confirmation.EditStyle = GetEditStyle(userId, person.ID);
                    confirmation.Feedback = Mapper.Map<PersonConfirmation, ConfirmationFeedback>(person);
                    confirmation.Employee = Mapper.Map<Person, EmployeeBasicProfile>(person.Person);
                    viewModel.Confirmations.Add(confirmation);
                }

                return viewModel;
            });
        }
        public async Task<PersonConfirmationViewModel> InitiatedHistory(int userId)
        {
            return await Task.Run(() =>
            {
                PersonConfirmationViewModel viewModel = new PersonConfirmationViewModel();

                var pendingConfirmation = _repo.GetAll<PersonConfirmation>().Where(x => x.ConfirmationState == 0 && x.InitiatedDate <= DateTime.Now).ToList();

                foreach (var person in pendingConfirmation)
                {
                    Confirmations confirmation = Mapper.Map<PersonConfirmation, Confirmations>(person);
                    confirmation.ProbationReviewDate = GetProbationReviewDate(person.Person);
                    confirmation.EditStyle = GetEditStyle(userId, person.ID);
                    confirmation.Feedback = Mapper.Map<PersonConfirmation, ConfirmationFeedback>(person);
                    confirmation.Employee = Mapper.Map<Person, EmployeeBasicProfile>(person.Person);
                    viewModel.Confirmations.Add(confirmation);
                }

                return viewModel;
            });
        }
        public bool CheckIfWorkingDay()
        {
            if (DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

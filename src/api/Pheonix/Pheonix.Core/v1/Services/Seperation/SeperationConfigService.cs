using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.Repository;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using System.Linq;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;

namespace Pheonix.Core.v1.Services
{
    public class SeperationConfigService : ISeperationConfigService
    {
        private ISeperationConfigRepository _seperationConfigRepository;
        private IApprovalService _ApprovalService;
        private IBasicOperationsService _BasicService;
        private IEmailService _EmailService;
        public int UserId { get; set; }
        private PhoenixEntities _phoenixEntity;

        public SeperationConfigService(ISeperationConfigRepository seperationConfigRepository, IApprovalService approvalService,
            IEmailService emailService, IBasicOperationsService basicService)
        {
            _seperationConfigRepository = seperationConfigRepository;
            _ApprovalService = approvalService;
            _BasicService = basicService;
            _EmailService = emailService;
        }

        public SeperationConfigService()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        #region Implementation of ISeperationConfigService
        public IEnumerable<SeperationConfigViewModel> GetList(string filters)
        {
            var seperationconfigList = _seperationConfigRepository.GetList(filters);

            return seperationconfigList;
        }

        public ActionResult Add(SeperationConfigViewModel model)
        {
            var statusCode = _seperationConfigRepository.Add(model);
            return statusCode;
        }

        public ActionResult Update(SeperationConfigViewModel model)
        {
            var statusCode = _seperationConfigRepository.Update(model);
            return statusCode;
        }

        public ActionResult Delete(int id)
        {
            return _seperationConfigRepository.Delete(id);
        }

        public Task<List<DropdownItems>> GetRoleList()
        {
            //return Mapper.Map<IEnumerable<Role>, IEnumerable<RoleViewModel>>(_seperationConfigRepository.GetRoleList());
            return _seperationConfigRepository.GetRoleList();
        }
        #endregion

        //TODO: Need to remove after testing
        #region Email Notification
        void RaiseEmailForResignation(SeperationViewModel model)
        {
            string emailTo = string.Empty, emailCC = string.Empty, subject = string.Empty, body = string.Empty;
            GetEmailToCC(model, out emailTo, out emailCC);
            subject = GetEmailSubject(model);
            body = GetEmailMessage(model);
            // _EmailService.SendResignationEmail(model, subject, body,);
        }
        //TODO: Need to remove after testing
        string GetEmailMessage(SeperationViewModel model)
        {
            switch (model.StatusID)
            {
                case 0:
                    return "I would like to resign from the company and services.";

                case 2:
                    return "Employee resignation accepted.";

                case 3:
                    return "Separation Release Process intitiated, respective departments will communicate you to complete exit process.";

                default:
                    return "Separation review process is completed, HR will update you for further documentation.";
            }
        }
        //TODO: Need to remove after testing
        string GetEmailSubject(SeperationViewModel model)
        {
            switch (model.StatusID)
            {
                case 0:
                    return "Resigned from the designation and serives.";
                case 1:
                    return "Resignation withdrawal.";
                case 2:
                    return "Resignation accepted.";
                case 3:
                    return "Separation release process started.";
                default:
                    return "Separation process completed.";
            }
        }
        //TODO: Need to remove after testing
        string GetEmployeesEmailId(EmployeeBasicProfile employee)
        {
            if (employee != null)
            {
                return employee.Email;
            }
            return string.Empty;
        }

        //TODO: Need to remove after testing
        GetGroupHeadEmail_Result GetManager(int personId)
        {
            var manager = QueryHelper.GetGroupHeadEmail( personId,6);
            return manager;
        }

        //TODO: Need to remove after testing
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
        //TODO: Need to remove after testing
        void GetEmailToCC(SeperationViewModel separation, out string emailTo, out string emailCC)
        {
            emailTo = string.Empty;
            emailCC = string.Empty;

            string reportingManager = GetManager(separation.PersonID).organizationemail;
            string hrManagers = GetHREmailIds();
            string employeeEmail = GetEmployeesEmailId(separation.EmployeeProfile);

            //using (PhoenixEntities entites = new PhoenixEntities())
            //{
            //    reporting = entites.People.Where(x => x.ID == separation.ApprovalID).FirstOrDefault();
            //    reportingManager = reporting.PersonEmployment.First().OrganizationEmail;
            //}

            if (separation.StatusID == 0)
            {
                emailTo = reportingManager;
                emailCC = string.Concat(employeeEmail, ";", hrManagers);
            }
            else if (separation.StatusID == 1)
            {
                emailTo = employeeEmail;
                emailCC = string.Concat(reportingManager, ";", hrManagers);
            }
            else if (separation.StatusID == 2)
            {
                emailTo = employeeEmail;
                emailCC = string.Concat(reportingManager, ";", hrManagers);
            }
            else if (separation.StatusID == 3)
            {
                emailTo = employeeEmail;
                emailCC = string.Concat(reportingManager, ";", hrManagers);
            }
        }
        #endregion

        #region SeparationReasonMaster
        public ActionResult AddReason(SeparationReasonViewModel model)
        {
            model.CreatedBy = UserId;
            model.CreatedOn = DateTime.Now;
            var statusCode = _seperationConfigRepository.AddReason(model);
            return statusCode;
        }

        public ActionResult UpdateReason(SeparationReasonViewModel model)
        {
            model.UpdatedBy = UserId;
            model.UpdatedOn = DateTime.Now;
            var statusCode = _seperationConfigRepository.UpdateReason(model);
            return statusCode;
        }

        public ActionResult DeleteReson(int id)
        {
            return _seperationConfigRepository.DeleteReason(id);
        }

        public IEnumerable<SeparationReasonViewModel> GetReasonList(string filters)
        {
            var seperationconfigList = _seperationConfigRepository.GetReasonList(filters);

            return seperationconfigList;
        }
        #endregion
    }
}

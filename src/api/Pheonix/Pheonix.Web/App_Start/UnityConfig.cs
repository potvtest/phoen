using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using Pheonix.Core.Repository;
using Pheonix.Core.Repository.PMS;
using Pheonix.Core.Repository.SeparationCard;
using Pheonix.Core.Services;
using Pheonix.Core.Services.Confirmation;
using Pheonix.Core.Services.Invoice;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Admin;
using Pheonix.Core.v1.Services.AdminConfig;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.Email;
using Pheonix.Core.v1.Services.PMS;
using Pheonix.Core.v1.Services.Reports;
using Pheonix.Core.v1.Services.Seperation;
using Pheonix.Core.v1.Services.ValuePortal;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.DBContext.Repository.ValuePortal;
//using Pheonix.DBContext.Repository.ValuePortal;
using Pheonix.Web.Controllers;
using Pheonix.Web.Models;
using Pheonix.Web.Report;
using System.Data.Entity;
using System.Web.Http;
using System.Web.Mvc;
using Unity.WebApi;
using ValuePortal;
using Pheonix.Core.Repository.TARRFrequest;
using Pheonix.Core.v1.Services.KRA;
//using ValuePortal;
//using ValuePortal.Repository;
//using ValuePortal.Services;

namespace Pheonix.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<IPersonRepository, PersonRepository>();
            container.RegisterType<IPersonSkillMappingRepository, PersonSkillMappingRepository>();
            container.RegisterType<ISkillMatrixRepository, SkillMatrixRepository>();
            container.RegisterType<IPersonAddressRepository, PersonAddressRepository>();
            container.RegisterType<IPersonContactRepository, PersonContactRepository>();
            container.RegisterType<IPersonEmploymentRepository, PersonEmploymentRepository>();
            container.RegisterType<IPersonPersonalRepository, PersonPersonalRepository>();
            container.RegisterType<IProjectRepository, ProjectRepository>();
            container.RegisterType<IPMSService, PMSService>();
            container.RegisterType<IPMSRepository, PMSRepository>();
            container.RegisterType<IProjectService, ProjectService>();
            container.RegisterType<IPersonService, PersonService>();
            container.RegisterType<IPersonSkillMappingService, PersonSkillMappingService>();
            container.RegisterType<ISkillMatrixService, SkillMatrixService>();
            container.RegisterType<ICustomerService, CustomerService>();
            container.RegisterType<ICustomerRepository, CustomerRepository>();

            container.RegisterType<ICustomerAddressRepository, CustomerAddressRepository>();
            container.RegisterType<ICustomerAddressService, CustomerAddressService>();

            container.RegisterType<ICustomerContactPersonRepository, CustomerContactPersonRepository>();
            container.RegisterType<ICustomerContactPersonService, CustomerContactPersonService>();

            container.RegisterType<ICustomerContractRepository, CustomerContractRepository>();
            container.RegisterType<ICustomerContractService, CustomerContractService>();

            container.RegisterType<IPersonAddressService, PersonAddressService>();
            container.RegisterType<IPersonContactService, PersonContactService>();
            container.RegisterType<IPersonEmploymentService, PersonEmploymentService>();
            container.RegisterType<IPersonPersonalService, PersonPersonalService>();

            container.RegisterType<IEmployeeService, EmployeeService>();
            container.RegisterType<IUserMenuService, UserMenuService>();
            container.RegisterType<IContextRepository, ContextRepository<PhoenixEntities>>();
            container.RegisterType<IVPContextRepository, VPContextRepository<ValuePortalEntities>>();
            container.RegisterType<IBasicOperationsService, BasicOperationsService>();
            container.RegisterType<ISISOService, SISOService>();
            container.RegisterType<IJobSchedulerService, JobSchedulerService>();
            container.RegisterType<IExpenseService, ExpenseService>();
            container.RegisterType<INewExpenseService, NewExpenseService>();
            container.RegisterType<IEmployeeLeaveService, EmployeeLeaveService>();
            container.RegisterType<ISaveToStageService, SaveToStageService>();
            container.RegisterType<IComponentService, ComponentService>();
            //  container.RegisterType<IRRFService, RRFService>();
            container.RegisterType<IEmailService, EmailSendingService>();
            container.RegisterType<IApprovalService, ApprovalService>();
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<IHelpDeskService, HelpDeskService>();
            container.RegisterType<ITravelService, TravelService>();
            container.RegisterType<IAppraisalService, AppraisalService>();
            container.RegisterType<IAdminService, AdminService>();
            container.RegisterType<IEmailActionsService, EmailActionsService>();
            container.RegisterType<ISeperationConfigRepository, SeperationConfigRepository>();
            container.RegisterType<ISeperationConfigService, SeperationConfigService>();
            container.RegisterType<ISeparationCardService, SeparationCardService>();
            container.RegisterType<ISeparationCardRepository, SeparationCardRepository>();
            container.RegisterType<IConfirmationService, ConfirmationService>();
            container.RegisterType<IPrintReportInPDF, ReportPrinting>();
            container.RegisterType<IAdminConfigService, AdminConfigService>();
            container.RegisterType<DbContext, ApplicationDbContext>(new HierarchicalLifetimeManager());
            container.RegisterType<UserManager<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<IValuePortalService, ValuePortalService>();
            container.RegisterType<IMainOperationsService, MainOperationsService>();
            container.RegisterType<AccountController>(new InjectionConstructor());
            container.RegisterType<ITaskService, TaskService>();
            container.RegisterType<ITimesheetService, TimesheetService>();
            container.RegisterType<ICompOffExceptionService, CompOffExceptionService>();
            container.RegisterType<IResourceAllocationService, ResourceAllocationService>();
            container.RegisterType<IPrintSeparationReportInPDF, ReportPrinting>();
            container.RegisterType<IInvoiceService, InvoiceService>();
            container.RegisterType<Core.v1.Services.Business.ICelebreationListService, Core.v1.Services.Business.CelebreationListService>();
            container.RegisterType<ICandidateService, CandidateService>();
            container.RegisterType<ITARRFService, TARRFService>();
            container.RegisterType<ITARRFRepository, TARRFRepository>();
            container.RegisterType<IKRAService, KRAService>();

            //DependencyResolver.SetResolver(new Unity.Mvc4.UnityDependencyResolver(container));
            DependencyResolver.SetResolver(new Unity.Mvc4.UnityDependencyResolver(container));

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);


        }
    }
}
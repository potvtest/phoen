using Pheonix.Core.v1.Services.Business;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Expense;
using Pheonix.Web.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Pheonix.Web.Extensions;
using System.Security.Claims;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/newExpense"), Authorize]
    public class NewExpenseController : ApiController
    {
        private INewExpenseService service;
        static string fileUrl = ConfigurationManager.AppSettings["UploadedFileUrl"].ToString();

        public NewExpenseController(INewExpenseService service)
        {
            this.service = service;
        }

        [Route("delete-expense/{id}"), HttpGet]
        public async Task<bool> DeleteExpense(int id)
        {
            return await service.DeleteExpense(id);
        }

        [Route("save-update"), HttpPost]
        public async Task<bool> SaveOrUpdate(NewExpenseViewModel model)
        {
            return await service.SaveOrUpdate(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("get-all-list/{isDraft:bool?}"), HttpGet]
        public async Task<IEnumerable<NewExpenseViewModel>> GetAllList(bool isDraft = false)
        {
            return await service.GetAllList(isDraft, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("get-expense-list/{expenseId:int?}"), HttpGet]
        public async Task<IEnumerable<NewExpenseViewModel>> GetList(int expenseId = 0)
        {
            return await service.GetExpense(expenseId);
        }

        [Route("delete-expense-detail/{id}"), HttpGet]
        public async Task<bool> DeleteExpenseDetail(int id)
        {
            return await service.DeleteExpenseDetail(id);
        }

        [Route("add-detail/{expenseId}")]
        public async Task<bool> AddDetail(int expenseId)
        {
            return await Task.Run<bool>(() => { return true; });
        }

        [Route("download/{*url}"), HttpPost]
        public HttpResponseMessage Download(string url)
        {
            string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"].ToString();
            url = url.Replace(fileUrl, "").Replace("/", "\\").Insert(0, "\\");

            var path = uploadFolder + url;
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(path);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentLength = stream.Length;
            return result;
        }

        [Route("dropdowns"), HttpGet]
        public async Task<IHttpActionResult> GetDropDowns()
        {
            return Ok(await service.GetDropdowns(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("get-approvals"), HttpGet]
        public async Task<IEnumerable<NewExpenseViewModel>> GetApprovals()
        {
            return await service.GetApprovals(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("reject-expense"), HttpPost]
        public async Task<IHttpActionResult> RejectExpense(NewExpenseViewModel expense)
        {
            return Ok(await service.RejectExpense(expense, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("approve-expense"), HttpPost]
        public async Task<IHttpActionResult> ApproveExpense(NewExpenseViewModel expense)
        {
            var a = await service.ApproveExpense(expense.expenseId, expense.formCode, expense.chequeDetails, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return Ok(a);
        }


        [Route("on-hold-expense"), HttpPost]
        public async Task<IHttpActionResult> OnHoldExpense(NewExpenseViewModel model)
        {
            return Ok(await service.OnHoldExpense(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("approved-expense"), HttpGet]
        public async Task<IHttpActionResult> ApprovedExpenses()
        {
            return Ok(await service.ApprovalHistory(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("activity"), HttpGet]
        public IHttpActionResult GetLatestActivity()
        {
            return Ok(service.DashBoardExpenseCardView(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("save-update/expCategory"), HttpPost]
        public async Task<bool> SaveOrUpdate(ExpenseCategoryViewModel model)
        {
            return await service.SaveOrUpdate(model);
        }

        [Route("get/expCategory"), HttpGet]
        public async Task<IHttpActionResult> GetExpCategory()
        {
            return Ok(await service.GetExpCategoryList());
        }

        [Route("delete-expenseCategory/{id}"), HttpPost]
        public async Task<bool> DeleteExpenseCategory(int id)
        {
            return await service.DeleteExpenseCategory(id);
        }

        [HttpGet, Route("IsApprover")]
        public async Task<bool> IsApprover()
        {
            return await service.IsApprover(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("expense-reminder"),HttpPost]
        public async Task<bool> ExpenseReminderMail(ExpenseMail expReminder)
        {
            return await service.SendExpenseReminder(expReminder);

        }
    }
}
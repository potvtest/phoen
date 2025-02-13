using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using Pheonix.Web.Authorization;
using Pheonix.Web.Extensions;
using Pheonix.Core.Services.Invoice;
using Pheonix.Models;
using Pheonix.Models.VM.Classes.ResourceAllocation;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/invoice"), Authorize]
    public class InvoiceController : ApiController
    {
        private IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [Route("dropdowns"), HttpGet]
        public async Task<IHttpActionResult> GetDropDowns()
        {
            return Ok(await _invoiceService.GetDropdowns(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("save-update"), HttpPost]
        public async Task<int> SaveOrUpdate(InvoiceViewModel model)
        {
            return await _invoiceService.SaveOrUpdate(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("get-all-list/{isDraft:bool?}"), HttpGet]
        public async Task<IEnumerable<InvoiceViewModel>> GetAllList(bool isDraft = false)
        {
            return await _invoiceService.GetAllList(isDraft, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("get-invoice-list/{invoiceId:int?}"), HttpGet]
        public async Task<InvoiceViewModel> GetList(int invoiceId = 0)
        {
            return await _invoiceService.GetInvoice(invoiceId);
        }

        [Route("get-approvals"), HttpGet]
        public async Task<IEnumerable<InvoiceViewModel>> GetApprovals()
        {
            return await _invoiceService.GetApprovals(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("reject-invoice"), HttpPost]
        public async Task<IHttpActionResult> RejectInvoice(InvoiceViewModel invoice)
        {
            return Ok(await _invoiceService.RejectInvoice(invoice, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("approve-invoice"), HttpPost]
        public async Task<IHttpActionResult> ApproveInvoice(InvoiceViewModel invoice)
        {
            var flag = await _invoiceService.ApproveInvoice(invoice.Id, RequestContext.GetClaimInt(ClaimTypes.PrimarySid), invoice.InvoiceFormCode, invoice.Details);
            return Ok(flag);
        }


        [Route("on-hold-invoice"), HttpPost]
        public async Task<IHttpActionResult> OnHoldInvoice(InvoiceViewModel invoice)
        {
            return Ok(await _invoiceService.OnHoldInvoice(invoice, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("approved-invoice"), HttpGet]
        public async Task<IHttpActionResult> ApprovedInvoice()
        {
            return Ok(await _invoiceService.ApprovalHistory(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("delete-invoice/{id}"), HttpGet]
        public async Task<bool> DeleteInvoice(int id)
        {
            return await _invoiceService.DeleteInvoice(id);
        }

        [Route("delete-invoice-detail/{id}"), HttpGet]
        public async Task<bool> DeleteInvoiceDetail(int id)
        {
            return await _invoiceService.DeleteInvoiceDetails(id);
        }

        [HttpGet, Route("IsApprover")]
        public async Task<bool> IsApprover()
        {
            return await _invoiceService.IsApprover(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("savePayment"), HttpPost]
        public async Task<bool> SavePaymentDetails(InvoicePaymentsViewModel model)
        {
            return await _invoiceService.SavePaymentDetails(model);
        }

        [Route("getInvoicePayments/{invoiceId:int}"), HttpGet]
        public async Task<IEnumerable<InvoicePaymentsViewModel>> GetAllList(int invoiceId = 0)
        {
            return await _invoiceService.GetInvoicePayments(invoiceId);
        }

        [Route("get-projects"), HttpGet]
        public Task<IEnumerable<RAProjectAccess>> GetProjectAccess()
        {
            return _invoiceService.GetProjectAccess(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }
    }
}

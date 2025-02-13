using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pheonix.Models.VM;
using Pheonix.Models;
using Pheonix.Models.VM.Classes.ResourceAllocation;

namespace Pheonix.Core.Services.Invoice
{
    public interface IInvoiceService
    {
        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId);
        Task<int> SaveOrUpdate(InvoiceViewModel model, int id);
        Task<IEnumerable<InvoiceViewModel>> GetAllList(bool isDraft, int id);
        Task<InvoiceViewModel> GetInvoice(int invoiceId);
        Task<IEnumerable<InvoiceViewModel>> GetApprovals(int id);
        Task<bool> ApproveInvoice(int invoiceId, int id, string invoiceFormCode, Nullable<DateTime> details);
        Task<bool> RejectInvoice(InvoiceViewModel invoiceModel, int id);
        Task<bool> OnHoldInvoice(InvoiceViewModel invoice, int userId);
        Task<IEnumerable<InvoiceViewModel>> ApprovalHistory(int userId);
        Task<bool> DeleteInvoice(int id);
        Task<bool> DeleteInvoiceDetails(int id);
        Task<bool> IsApprover(int userid);
        Task<bool> SavePaymentDetails(InvoicePaymentsViewModel invPayment);
        Task<IEnumerable<InvoicePaymentsViewModel>> GetInvoicePayments(int invoiceId);
        Task<IEnumerable<RAProjectAccess>> GetProjectAccess(int userid);
    }
}

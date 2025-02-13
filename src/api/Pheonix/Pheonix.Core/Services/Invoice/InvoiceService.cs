using System;
using AutoMapper;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pheonix.Models.VM;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM.Classes.ResourceAllocation;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Email;
using System.Data.Entity;

namespace Pheonix.Core.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private IBasicOperationsService _service;
        private IEmailService emailService;
        private readonly PhoenixEntities _dbContext;

        public InvoiceService(IBasicOperationsService opsService, IEmailService emailService)
        {
            this._service = opsService;
            this.emailService = emailService;
            this._dbContext = new PhoenixEntities();
        }

        public Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId)
        {
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();


            //var projNames = _service.All<InvoiceSalesPeriod>().Where(x => x.IsOpen == true).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList();
            var projNames = _service.All<InvoiceSalesPeriod>().OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList();
            lstItems = new List<DropdownItems>();
            foreach (var item in projNames)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = Convert.ToString(item.Month).Trim() + "/" + Convert.ToString(item.Year).Trim(),
                    PrefixText = Convert.ToBoolean(item.IsOpen) ? "true" : "false"
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(InvoiceDropDowns.SalesPeriod.ToString(), lstItems);



            var Project = _service.All<ProjectList>().OrderBy(x => x.ProjectName).ToList();
            lstItems = new List<DropdownItems>();
            foreach (var item in Project)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.ProjectName.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(InvoiceDropDowns.ProjectName.ToString(), lstItems);
            return Task.Run(() => { return Items; });
        }

        public Task<int> SaveOrUpdate(InvoiceViewModel model, int userId)
        {
            return Task.Run(() =>
            {
                int invoiceID = 0;
                bool isInvoiceCreated = false;
                bool isDetailCreated = false;

                foreach (var vmDetail in model.InvoiceDetailsModel)
                {
                    if (vmDetail.SOW_Reference == null)
                    {
                        vmDetail.SOW_Reference = new List<int>();
                        vmDetail.soW_Referencevalue = string.Empty;
                    }
                }

                //ValidateInvoiceDetails(ref model);

                var invoice = Mapper.Map<InvoiceViewModel, PMSInvoice>(model);
                var oldModel = _service.Top<PMSInvoice>(1, x => x.Id == model.Id);
                var oldDetailModel = _service.Top<PMSInvoiceDetails>(1, x => x.InvoiceID == model.Id);
                bool isApprover = QueryHelper.IsInvoiceApprover(userId);
                invoice.RaisedBy = userId;
                invoice.CreatedDate = DateTime.Now;
                invoice.IsHold = false;
                invoice.IsRejected = false;
                if (isApprover)
                {
                    invoice.StageId = 1;
                    invoice.PrimaryApprover = userId;
                    invoice.PrimaryApprovalOn = DateTime.Now;
                }

                if (!oldModel.Any())
                {
                    isInvoiceCreated = _service.Create<PMSInvoice>(invoice, x => x.Id == model.Id);
                    isDetailCreated = isInvoiceCreated;
                }
                else if (oldModel.FirstOrDefault().IsRejected.Value || oldModel.FirstOrDefault().IsApproved)
                {
                    invoice.IsRejected = false;
                    invoice.Comment = null;
                    invoice.ApprovedBy = null;
                    invoice.ApprovedOn = null;
                    invoice.ApprovedBy = null;
                    invoice.PrimaryApprovalOn = null;
                    invoice.PrimaryApprover = null;
                    invoice.InvoiceFormCode = null;
                    invoice.Details = null;
                    invoice.StageId = 0;
                    invoice.IsApproved = false;
                    invoice.Id = 0;
                    foreach (PMSInvoiceDetails item in invoice.PMSInvoiceDetails)
                    {
                        item.AttachedFile = null;
                    }
                    if (isApprover)
                    {
                        invoice.StageId = 1;
                        invoice.PrimaryApprover = userId;
                        invoice.PrimaryApprovalOn = DateTime.Now;
                    }
                    isInvoiceCreated = _service.Create<PMSInvoice>(invoice, x => x.Id == invoice.Id);
                    isDetailCreated = isInvoiceCreated;
                }
                else if (oldModel.Any() && !oldDetailModel.Any() && !invoice.IsDraft.Value)
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        //invoice.RaisedBy = oldModel.First().RaisedBy;
                        //invoice.CreatedDate = oldModel.First().CreatedDate;
                        isInvoiceCreated = _service.Update<PMSInvoice>(invoice, oldModel.First());  //// Update the invoice.
                        _service.Finalize(true);
                        foreach (PMSInvoiceDetails item in invoice.PMSInvoiceDetails) /////// Adding and updating invoice details if any.
                        {
                            var data = entites.PMSInvoice.Where(x => x.Id == invoice.Id).FirstOrDefault();
                            data.PMSInvoiceDetails.Add(item);
                            entites.SaveChanges();
                            isDetailCreated = true;
                        }

                    }
                }
                else
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        isInvoiceCreated = _service.Update<PMSInvoice>(invoice, oldModel.First());  //// Update the invoice.
                        invoice.RaisedBy = oldModel.First().RaisedBy;
                        invoice.CreatedDate = oldModel.First().CreatedDate;
                        foreach (PMSInvoiceDetails item in invoice.PMSInvoiceDetails) /////// Adding and updating invoice details if any.
                        {
                            if (item.Id == 0)  ////    Adding invoice details 
                            {
                                var data = entites.PMSInvoice.Where(x => x.Id == invoice.Id).FirstOrDefault();
                                data.PMSInvoiceDetails.Add(item);
                                entites.SaveChanges();
                            }
                            else ////// Upadting invoice details 
                            {
                                item.PMSInvoice = invoice;
                                var oldDetail = _service.Top<PMSInvoiceDetails>(1, x => x.Id == item.Id).First();
                                isDetailCreated = _service.Update<PMSInvoiceDetails>(item, oldDetail);
                            }
                        }
                    }
                }

                if (isInvoiceCreated && isDetailCreated && !invoice.IsDraft.Value)
                {
                    _service.Finalize(true);
                    this.emailService.SendInvoiceMails(InvoiceMailAction.Submitted, invoice, isApprover, userId);
                    invoiceID = invoice.Id;

                }
                else if (invoice.IsDraft.Value)
                {
                    _service.Finalize(true);
                    invoiceID = invoice.Id;
                }

                return invoiceID;
            });
        }

        public Task<IEnumerable<InvoiceViewModel>> GetAllList(bool isDraft, int id)
        {
            return Task.Run(() =>
            {
                var result = _service.All<PMSInvoice>().Where(x => x.IsDraft == isDraft && x.RaisedBy == id).OrderByDescending(x => x.Id);
                if (result.Any())
                {
                    List<InvoiceViewModel> expenseList = GetInvoiceMappedDetails(result.ToList());
                    return expenseList.AsEnumerable();
                }
                else
                {
                    return null;
                }
            });
        }

        public async Task<InvoiceViewModel> GetInvoice(int invoiceId)
        {
            return await Task.Run(() =>
            {
                var result = _service.First<PMSInvoice>(x => x.Id == invoiceId);
                if (result != null)
                {
                    var invoice = Mapper.Map<PMSInvoice, InvoiceViewModel>(result);
                    var customerAddress = _service.First<CustomerAddress>(x => x.ID == invoice.CustomerAddress);
                    var customerContract = _service.First<CustomerContract>(x => x.ID == invoice.Contract);
                    var customerContact = _service.First<CustomerContactPerson>(x => x.ID == invoice.ContactPerson);
                    if (customerContact != null)
                        invoice.ContactPersonName = customerContact.FirstName + " " + customerContact.LastName;
                    if (customerContract != null)
                        invoice.ContractDet = customerContract.ContractName;
                    if (customerAddress != null)
                        invoice.CustomerAddressDet = customerAddress.Address + ", " + customerAddress.City + ", " + customerAddress.State + ", " + customerAddress.ZipCode;
                    return invoice;
                }
                return null;
            });
        }

        public Task<IEnumerable<InvoiceViewModel>> GetApprovals(int id)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    bool isFinanceApprover = QueryHelper.IsFinanceApprover(id);
                    bool isApprover = QueryHelper.IsInvoiceApprover(id);
                    List<int> project = new List<int>();

                    project = (from p in context.ProjectList
                               join pc in context.PMSConfiguration on p.ID equals pc.Project
                               where pc.PersonID == id
                               select p.ID).ToList();

                    var invoiceIds = new List<Int32>();
                    if (isFinanceApprover)
                    {
                        invoiceIds = context.PMSInvoice
                            .Where(x =>
                                 x.IsDraft == false
                                && x.IsRejected == false
                                && x.IsDeleted == false
                                && x.IsApproved == false
                                && x.StageId == 1
                               )
                          .Select(x => x.Id)
                          .ToList();
                    }
                    else if (isApprover)
                    {
                        invoiceIds = context.PMSInvoice
                            .Where(x =>
                                 x.IsDraft == false
                                && x.IsRejected == false
                                && x.IsDeleted == false
                                && x.IsApproved == false
                                && x.StageId < 1
                                && project.Contains(x.Project.Value) && x.RaisedBy != id
                               )
                          .Select(x => x.Id)
                          .ToList();
                    }

                    List<PMSInvoice> lstInvoice = context.PMSInvoice.Where(t => invoiceIds.Contains(t.Id)).OrderByDescending(p => p.Id).ToList();
                    List<InvoiceViewModel> invoiceList = GetInvoiceMappedDetails(lstInvoice);
                    if (lstInvoice.Any())
                        return invoiceList.AsEnumerable();
                    else
                        return null;
                }
            });
        }

        public Task<bool> ApproveInvoice(int invoiceId, int userId, string invoiceFormCode, Nullable<DateTime> details)
        {
            return Task.Run(() =>
            {
                var invoice = _service.First<PMSInvoice>(x => x.Id == invoiceId);
                var newInvoice = new PMSInvoice();

                bool isFinanceApprover = QueryHelper.IsFinanceApprover(userId);
                newInvoice.Comment = "";
                if (isFinanceApprover)
                {
                    newInvoice.StageId = 2;
                    newInvoice.ApprovedBy = userId;
                    newInvoice.IsApproved = true;
                    newInvoice.ApprovedOn = DateTime.Now;
                    newInvoice.CreatedDate = invoice.CreatedDate;
                    newInvoice.InvoiceFormCode = invoiceFormCode;
                    newInvoice.Details = details;
                }
                else
                {
                    newInvoice.StageId = 1;
                    newInvoice.PrimaryApprover = userId;
                    newInvoice.PrimaryApprovalOn = DateTime.Now;
                    newInvoice.CreatedDate = invoice.CreatedDate;
                }

                bool isInvoiceUpdated = _service.Update<PMSInvoice>(newInvoice, invoice);
                if (isInvoiceUpdated)
                {
                    _service.Finalize(isInvoiceUpdated);
                    var viewModel = AutoMapper.Mapper.Map<PMSInvoice, InvoiceViewModel>(invoice);
                    this.emailService.SendInvoiceMails(InvoiceMailAction.Approved, invoice, isFinanceApprover, userId);
                }
                return true;
            });
        }

        public Task<bool> RejectInvoice(InvoiceViewModel invoiceModel, int userId)
        {
            var oldInvoice = _service.First<PMSInvoice>(x => x.Id == invoiceModel.Id);
            bool isFinanceApprover = QueryHelper.IsFinanceApprover(userId);
            var invoice = Mapper.Map<InvoiceViewModel, PMSInvoice>(invoiceModel);
            oldInvoice.Comment = invoice.Comment;
            if (isFinanceApprover)
            {
                oldInvoice.ApprovedBy = userId;
                oldInvoice.IsRejected = true;
                oldInvoice.ApprovedOn = DateTime.Now;
            }
            else
            {
                oldInvoice.PrimaryApprover = userId;
                oldInvoice.IsRejected = true;
                oldInvoice.PrimaryApprovalOn = DateTime.Now;
            }

            var isRejected = _service.Update<PMSInvoice>(oldInvoice, oldInvoice);

            if (isRejected)
            {
                //emailService.SendExpenseApprovalEmail(oldExpense, person, ApprovalStage.Rejected, invoiceModel.comments);
                _service.Finalize(true);
                var viewModel = AutoMapper.Mapper.Map<PMSInvoice, InvoiceViewModel>(invoice);
                this.emailService.SendInvoiceMails(InvoiceMailAction.Rejected, oldInvoice, isFinanceApprover, userId);
            }
            return Task.Run(() =>
            {
                return isRejected;
            });
        }

        public Task<bool> OnHoldInvoice(InvoiceViewModel invoice, int userId)
        {
            var oldInvoice = _service.First<PMSInvoice>(x => x.Id == invoice.Id);
            var person = _service.First<Person>(x => x.ID == userId);
            bool isFinanceApprover = QueryHelper.IsFinanceApprover(userId);
            oldInvoice.Comment = invoice.Comment;
            if (isFinanceApprover)
            {
                oldInvoice.IsHold = true;
                oldInvoice.ApprovedBy = userId;
                oldInvoice.ApprovedOn = DateTime.Now;
            }
            else
            {
                oldInvoice.PrimaryApprover = userId;
                oldInvoice.IsHold = true;
                oldInvoice.PrimaryApprovalOn = DateTime.Now;
            }

            var IsHold = _service.Update<PMSInvoice>(oldInvoice, oldInvoice);

            if (IsHold)
            {
                //emailService.SendExpenseApprovalEmail(oldExpense, person, ApprovalStage.OnHold, expense.comments);
                _service.Finalize(true);
                var viewModel = AutoMapper.Mapper.Map<PMSInvoice, InvoiceViewModel>(oldInvoice);
                this.emailService.SendInvoiceMails(InvoiceMailAction.OnHold, oldInvoice, isFinanceApprover, userId);
            }

            return Task.Run(() => { return IsHold; });
        }

        public Task<IEnumerable<InvoiceViewModel>> ApprovalHistory(int userId)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    bool isfinanceApprover = QueryHelper.IsFinanceApprover(userId);
                    bool isApprover = QueryHelper.IsInvoiceApprover(userId);
                    //List<int> project = new List<int>();

                    //project = (from p in context.ProjectList
                    //           join pc in context.PMSConfiguration on p.ID equals pc.Project
                    //           where pc.PersonID == userId
                    //           select p.ID).ToList();

                    List<PMSInvoice> lstInvoice = new List<PMSInvoice>();
                    if (isfinanceApprover)
                        lstInvoice = context.PMSInvoice
                            .Where(e => e.StageId > 1 && e.IsApproved == true || (e.IsRejected == true && e.StageId >= 1))
                            .OrderByDescending(x => x.Id).ToList();
                    else if (isApprover)
                        lstInvoice = context.PMSInvoice
                            .Where(e => e.PrimaryApprover == userId && e.RaisedBy != userId && (e.StageId >= 1 || (e.IsRejected == true && e.StageId == 0))).ToList();
                    //.Union(context.PMSInvoice.Where(p => p.StageId > 1 && project.Contains(p.Project.Value))).ToList();


                    List<InvoiceViewModel> invoiceList = GetInvoiceMappedDetails(lstInvoice.ToList());
                    if (lstInvoice.Any())
                        return invoiceList.AsEnumerable();
                    else
                        return null;

                }
            });
        }

        public Task<bool> DeleteInvoice(int id)
        {
            bool isDeleted = false;
            var expense = _service.First<PMSInvoice>(x => x.Id == id);

            isDeleted = DeleteDetails(id, isDeleted);

            isDeleted = _service.Remove<PMSInvoice>(expense, x => x.Id == id);
            if (isDeleted)
                _service.Finalize(true);

            return Task.Run(() => { return isDeleted; });
        }


        public Task<bool> DeleteInvoiceDetails(int id)
        {
            bool isDeleted = false;
            var expense = _service.First<PMSInvoiceDetails>(x => x.Id == id);
            int? invoiceID = expense.InvoiceID;
            decimal? amount = expense.Amount;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                PMSInvoice i = (from x in dbContext.PMSInvoice
                                where x.Id == invoiceID
                                select x).FirstOrDefault();
                i.TotalAmt = i.TotalAmt - amount;
                dbContext.Entry(i).State = EntityState.Modified;

                isDeleted = _service.Remove<PMSInvoiceDetails>(expense, x => x.Id == id);
                if (isDeleted)
                {
                    _service.Finalize(true);
                    dbContext.SaveChanges();
                }
            }

            return Task.Run(() => { return isDeleted; });
        }

        public async Task<bool> IsApprover(int userID)
        {
            return await Task.Run(() =>
            {
                return QueryHelper.IsInvoiceApprover(userID);
            });
        }

        public Task<bool> SavePaymentDetails(InvoicePaymentsViewModel invPayment)
        {
            return Task.Run(() =>
            {
                bool isCreated = false;
                var invoicePayment = Mapper.Map<InvoicePaymentsViewModel, PMSInvoicePayments>(invPayment);
                var oldModel = _service.Top<PMSInvoicePayments>(1, x => x.Id == invPayment.Id);

                if (!oldModel.Any())
                {
                    isCreated = _service.Create<PMSInvoicePayments>(invoicePayment, x => x.Id == invPayment.Id);
                }
                if (isCreated)
                    _service.Finalize(true);
                return isCreated;
            });
        }

        public async Task<IEnumerable<InvoicePaymentsViewModel>> GetInvoicePayments(int invoiceId)
        {
            return await Task.Run(() =>
            {
                var result = _service.All<PMSInvoicePayments>().Where(x => x.InvoiceId == invoiceId);
                if (result.Any())
                {
                    var invoice = Mapper.Map<IEnumerable<PMSInvoicePayments>, IEnumerable<InvoicePaymentsViewModel>>(result);
                    return invoice;
                }
                return null;
            });
        }

        public Task<IEnumerable<RAProjectAccess>> GetProjectAccess(int userid)
        {
            IEnumerable<RAProjectAccess> projectAccess;
            return Task.Run(() =>
            {
                using (var db = _dbContext)
                {
                    try
                    {
                        projectAccess = (from pa in db.getPMSUserActions(userid)
                                         where pa.ActionID.Value == 11
                                         select new RAProjectAccess
                                         {
                                             ActionId = pa.ActionID,
                                             ProjectID = pa.ProjectID
                                         }).ToList();
                        return projectAccess;
                    }
                    catch
                    {
                        return projectAccess = null;
                    }

                }
            });

        }

        #region PrivateMethods

        private List<InvoiceViewModel> GetInvoiceMappedDetails(List<PMSInvoice> invoiceList)
        {
            var invoices = Mapper.Map<IEnumerable<PMSInvoice>, IEnumerable<InvoiceViewModel>>(invoiceList);
            foreach (InvoiceViewModel item in invoices)
            {
                item.invoiceStatus = GetStages(item);
                item.totalStages = item.invoiceStatus.Count();
                var customerAddress = _service.First<CustomerAddress>(x => x.ID == item.CustomerAddress);
                var customerContract = _service.First<CustomerContract>(x => x.ID == item.Contract);
                var customerContact = _service.First<CustomerContactPerson>(x => x.ID == item.ContactPerson);
                if (customerContact != null)
                    item.ContactPersonName = customerContact.FirstName + " " + customerContact.LastName;
                if (customerContract != null)
                    item.ContractDet = customerContract.ContractName;
                if (customerAddress != null)
                    item.CustomerAddressDet = customerAddress.Address + ", " + customerAddress.City + ", " + customerAddress.State + ", " + customerAddress.ZipCode;
                var employee = _service.First<Person>(x => x.ID == item.RaisedBy);
                if (employee != null)
                    item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(employee);
                var financeApprover = _service.First<Person>(x => x.ID == item.ApprovedBy);
                if (financeApprover != null)
                    item.FinanceApprover = financeApprover.FirstName + " " + financeApprover.LastName;
                var primaryApprover = _service.First<Person>(x => x.ID == item.PrimaryApprover);
                if (primaryApprover != null)
                    item.PrimaryApproverName = primaryApprover.FirstName + " " + primaryApprover.LastName;
                var salesPeriod = _service.First<InvoiceSalesPeriod>(x => x.Id == item.SalesPeriod);
                if (salesPeriod != null)
                    item.irNumber = item.Id + "_" + salesPeriod.Year;

                var payment = _service.All<PMSInvoicePayments>().Where(x => x.InvoiceId == item.Id).ToList();

                foreach (var amt in payment)
                {
                    item.totalAmtRecieved = item.totalAmtRecieved + amt.PaymentRecieved.Value;
                }

                item.totalBalAmt = item.TotalAmt.Value - item.totalAmtRecieved;
                foreach (InvoiceDetailsViewModel invDet in item.InvoiceDetailsModel)
                {
                    invDet.soW_Referencevalue = string.Empty;
                    foreach (int sowVal in invDet.SOW_Reference)
                    {
                        var customerSow = _service.First<CustomerContract>(x => x.ID == sowVal);
                        if (customerSow != null)
                        {
                            invDet.soW_Referencevalue = invDet.soW_Referencevalue + ", " + customerSow.ContractName;
                        }
                    }
                    if (!string.IsNullOrEmpty(invDet.soW_Referencevalue))
                    {
                        invDet.soW_Referencevalue = invDet.soW_Referencevalue.TrimStart(',');
                    }
                }

            }
            return invoices.ToList();
        }

        private List<StageStatus> GetStages(InvoiceViewModel invoice)
        {
            List<StageStatus> status = new List<StageStatus>();
            StageStatus primaryApproval;
            StageStatus financeApproval;
            int tillCount = 0;
            bool twoStageApproval = false;

            if (invoice.RaisedBy == invoice.PrimaryApprover)
                twoStageApproval = true;

            if (!twoStageApproval)
            {
                //Primary Approval Stage Status details
                primaryApproval = new StageStatus();
                primaryApproval.Stage = 1;
                primaryApproval.comment = invoice.Comment;
                if (invoice.StageId < 1)
                    primaryApproval.Status = 0;
                else
                    primaryApproval.Status = 1;
                status.Add(primaryApproval);

                // Approval Stage Status details
                financeApproval = new StageStatus();
                financeApproval.Stage = 2;
                financeApproval.comment = invoice.Comment;
                if (invoice.StageId < 2)
                    financeApproval.Status = 0;
                else
                    financeApproval.Status = 1;
                status.Add(financeApproval);
            }
            else
            {
                // Approval Stage Status details
                primaryApproval = new StageStatus();
                primaryApproval.Stage = 1;
                primaryApproval.comment = invoice.Comment;
                if (invoice.StageId < 2)
                    primaryApproval.Status = 0;
                else
                    primaryApproval.Status = 1;
                status.Add(primaryApproval);
            }

            if (invoice.IsRejected == true || invoice.IsHold == true)
            {
                if (twoStageApproval)
                    tillCount = 1;
                else if (invoice.ApprovedBy > 0)
                    tillCount = 2;
                else
                    tillCount = 1;
            }
            else
                tillCount = invoice.StageId;


            if (invoice.IsRejected == true && tillCount == 1)
                status.FirstOrDefault().Status = 2;
            else if (invoice.IsRejected == true && tillCount == 2)
                status.Last().Status = 2;
            else if (invoice.IsHold == true && tillCount == 1)
                status.FirstOrDefault().Status = 3;
            else if (invoice.IsHold == true && tillCount == 2)
                status.Last().Status = 3;

            return status;
        }

        private string GetStatus(int status)
        {
            string result = string.Empty;
            switch (status)
            {
                case 0:
                    result = "Pending";
                    break;
                case 1:
                    result = "Approved";
                    break;
                case 2:
                    result = "Rejected";
                    break;
                case 3:
                    result = "On Hold";
                    break;
            }
            return result;
        }

        private bool DeleteDetails(int id, bool isDeleted)
        {
            var expenseDetails = _service.Top<PMSInvoiceDetails>(1000, x => x.PMSInvoice.Id == id);
            foreach (var item in expenseDetails)
            {
                isDeleted = _service.Remove<PMSInvoiceDetails>(item, x => x.PMSInvoice.Id == id);
            }
            return isDeleted;
        }

        #endregion
    }

    public enum InvoiceDropDowns
    {
        ProjectName,
        SalesPeriod
    }
}

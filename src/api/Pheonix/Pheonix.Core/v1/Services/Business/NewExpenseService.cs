using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public class NewExpenseService : INewExpenseService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;

        public NewExpenseService(IContextRepository repository, IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
        }

        public Task<bool> SaveOrUpdate(Models.VM.NewExpenseViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isExpenseCreated = false;
                bool isDetailCreated = false;

                ValidateExpenseDetails(ref model);

                var expense = Mapper.Map<NewExpenseViewModel, Expense_New>(model);
                var oldModel = service.Top<Expense_New>(1, x => x.ExpenseId == model.expenseId);

                var person = service.Top<Person>(10, x => x.ID == id).ToList().FirstOrDefault();
                expense.Person = person;

                if (!oldModel.Any())
                {
                    isExpenseCreated = service.Create<Expense_New>(expense, x => x.ExpenseId == model.expenseId);
                    isDetailCreated = isExpenseCreated;
                }
                else if (oldModel.FirstOrDefault().IsRejected.Value)
                {
                    expense.IsRejected = false;
                    expense.Comment = null;
                    expense.StageID = 0;
                    expense.PrimaryApprovalOn = null;
                    expense.FinanceApprover = null;
                    expense.ReImbursedOn = null;
                    expense.ExpenseId = 0;

                    isExpenseCreated = service.Create<Expense_New>(expense, x => x.ExpenseId == expense.ExpenseId);

                    if (isExpenseCreated)
                    {
                        var oldModelUpdate = service.Top<Expense_New>(1, x => x.ExpenseId == model.expenseId).FirstOrDefault();
                        var expenseUpdate = Mapper.Map<Expense_New, NewExpenseViewModel>(oldModelUpdate);
                        var expenseFinal = Mapper.Map<NewExpenseViewModel, Expense_New>(expenseUpdate);
                        expenseFinal.IsApproved = true;//For hiding the edit icon of rejected expense with reference to Pivotal Story:#157282070
                        isExpenseCreated = service.Update<Expense_New>(expenseFinal, oldModelUpdate);
                    }

                    isDetailCreated = isExpenseCreated;

                }
                else
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        isExpenseCreated = service.Update<Expense_New>(expense, oldModel.First());  //// Update the expense.

                        foreach (ExpenseDetails_New item in expense.ExpenseDetails_New) /////// Adding and updating expense details if any.
                        {
                            if (item.ExpenseDetailId == 0)  ////    Adding expense details 
                            {
                                var data = entites.Expense_New.Where(x => x.ExpenseId == expense.ExpenseId).FirstOrDefault();
                                data.ExpenseDetails_New.Add(item);
                                entites.SaveChanges();
                            }
                            else ////// Updating expense details 
                            {
                                item.Expense_New = expense;
                                var oldDetail = service.Top<ExpenseDetails_New>(1, x => x.ExpenseDetailId == item.ExpenseDetailId).First();
                                isDetailCreated = service.Update<ExpenseDetails_New>(item, oldDetail);
                            }
                        }
                    }
                }

                if (isExpenseCreated && isDetailCreated && !expense.IsDraft)
                {
                    service.Finalize(true);
                    emailService.SendExpenseApprovalEmail(expense, person, ApprovalStage.Submitted, null);
                }
                else if (expense.IsDraft)
                {
                    service.Finalize(true);
                }

                return isExpenseCreated;
            });
        }

        public Task<bool> DeleteExpense(int id)
        {
            bool isDeleted = false;
            var expense = service.First<Expense_New>(x => x.ExpenseId == id);

            isDeleted = DeleteDetails(id, isDeleted);

            isDeleted = service.Remove<Expense_New>(expense, x => x.ExpenseId == id);
            if (isDeleted)
                service.Finalize(true);

            return Task.Run(() => { return isDeleted; });
        }

        public Task<IEnumerable<NewExpenseViewModel>> GetAllList(bool isDraft, int id)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var result = service.All<Expense_New>().Where(x => x.IsDraft == isDraft && x.Person.ID == id).OrderByDescending(x => x.ExpenseId);
                    if (result.Any())
                    {
                        List<NewExpenseViewModel> expenseList = GetExpenseMappedDetails(result.ToList());
                        return expenseList.AsEnumerable();
                    }
                    else
                    {
                        return null;
                    }
                }
            });
        }

        public async Task<IEnumerable<Models.VM.NewExpenseViewModel>> GetExpense(int expenseId)
        {
            return await Task.Run(() =>
            {
                var result = service.Top<Expense_New>(100, x => x.ExpenseId == expenseId && (x.IsDraft || x.IsRejected.Value));
                if (result.Any())
                    return Mapper.Map<IEnumerable<Expense_New>, IEnumerable<NewExpenseViewModel>>(result);
                return null;
            });
        }

        public Task<bool> DeleteExpenseDetail(int id)
        {
            bool isDeleted = false;
            isDeleted = DeleteDetails(id, isDeleted);

            if (isDeleted)
                service.Finalize(true);

            return Task.Run(() => { return isDeleted; });
        }

        public Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId)
        {
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();

            var currency = service.All<Currency>().OrderBy(x => x.Title);

            lstItems = new List<DropdownItems>();
            foreach (var item in currency)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.CurrencyId,
                    Text = item.Title.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(NewExpenseDropDownType.Currency.ToString(), lstItems);

            var costCenter = service.All<CostCenter>().Where(x => x.IsDeleted == false).OrderBy(x => x.Title);

            lstItems = new List<DropdownItems>();
            foreach (var item in costCenter)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.CostCenterId,
                    Text = item.Title.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(NewExpenseDropDownType.CostCenter.ToString(), lstItems);

            List<GetExpenseApprovers_Result> approvers = null;

            //using (PhoenixEntities context = new PhoenixEntities())
            //{
                approvers = QueryHelper.GetExpenseApprovers(userId);
            //}

            lstItems = new List<DropdownItems>();
            foreach (var item in approvers)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.FullName.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(NewExpenseDropDownType.PrimaryApprover.ToString(), lstItems);

            var secondaryApprover = service.All<SecondaryApprover>();

            lstItems = new List<DropdownItems>();
            foreach (var item in secondaryApprover)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.SecondaryApproverId,
                    Text = item.Title.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(NewExpenseDropDownType.SecondaryApprover.ToString(), lstItems);

            int locationID = (int)service.First<PersonEmployment>(x => x.PersonID == userId).OfficeLocation;
            int location = 0;

            if (locationID == 2)
                location = 1;//Denotes US in ExpenseCategory table: Rahul R
            else
                location = 0;//Denotes India in ExpenseCategory table: : Rahul R
            var expenseCategories = service.All<ExpenseCategory_New>().Where(x => x.LocationID == location && x.IsDeleted == false).OrderBy(x => x.Title);

            lstItems = new List<DropdownItems>();
            foreach (var item in expenseCategories)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ExpenseCategoryId,
                    Text = item.Title.Trim(),
                    PrefixText = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(NewExpenseDropDownType.ExpenseCategory.ToString(), lstItems);

            var projNames = service.All<ProjectList>().OrderBy(x => x.ProjectName).ToList();
            lstItems = new List<DropdownItems>();
            foreach (var item in projNames)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.ProjectName.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(NewExpenseDropDownType.ProjectName.ToString(), lstItems);


            return Task.Run(() => { return Items; });
        }

        public Task<IEnumerable<NewExpenseViewModel>> GetApprovals(int id)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    bool isFinanceApprover = QueryHelper.IsFinanceApprover(id);
                    var expenseIds = new List<Int32>();
                    if (!isFinanceApprover)
                    {
                        expenseIds = context.Expense_New
                          .Where(x => x.StageID == 0
                              && x.IsDraft == false
                              && x.IsRejected == false
                              && x.IsDeleted == false
                              && x.PrimaryApprover == id)
                          .Select(x => x.ExpenseId).ToList();
                    }
                    else
                    {

                        expenseIds = context.Expense_New
                            .Where(x => x.StageID == 1
                                && x.IsDeleted == false
                                && x.IsDraft == false
                                && x.IsRejected == false || (x.StageID == 0 && x.PrimaryApprover == id && x.IsRejected == false && x.IsDeleted == false && x.IsDraft == false))
                            .Select(x => x.ExpenseId).ToList();
                    }

                    List<Expense_New> lstexpense = context.Expense_New.Where(t => expenseIds.Contains(t.ExpenseId)).
                        OrderByDescending(x => x.CreatedDate).ToList();

                    List<NewExpenseViewModel> expenseList = GetExpenseMappedDetails(lstexpense);
                    if (lstexpense.Any())
                        return expenseList.AsEnumerable();
                    else
                        return null;
                }
            });
        }

        public Task<bool> RejectExpense(NewExpenseViewModel expenseModel, int id)
        {

            var oldExpense = service.First<Expense_New>(x => x.ExpenseId == expenseModel.expenseId);
            bool isFinanceApprover = QueryHelper.IsFinanceApprover(id);
            var expense = Mapper.Map<NewExpenseViewModel, Expense_New>(expenseModel);

            if (isFinanceApprover)
            {
                oldExpense.FinanceApprover = id;
                oldExpense.ReImbursedOn = DateTime.Now;
            }
            else
            {
                oldExpense.PrimaryApprovalOn = DateTime.Now;
            }
            oldExpense.IsRejected = true;
            oldExpense.IsHold = false;
            oldExpense.Comment = expenseModel.comments;
            var person = service.First<Person>(x => x.ID == id);
            var isRejected = service.Update<Expense_New>(oldExpense, oldExpense);


            if (isRejected)
            {
                emailService.SendExpenseApprovalEmail(oldExpense, person, ApprovalStage.Rejected, expenseModel.comments);
                service.Finalize(true);
            }
            return Task.Run(() =>
            {
                return isRejected;
            });
        }

        public Task<bool> ApproveExpense(int expenseId, string formCode, string chequeDetails, int id)
        {
            return Task.Run(() =>
            {
                var expense = service.First<Expense_New>(x => x.ExpenseId == expenseId);
                var person = service.First<Person>(x => x.ID == id);
                var newExpense = new Expense_New();
                bool IsDirectApproval = QueryHelper.IsFinanceApprover(Convert.ToInt32(expense.PrimaryApprover));
                expense.Comment = null;
                newExpense.Comment = null;
                switch (expense.StageID)
                {
                    case 0:
                        if (!IsDirectApproval)
                        {
                            newExpense.StageID = 1;
                            newExpense.PrimaryApprovalOn = DateTime.Now;
                            newExpense.IsHold = false;
                        }
                        else
                        {
                            newExpense.StageID = 2;
                            newExpense.IsApproved = true;
                            newExpense.FormCode = formCode;
                            newExpense.ChequeDetails = chequeDetails;
                            newExpense.PrimaryApprovalOn = DateTime.Now;
                            newExpense.FinanceApprover = id;
                            newExpense.ReImbursedOn = DateTime.Now;
                            newExpense.IsHold = false;
                        }
                        break;
                    case 1:
                        newExpense.StageID = 2;
                        newExpense.IsApproved = true;
                        newExpense.FormCode = formCode;
                        newExpense.ChequeDetails = chequeDetails;
                        newExpense.ReImbursedOn = DateTime.Now;
                        newExpense.FinanceApprover = id;
                        newExpense.IsHold = false;
                        break;
                    default:
                        break;
                }

                bool isExpenseUpdated = service.Update<Expense_New>(newExpense, expense);
                if (isExpenseUpdated)
                {
                    service.Finalize(isExpenseUpdated);
                    emailService.SendExpenseApprovalEmail(expense, person, (ApprovalStage)expense.StageID, "Approved");
                }


                return true;
            });
        }

        public Task<bool> OnHoldExpense(NewExpenseViewModel expense, int userId)
        {
            var oldExpense = service.First<Expense_New>(x => x.ExpenseId == expense.expenseId);
            var person = service.First<Person>(x => x.ID == userId);
            bool isFinanceApprover = QueryHelper.IsFinanceApprover(userId);
            oldExpense.IsHold = true;
            oldExpense.Comment = expense.comments;
            if (oldExpense.StageID == 1)
            {
                oldExpense.FinanceApprover = userId;
                oldExpense.ReImbursedOn = DateTime.Now;
            }
            else
            {
                oldExpense.FinanceApprover = null;
                oldExpense.PrimaryApprovalOn = DateTime.Now;
            }
            var IsHold = service.Update<Expense_New>(oldExpense, oldExpense);

            if (IsHold)
            {
                emailService.SendExpenseApprovalEmail(oldExpense, person, ApprovalStage.OnHold, expense.comments);
                service.Finalize(true);
            }

            return Task.Run(() => { return IsHold; });
        }

        public Task<IEnumerable<NewExpenseViewModel>> ApprovalHistory(int userId)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    bool isfinanceApprover = QueryHelper.IsFinanceApprover(userId);
                    List<Expense_New> lstexpense = new List<Expense_New>();
                    if (isfinanceApprover)
                        lstexpense = context.Expense_New.Where(e => e.StageID > 1 && e.IsApproved == true || (e.IsRejected == true && e.StageID >= 1) || (e.IsRejected == true && e.StageID == 0 && e.PrimaryApprover == userId)).OrderByDescending(x => x.ExpenseId).ToList();
                    else
                        lstexpense = context.Expense_New.Where(e => e.PrimaryApprover == userId && e.StageID >= 1 || (e.IsRejected == true && e.StageID == 0 && e.PrimaryApprover == userId)).OrderByDescending(x => x.ExpenseId).ToList();

                    List<NewExpenseViewModel> expenseList = GetExpenseMappedDetails(lstexpense.ToList());
                    if (lstexpense.Any())
                        return expenseList.AsEnumerable();
                    else
                        return null;

                }
            });
        }

        public IEnumerable<NewExpenseViewModel> DashBoardExpenseCardView(int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                List<NewExpenseViewModel> viewModel = new List<NewExpenseViewModel>();
                var expenseList = context.Expense_New.Where(x => x.PersonID == userId && x.IsDraft == false).ToList();
                if (expenseList.Count > 2)
                {
                    int skipCount = expenseList.Count - 2;
                    expenseList = expenseList.Skip(skipCount).Take(2).ToList();
                }
                foreach (var item in expenseList)
                {
                    var expense = context.Expense_New.Where(x => x.ExpenseId == item.ExpenseId).FirstOrDefault();
                    var mappedExpense = Mapper.Map<Expense_New, NewExpenseViewModel>(expense);
                    mappedExpense.expenseStatus = GetStages(mappedExpense);
                    viewModel.Add(mappedExpense);
                }

                return viewModel;
            }
        }

        #region Private Methods

        private List<NewExpenseViewModel> GetExpenseMappedDetails(List<Expense_New> expenseList)
        {
            var expenses = Mapper.Map<IEnumerable<Expense_New>, IEnumerable<NewExpenseViewModel>>(expenseList);
            using (PhoenixEntities context = new PhoenixEntities())
            {
                foreach (NewExpenseViewModel item in expenses)
                {
                    item.expenseStatus = GetStages(item);
                    item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(expenseList.Where(x => x.ExpenseId == item.expenseId).First().Person);
                    item.totalStages = item.expenseStatus.Count();

                    if (item.PrimaryApproverId > 0)
                    {
                        var primaryApprover = service.First<Person>(x => x.ID == item.PrimaryApproverId);
                        item.primaryApproverName = primaryApprover.FirstName + ' ' + primaryApprover.LastName;
                        item.expenseApprovedDate = item.expenseApprovedDate.HasValue ? Convert.ToDateTime(item.expenseApprovedDate) : (DateTime?)null;
                    }
                    if (item.financeApproverId > 0)
                    {
                        var financeApprover = service.First<Person>(x => x.ID == item.financeApproverId);
                        item.financeApproverName = financeApprover.FirstName + ' ' + financeApprover.LastName;
                        item.financeApprovedDate = item.financeApprovedDate.HasValue ? Convert.ToDateTime(item.financeApprovedDate) : (DateTime?)null;
                    }

                    if (item.CurrencyId > 0)
                    {
                        var currency = context.Currency.Where(x => x.CurrencyId == item.CurrencyId).FirstOrDefault();
                        item.currency = currency.Title;
                    }
                    else
                    {
                        item.currency = "";
                    }

                    if (item.expenseStatus != null)
                    {
                        if (item.expenseStatus.Count() == 1)
                        {
                            item.status = GetStatus(item.expenseStatus.FirstOrDefault().Status);
                        }
                        else
                        {
                            if (item.expenseStatus.Last().Status == 0)
                            {
                                var data = item.expenseStatus.AsEnumerable().Reverse().Skip(1).FirstOrDefault();
                                item.status = GetStatus(data.Status);
                            }
                            else
                            {
                                item.status = GetStatus(item.expenseStatus.Last().Status);
                            }
                        }
                    }

                }
                return expenses.ToList();
            }

        }

        private List<StageStatus> GetStages(NewExpenseViewModel expense)
        {
            List<StageStatus> status = new List<StageStatus>();
            StageStatus primaryApproval;
            StageStatus financeApproval;
            int tillCount = 0;
            bool isDirectApproval = QueryHelper.IsFinanceApprover(expense.PrimaryApproverId);
            if (!isDirectApproval)
            {
                //Primary Approval Stage Status details
                primaryApproval = new StageStatus();
                primaryApproval.Stage = 1;
                primaryApproval.comment = expense.comments;
                if (expense.stageId < 1)
                    primaryApproval.Status = 0;
                else
                    primaryApproval.Status = 1;
                status.Add(primaryApproval);

                // Approval Stage Status details
                financeApproval = new StageStatus();
                financeApproval.Stage = 2;
                financeApproval.comment = expense.comments;
                if (expense.stageId < 2)
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
                primaryApproval.comment = expense.comments;
                if (expense.stageId < 1)
                    primaryApproval.Status = 0;
                else
                    primaryApproval.Status = 1;
                status.Add(primaryApproval);
            }


            if (expense.IsRejected == true || expense.IsHold == true)
            {
                if (isDirectApproval)
                    tillCount = 1;
                else if (expense.financeApproverId > 0)
                    tillCount = 2;
                else
                    tillCount = 1;
            }
            else
                tillCount = expense.stageId;

            if (expense.IsRejected == true && tillCount == 1)
                status.FirstOrDefault().Status = 2;
            else if (expense.IsRejected == true && tillCount == 2)
                status.Last().Status = 2;
            else if (expense.IsHold == true && tillCount == 1)
                status.FirstOrDefault().Status = 3;
            else if (expense.IsHold == true && tillCount == 2)
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
            var expenseDetails = service.Top<ExpenseDetails_New>(1000, x => x.Expense_New.ExpenseId == id);
            foreach (var item in expenseDetails)
            {
                isDeleted = service.Remove<ExpenseDetails_New>(item, x => x.Expense_New.ExpenseId == id);
            }
            return isDeleted;
        }

        private void ValidateExpenseDetails(ref NewExpenseViewModel model)
        {
            List<int> toDelete = new List<int>();
            foreach (NewEmployeeExpenseDetails item in model.details.ToList().Where(x => x.isDetailsValid == false))
            {
                if (!item.isDetailsValid)
                {
                    var expensedetail = service.First<ExpenseDetails_New>(x => x.ExpenseDetailId == item.ExpenseDetailId);

                    if (expensedetail != null)
                    {
                        bool isDeleted = service.Remove<ExpenseDetails_New>(expensedetail, x => x.ExpenseDetailId == item.ExpenseDetailId);
                        service.Finalize(true);
                    }
                }
            }

            model.details.RemoveAll(x => x.isDetailsValid == false);

        }

        #endregion

        public Task<bool> SaveOrUpdate(ExpenseCategoryViewModel model)
        {
            return Task.Run(() =>
            {
                bool isExpenseCategCreated = false;
                var mappedExpenseModel = Mapper.Map<ExpenseCategoryViewModel, ExpenseCategory_New>(model);
                if (model.ExpenseCategoryId == 0)
                {
                    service.Create<ExpenseCategory_New>(mappedExpenseModel, x => x.ExpenseCategoryId == model.ExpenseCategoryId);
                    service.Finalize(true);
                    isExpenseCategCreated = true;
                }
                else
                {
                    int expenseCount = service.All<ExpenseDetails_New>().Where(x => x.ExpenseCategoryId == model.ExpenseCategoryId).Count();
                    if (expenseCount <= 0)
                    {
                        service.Update<ExpenseCategory_New>(mappedExpenseModel);
                        service.Finalize(true);
                        isExpenseCategCreated = true;
                    }
                }
                return isExpenseCategCreated;
            });
        }

        public Task<IEnumerable<ExpenseCategory_New>> GetExpCategoryList()
        {
            return Task.Run(() =>
            {
                return service.All<ExpenseCategory_New>();
            });
        }

        public Task<bool> DeleteExpenseCategory(int id)
        {
            bool isDeleted = false;
            int expenseCount = service.All<ExpenseDetails_New>().Where(x => x.ExpenseCategoryId == id).Count();
            if (expenseCount <= 0)
            {
                var expenseCategory = service.First<ExpenseCategory_New>(x => x.ExpenseCategoryId == id);
                isDeleted = service.SoftRemove<ExpenseCategory_New>(expenseCategory, x => x.ExpenseCategoryId == id);
            }
            else
                isDeleted = false;
            if (isDeleted)
                service.Finalize(true);
            return Task.Run(() => { return isDeleted; });
        }

        public async Task<bool> IsApprover(int userID)
        {
            return await Task.Run(() =>
            {
                var result = false;
                var Expense = service.Top<Pheonix.DBContext.Expense_New>(0, x => x.PrimaryApprover == userID).ToList();
                if (Expense.Count > 0)
                {
                    result = true;
                }
                return result;
            });
        }

        public Task<bool> SendExpenseReminder(ExpenseMail expReminder)
        {
            return Task.Run(() => { return emailService.SendExpenseReminder(expReminder); });
        }
    }
}

public enum NewExpenseDropDownType
{
    Currency,
    CostCenter,
    PrimaryApprover,
    SecondaryApprover,
    ExpenseCategory,
    ProjectName
}

using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Approval;
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
    public class ExpenseService : IExpenseService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;
        private IApprovalService approvalService;
        const int expenseRequestType = 3;


        public ExpenseService(IContextRepository repository, IBasicOperationsService opsService, IEmailService opsEmailService, IApprovalService opsApprovalService)
        {
            service = opsService;
            emailService = opsEmailService;
            approvalService = opsApprovalService;
        }

        public Task<bool> SaveOrUpdate(Models.VM.ExpenseViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isExpenseCreated = false;
                bool isDetailCreated = false;

                ValidateExpenseDetails(ref model);

                var expense = Mapper.Map<ExpenseViewModel, Expense>(model);
                var oldModel = service.Top<Expense>(1, x => x.ExpenseId == model.expenseId);
                var person = service.Top<Person>(10, x => x.ID == id).ToList().FirstOrDefault();
                expense.Person = person;

                if (!oldModel.Any())
                {
                    isExpenseCreated = service.Create<Expense>(expense, x => x.ExpenseId == model.expenseId);
                    isDetailCreated = isExpenseCreated;
                }
                else
                {
                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        DeleteRejectedExpense(oldModel, entites);   ////   This will run only in case of if a rejected expense is submitted again.

                        isExpenseCreated = service.Update<Expense>(expense, oldModel.First());  //// Update the expense.

                        foreach (ExpenseDetails item in expense.ExpenseDetails) /////// Adding and upadting expense details if any.
                        {
                            if (item.ExpenseDetailId == 0)  ////    Adding expense details 
                            {
                                var data = entites.Expense.Where(x => x.ExpenseId == expense.ExpenseId).FirstOrDefault();
                                data.ExpenseDetails.Add(item);
                                entites.SaveChanges();
                            }
                            else ////// Upadting expense details 
                            {
                                item.Expense = expense;
                                var oldDetail = service.Top<ExpenseDetails>(1, x => x.ExpenseDetailId == item.ExpenseDetailId).First();
                                isDetailCreated = service.Update<ExpenseDetails>(item, oldDetail);
                            }
                        }
                    }
                }

                if (isExpenseCreated && isDetailCreated && !expense.IsDraft)    //// Hook approval only in case its not a draft
                {
                    service.Finalize(true);
                    HookApproval(id, expense.ExpenseId, expense.PrimaryApprover.Value);
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
            var expense = service.First<Expense>(x => x.ExpenseId == id);

            isDeleted = DeleteDetails(id, isDeleted);

            isDeleted = service.Remove<Expense>(expense, x => x.ExpenseId == id);
            if (isDeleted)
                service.Finalize(true);

            return Task.Run(() => { return isDeleted; });
        }

        public Task<IEnumerable<ExpenseViewModel>> GetAllList(bool isDraft, int id)
        {
            return Task.Run(() =>
            {
                List<GetMySubmittedExpenses_Result> totalExpenses = null;
                int totalStages = 0;

                using (PhoenixEntities context = new PhoenixEntities())
                {
                    totalExpenses = QueryHelper.GetMySubmittedExpenses(id);
                    //totalStages = QueryHelper.GetTotalStages(context, 0, id);
                    //}

                    var result = service.All<Expense>().Where(x => x.IsDraft == isDraft && x.Person.ID == id).OrderByDescending(x => x.CreatedDate);
                    //totalStages = totalExpenses.GroupBy(x => x.ApprovalID.Value).Count();

                    if (result.Any())
                    {
                        var expenses = Mapper.Map<IEnumerable<Expense>, IEnumerable<ExpenseViewModel>>(result);

                        foreach (ExpenseViewModel item in expenses)
                        {
                            if (!isDraft)
                            totalStages = totalExpenses.Where(x => x.RequestID == item.expenseId).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                            var stages = totalExpenses.Where(x => x.RequestID == item.expenseId);

                            if (item.PrimaryApproverId > 0)
                            {
                                var primaryApprover = service.First<Person>(x => x.ID == item.PrimaryApproverId);
                                item.primaryApproverName = primaryApprover.FirstName + ' ' + primaryApprover.LastName;
                            }

                            if (stages.Any())
                            {
                                item.expenseStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                            }
                            item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(result.Where(x => x.ExpenseId == item.expenseId).First().Person);
                            item.totalStages = totalStages;

                            // For :#150915531 on 09/08/2017 To get Currency and Status
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
                        return expenses;
                    }
                    else
                    {
                        return null;
                    }
                }
            });
            //return Task.Run(() =>
            //{
            //    using (PhoenixEntities context = new PhoenixEntities())
            //    {
            //        int totalStages = 0;

            //        var expenseIds = QueryHelper.GetApprovalsForUser1(context, id, 3);
            //        List<GetExpenseToApprove_Result> approvals = QueryHelper.GetApprovalsToApprove(context, id);
            //        //var expenseIds = approvals.Select(x => x.RequestID).Distinct(); /// To change after validation.
            //        List<Expense> lstexpense = context.Expense.Where(t => expenseIds.Contains(t.ExpenseId)).OrderByDescending(x => x.CreatedDate).ToList();
            //        var mappedExpense = Mapper.Map<List<Expense>, IEnumerable<ExpenseViewModel>>(lstexpense);

            //        foreach (ExpenseViewModel item in mappedExpense)
            //        {
            //            //totalStages = QueryHelper.GetTotalStages(context, item.expenseId, null);
            //            totalStages = approvals.Where(x => x.RequestID == item.expenseId).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
            //            var stages = approvals.Where(x => x.RequestID == item.expenseId);

            //            if (stages.Any())
            //            {
            //                item.expenseStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
            //            }

            //            item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(lstexpense.Where(x => x.ExpenseId == item.expenseId).First().Person);
            //            item.totalStages = totalStages;

            //            var primaryemployee = context.People.Where(x => x.ID == item.PrimaryApproverId).FirstOrDefault();
            //            item.primaryApproverName = primaryemployee.FirstName + ' ' + primaryemployee.LastName;

            //            //For :#150915531 on 09/08/2017 To get Currency and Status
            //            var currency = context.Currency.Where(x => x.CurrencyId == item.CurrencyId).FirstOrDefault();
            //            item.currency = currency.Title;

            //            if (item.expenseStatus.Count() == 1)
            //            {
            //                item.status = GetStatus(item.expenseStatus.FirstOrDefault().Status);
            //            }
            //            else
            //            {
            //                if (item.expenseStatus.Last().Status == 0)
            //                {
            //                    var data = item.expenseStatus.AsEnumerable().Reverse().Skip(1).FirstOrDefault();
            //                    item.status = GetStatus(data.Status);
            //                }
            //                else
            //                {
            //                    item.status = GetStatus(item.expenseStatus.Last().Status);
            //                }
            //            }
            //        }

            //        if (lstexpense.Any())
            //            return mappedExpense;
            //        else
            //            return null;
            //    }
            //});
        }

        public async Task<IEnumerable<Models.VM.ExpenseViewModel>> GetExpense(int expenseId)
        {
            return await Task.Run(() =>
            {
                var result = service.Top<Expense>(100, x => x.ExpenseId == expenseId && (x.IsDraft || x.IsRejected.Value));
                if (result.Any())
                    return Mapper.Map<IEnumerable<Expense>, IEnumerable<ExpenseViewModel>>(result);
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

            /*Change done to fetch list from Customer table instead of ClientName table*/
            //var clientNames = service.All<ClientName>(); 
            var clientNames = service.All<Customer>().OrderBy(x => x.Name);

            foreach (var item in clientNames)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    //ID = item.ClientNameId,
                    ID = item.ID,
                    Text = item.Name.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(ExpenseDropDownType.ClientName.ToString(), lstItems);

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
            Items.Add(ExpenseDropDownType.Currency.ToString(), lstItems);

            var costCenter = service.All<CostCenter>().Where(x => x.IsDeleted == true).OrderBy(x => x.Title);

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
            Items.Add(ExpenseDropDownType.CostCenter.ToString(), lstItems);

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
            Items.Add(ExpenseDropDownType.PrimaryApprover.ToString(), lstItems);

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
            Items.Add(ExpenseDropDownType.SecondaryApprover.ToString(), lstItems);

            var expenseCategories = service.All<ExpenseCategory>().OrderBy(x => x.Title);

            lstItems = new List<DropdownItems>();
            foreach (var item in expenseCategories)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ExpenseCategoryId,
                    Text = item.Title.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(ExpenseDropDownType.ExpenseCategory.ToString(), lstItems);






            return Task.Run(() => { return Items; });
        }

        public Task<IEnumerable<ExpenseViewModel>> GetApprovals(int id)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    int totalStages = 0;

                    var expenseIds = QueryHelper.GetApprovalsForUser1(id, 3);
                    List<GetExpenseToApprove_Result> approvals = QueryHelper.GetApprovalsToApprove( id);
                    //var expenseIds = approvals.Select(x => x.RequestID).Distinct(); /// To change after validation.
                    List<Expense> lstexpense = context.Expense.Where(t => expenseIds.Contains(t.ExpenseId)).OrderByDescending(x => x.CreatedDate).ToList();
                    var mappedExpense = Mapper.Map<List<Expense>, IEnumerable<ExpenseViewModel>>(lstexpense);

                    foreach (ExpenseViewModel item in mappedExpense)
                    {
                        //totalStages = QueryHelper.GetTotalStages(context, item.expenseId, null);
                        totalStages = approvals.Where(x => x.RequestID == item.expenseId).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                        var stages = approvals.Where(x => x.RequestID == item.expenseId);

                        if (stages.Any())
                        {
                            item.expenseStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                        }

                        item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(lstexpense.Where(x => x.ExpenseId == item.expenseId).First().Person);
                        item.totalStages = totalStages;

                        var primaryemployee = context.People.Where(x => x.ID == item.PrimaryApproverId).FirstOrDefault();
                        item.primaryApproverName = primaryemployee.FirstName + ' ' + primaryemployee.LastName;

                        if (item.CurrencyId > 0)
                        {
                            var currency = context.Currency.Where(x => x.CurrencyId == item.CurrencyId).FirstOrDefault();
                            item.currency = currency.Title;
                        }
                        else
                        {
                            item.currency = "";
                        }


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

                    if (lstexpense.Any())
                        return mappedExpense;
                    else
                        return null;
                }
            });
        }

        public string GetStatus(int status)
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

        public Task<bool> RejectExpense(ExpenseViewModel expenseModel, int id)
        {

            var oldExpense = service.First<Expense>(x => x.ExpenseId == expenseModel.expenseId);

            var expense = Mapper.Map<ExpenseViewModel, Expense>(expenseModel);
            //expense.Person = oldExpense.Person;
            //expense.StageID = 5;
            oldExpense.IsRejected = true;
            var person = service.First<Person>(x => x.ID == id);
            var isRejected = service.Update<Expense>(oldExpense, oldExpense);


            if (isRejected)
            {
                UpdateExpenseApproval(oldExpense.Person.ID, oldExpense.ExpenseId, 2, expenseModel.comments, id);
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
                var expense = service.First<Expense>(x => x.ExpenseId == expenseId);
                var person = service.First<Person>(x => x.ID == id);
                var approvalDetails = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == expenseId);
                int stageID = (int)approvalDetails.Where(x => x.ApproverID == id).Select(t => t.Stage).First();
                int approvalUpdatedID = UpdateExpenseApproval(expense.Person.ID, expense.ExpenseId, 1, "Approved", id);
                if (approvalDetails.Count() == 1)
                {
                    stageID = stageID + 1;
                }
                var newExpense = new Expense();
                if (approvalUpdatedID != -999)
                {
                    switch (stageID)
                    {
                        case 1:// ND: Update the StageId in Expense table for Primary Approver
                            newExpense.ExpenseId = expense.ExpenseId;
                            newExpense.StageID = 2;
                            newExpense.FormCode = formCode;
                            newExpense.ChequeDetails = chequeDetails;
                            break;

                        case 2:// ND: Update the StageId in Expense table for Secondary Approver
                            newExpense.ExpenseId = expense.ExpenseId;
                            newExpense.StageID = 3;
                            newExpense.IsApproved = true;
                            newExpense.FormCode = formCode;
                            newExpense.ChequeDetails = chequeDetails;
                            break;

                        //case 3:// ND: Update the StageId in Expense table for Finance
                        //    newExpense.ExpenseId = expense.ExpenseId;
                        //    newExpense.StageID = 4;
                        //    newExpense.IsApproved = true;
                        //    newExpense.FormCode = formCode;
                        //    newExpense.ChequeDetails = chequeDetails;
                        //    break;

                        default:
                            break;
                    }

                    bool isExpenseUpdated = service.Update<Expense>(newExpense, expense);
                    if (isExpenseUpdated)
                    {
                        service.Finalize(isExpenseUpdated);
                        emailService.SendExpenseApprovalEmail(expense, person, (ApprovalStage)stageID, "Approved");
                    }
                }

                return true;
            });
        }

        public Task<bool> OnHoldExpense(ExpenseViewModel expense, int userId)
        {
            var oldExpense = service.First<Expense>(x => x.ExpenseId == expense.expenseId);
            var person = service.First<Person>(x => x.ID == userId);

            UpdateExpenseApproval(oldExpense.Person.ID, oldExpense.ExpenseId, 3, expense.comments, userId);
            emailService.SendExpenseApprovalEmail(oldExpense, person, ApprovalStage.OnHold, expense.comments);
            service.Finalize(true);

            return Task.Run(() => { return true; });
        }

        public Task<IEnumerable<ExpenseViewModel>> ApprovalHistory(int userId)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {

                    List<GetApprovedExpenses_Result> approvals = QueryHelper.GetApprovedExpenses(userId);
                    var requestIds = approvals.Where(x => x.ApproverID.Value == userId && (x.Status == 1 || x.Status == 2)).Select(x => x.RequestID).Distinct();
                    List<Expense> lstexpense = context.Expense.Where(e => requestIds.Contains(e.ExpenseId)).OrderByDescending(x => x.ExpenseId).ToList();

                    return MapExpense(approvals, lstexpense);
                }
            });
        }

        public IEnumerable<ExpenseViewModel> DashBoardExpenseCardView(int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                int totalStages = 0;
                List<ExpenseViewModel> viewModel = new List<ExpenseViewModel>();
                List<GetLatestActivity_Result> expenseActivity = QueryHelper.GetLatestActivity(userId, expenseRequestType);

                var requestIds = expenseActivity.Select(x => x.RequestID).Distinct().Take(2);

                foreach (var item in requestIds)
                {
                    var expense = context.Expense.Where(x => x.ExpenseId == item.Value).FirstOrDefault();
                    var mappedExpense = Mapper.Map<Expense, ExpenseViewModel>(expense);
                    var stages = expenseActivity.Where(x => x.RequestID == item.Value);

                    if (stages.Any())
                    {
                        mappedExpense.expenseStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                    }

                    mappedExpense.totalStages = totalStages;

                    viewModel.Add(mappedExpense);
                }
                return viewModel;
            }
        }

        #region Private Methods

        private bool DeleteDetails(int id, bool isDeleted)
        {
            var expenseDetails = service.Top<ExpenseDetails>(1000, x => x.Expense.ExpenseId == id);
            foreach (var item in expenseDetails)
            {
                isDeleted = service.Remove<ExpenseDetails>(item, x => x.Expense.ExpenseId == id);
            }
            return isDeleted;
        }

        private int HookApproval(int userId, int recordID, int primaryApproverId)
        {
            // ND: If there is no Secondary Approver we need to skip the Secondary and make Finance as Final Approver.
            // ND: Code not done for the above thing.
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.MultiLevel, userId, primaryApproverId, Convert.ToInt32(ConfigurationManager.AppSettings["FinanceRoleId"].ToString()));
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            service.SendForApproval(userId, 3, recordID, strategy.FetchApprovers()); // ND: Don't know what will be the RequestType for Expense, so for now keeping it as 3.
            return strategy.FetchApprovers().First();
        }

        private int UpdateExpenseApproval(int userID, int recordID, int statusID, string comments, int approverID)
        {
            ApprovalService service = new ApprovalService(this.service);
            return service.UpdateMultiLevelApproval(userID, 3, recordID, statusID, comments, approverID);
        }

        private void ValidateExpenseDetails(ref ExpenseViewModel model)
        {
            List<int> toDelete = new List<int>();
            foreach (EmployeeExpenseDetails item in model.details.ToList().Where(x => x.isDetailsValid == false))
            {
                if (!item.isDetailsValid)
                {
                    var expensedetail = service.First<ExpenseDetails>(x => x.ExpenseDetailId == item.ExpenseDetailId);

                    if (expensedetail != null)
                    {
                        bool isDeleted = service.Remove<ExpenseDetails>(expensedetail, x => x.ExpenseDetailId == item.ExpenseDetailId);
                        service.Finalize(true);
                    }
                }
            }

            model.details.RemoveAll(x => x.isDetailsValid == false);

        }

        private IEnumerable<DropdownItems> method<T>() where T : class
        {
            IEnumerable<T> returnData = service.All<T>();
            List<DropdownItems> lstItems = new List<DropdownItems>();

            foreach (var item in returnData)
            {
                DropdownItems dropdownItem = new DropdownItems
                {

                };
            }

            return lstItems;
        }

        private IEnumerable<ExpenseViewModel> MapExpense(List<GetApprovedExpenses_Result> approvals, List<Expense> lstexpense)
        {
            int totalStages = 0;
            var mappedExpense = Mapper.Map<List<Expense>, IEnumerable<ExpenseViewModel>>(lstexpense);

            foreach (ExpenseViewModel item in mappedExpense)
            {
                //totalStages = QueryHelper.GetTotalStages(context, item.expenseId, null);
                totalStages = approvals.Where(x => x.RequestID == item.expenseId).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                var stages = approvals.Where(x => x.RequestID == item.expenseId);

                if (item.PrimaryApproverId > 0)
                {
                    var primaryApprover = service.First<Person>(x => x.ID == item.PrimaryApproverId);
                    item.primaryApproverName = primaryApprover.FirstName + ' ' + primaryApprover.LastName;
                }
                if (stages.Any())
                {
                    item.expenseStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                }

                item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(lstexpense.Where(x => x.ExpenseId == item.expenseId).First().Person);
                item.totalStages = totalStages;

                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var primaryemployee = context.People.Where(x => x.ID == item.PrimaryApproverId).FirstOrDefault();
                    item.primaryApproverName = primaryemployee.FirstName + ' ' + primaryemployee.LastName;

                    if (item.CurrencyId > 0)
                    {
                        var currency = context.Currency.Where(x => x.CurrencyId == item.CurrencyId).FirstOrDefault();
                        item.currency = currency.Title;
                    }
                    else
                    {
                        item.currency = "";
                    }

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

            if (lstexpense.Any())
                return mappedExpense;
            else
                return null;
        }

        private void DeleteRejectedExpense(IEnumerable<Expense> oldModel, PhoenixEntities entites)
        {
            if (oldModel.FirstOrDefault().IsRejected.Value)
            {
                entites.DeleteApproval(oldModel.FirstOrDefault().ExpenseId);
                entites.SaveChanges();
            }
        }

        #endregion

    }
}

public enum ExpenseDropDownType
{
    ClientName,
    Currency,
    CostCenter,
    PrimaryApprover,
    SecondaryApprover,
    ExpenseCategory
}
public class ExpenseDashboardStatusCount
{
    public int Status { get; set; }
    public int Count { get; set; }
    public string Url { get; set; }
}
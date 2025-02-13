using Pheonix.DBContext;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface INewExpenseService
    {
        Task<bool> SaveOrUpdate(Models.VM.NewExpenseViewModel model, int id);
        Task<bool> DeleteExpense(int id);
        Task<IEnumerable<NewExpenseViewModel>> GetAllList(bool isDraft, int id);
        Task<IEnumerable<Models.VM.NewExpenseViewModel>> GetExpense(int expenseId);
        Task<bool> DeleteExpenseDetail(int id);
        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId);
        Task<IEnumerable<NewExpenseViewModel>> GetApprovals(int id);
        Task<bool> RejectExpense(NewExpenseViewModel expenseModel, int id);
        Task<bool> ApproveExpense(int expenseId, string formCode, string chequeDetails, int id);
        Task<bool> OnHoldExpense(NewExpenseViewModel expense, int userId);
        Task<IEnumerable<NewExpenseViewModel>> ApprovalHistory(int userId);
        IEnumerable<NewExpenseViewModel> DashBoardExpenseCardView(int userId);
        Task<bool> SaveOrUpdate(ExpenseCategoryViewModel model);
        Task<IEnumerable<ExpenseCategory_New>> GetExpCategoryList();
        Task<bool> DeleteExpenseCategory(int id);
        Task<bool> IsApprover(int userid);
        Task<bool> SendExpenseReminder(ExpenseMail expReminder);
    }
}

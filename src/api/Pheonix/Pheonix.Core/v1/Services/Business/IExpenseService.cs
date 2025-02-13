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
    public interface IExpenseService
    {
        Task<bool> SaveOrUpdate(Models.VM.ExpenseViewModel model, int id);
        Task<bool> DeleteExpense(int id);

        Task<IEnumerable<ExpenseViewModel>> GetAllList(bool isDraft, int id);

        Task<IEnumerable<Models.VM.ExpenseViewModel>> GetExpense(int expenseId);

        Task<bool> DeleteExpenseDetail(int id);
        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId);
        Task<IEnumerable<ExpenseViewModel>> GetApprovals(int id);
        Task<bool> RejectExpense(ExpenseViewModel expenseModel, int id);
        Task<bool> ApproveExpense(int expenseId, string formCode, string chequeDetails, int id);
        Task<bool> OnHoldExpense(ExpenseViewModel expense, int userId);
        Task<IEnumerable<ExpenseViewModel>> ApprovalHistory(int userId);
        IEnumerable<ExpenseViewModel> DashBoardExpenseCardView(int userId);
    }
}

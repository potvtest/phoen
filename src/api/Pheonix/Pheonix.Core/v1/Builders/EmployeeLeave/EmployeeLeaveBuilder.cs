using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Builders
{
    public abstract class EmployeeLeaveBuilder<T>
    {
        public EmployeeLeaveBuilder()
        {
        }

        protected virtual IEnumerable<T> BuildViewModel(int userID, DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        protected virtual AvailableLeaves BuildAvailableLeaves(int userID, int year)
        {
            return new AvailableLeaves();
        }

        protected virtual IEnumerable<EmployeeAdminHistoryData> BuildAdminLeaveViewModel(int userID, int year)
        {
            return new List<EmployeeAdminHistoryData>();
        }

        protected virtual IEnumerable<CompOffViewModel> BuildCompOffViewModel(int userID, int year)
        {
            return new List<CompOffViewModel>();
        }

        public async Task<LeaveViewModel<T>> GetViewModel(int userID, DateTime? fromDate, DateTime? toDate, int year)
        {
            return await Task.Run(() =>
            {
                LeaveViewModel<T> viewModel = new LeaveViewModel<T>();
                viewModel.EmployeeLeaveViewModels = BuildViewModel(userID, fromDate, toDate);
                viewModel.AdminLeaveDetails = BuildAdminLeaveViewModel(userID, year);
                viewModel.AvailableLeaves = BuildAvailableLeaves(userID, year);
                viewModel.CompOffsDetails = BuildCompOffViewModel(userID, year);
                return viewModel;
            });
        }
    }
}
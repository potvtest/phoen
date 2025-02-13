using Pheonix.Models;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ICompOffExceptionService 
    {
        Task<List<PersonViewModel>> GetManagerList();
        Task<List<PersonViewModel>> GetExecptionList();
        Task<ActiveEmpViewModel> GetActiveEmployeeList(string query, string managerID);
        Task<AdminActionResult> Add(List<CompOffExceptionViewModel> model);
        Task<AdminActionResult> Remove(List<CompOffExceptionViewModel> model);
    }
}

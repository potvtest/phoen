using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Web.Controllers.v1
{
    public interface IEmployeeContract
    {
        Task<EmployeeProfileViewModel> Profile(int id);


    }
}

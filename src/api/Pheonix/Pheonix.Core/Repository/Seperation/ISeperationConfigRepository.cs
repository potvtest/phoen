using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Pheonix.Models.VM;

namespace Pheonix.Core.Repository
{
    public interface ISeperationConfigRepository
    {
        IEnumerable<SeperationConfigViewModel> GetList(string filters);
        ActionResult Add(SeperationConfigViewModel model);
        ActionResult Update(SeperationConfigViewModel model);
        ActionResult Delete(int id);
        //IEnumerable<Role> GetRoleList(); 
        Task<List<DropdownItems>> GetRoleList();
        SeperationViewModel GetSeperationById(int id);

        #region SeparationReasonMaster
        ActionResult AddReason(SeparationReasonViewModel model);
        ActionResult UpdateReason(SeparationReasonViewModel model);
        ActionResult DeleteReason(int id);
        IEnumerable<SeparationReasonViewModel> GetReasonList(string filters);
        #endregion
    }
}


using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services
{
    public interface ISeperationConfigService
    {
        int UserId { get; set; }

        IEnumerable<SeperationConfigViewModel> GetList(string filters);

        ActionResult Add(SeperationConfigViewModel model);

        ActionResult Update(SeperationConfigViewModel model);
        ActionResult Delete(int id);

        Task<List<DropdownItems>> GetRoleList();
        //ActionResult Approve(SeperationViewModel model);

        #region SeparationReasonMaster
        ActionResult AddReason(SeparationReasonViewModel model);
        ActionResult UpdateReason(SeparationReasonViewModel model);
        ActionResult DeleteReson(int id);
        IEnumerable<SeparationReasonViewModel> GetReasonList(string filters);
        #endregion
    }
}

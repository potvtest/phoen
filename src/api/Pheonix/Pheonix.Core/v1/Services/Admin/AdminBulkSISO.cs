using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Admin
{
    class AdminBulkSISO : IAdminTask
    {
        public AdminActionResult TakeActionOn(IContextRepository repo, Models.Models.Admin.AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            string message = string.Empty;

            try
            {
                int i = Pheonix.Core.Helpers.QueryHelper.BulkSISO(model.From, model.To, model.EmployeeID, model.Comments, model.AdminID);
                result.isActionPerformed = true;
                result.message = string.Format("Bulk SISO done successfully");
            }
            catch (Exception)
            {
                result.isActionPerformed = false;
                result.message = string.Format("Bulk SISO was unsucessful");
            }

            return result;
        }

        //Not in use just needed to inherit
        public AdminActionResult Delete(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            result.isActionPerformed = true;          
            return result;
        }
    }
}

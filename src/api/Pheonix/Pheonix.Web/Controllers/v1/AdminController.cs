using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Core.v1.Services.Admin;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.VM.Classes.Employee;
using Pheonix.Web.Extensions;
using Pheonix.Web.Models;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("admin"), Authorize]
    public class AdminController : ApiController
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        //[HttpPost, Route("save-Arrears")]
        //public AdminActionResult SaveArrearManagement(PersonArrearViewModel model)
        //{

        //    AdminActionResult result = new AdminActionResult();

        //    using (var db = new PhoenixEntities())
        //    {
        //        PersonArrearTable arreartbl = null;
        //        //Check Dupliction
        //        if (db.PersonArrearTable.Count(f => f.ForDate == model.fromDate && f.EmployeeId == model.EmployeeId) > 0)
        //        {
        //            result.message = "Duplicete Arrear Not Allowed";
        //            result.isActionPerformed = false;
        //        }
        //        //check User Done SISO on that Day
        //        else if (db.SignInSignOut.Count(x => x.AttendanceDate == model.fromDate && x.DayNotation == "p" && x.UserID == model.EmployeeId) > 0)
        //        {
        //            result.message = "Emplyee was Present office that Day ";
        //            result.isActionPerformed = false;
        //        }
        //        else
        //        {
        //            arreartbl = new PersonArrearTable();
        //            arreartbl.comments = model.comments;
        //            arreartbl.ForDate = model.fromDate;
        //            arreartbl.LocationID = model.LocationID;
        //            arreartbl.CreatedBy = model.CreatedBy;
        //            arreartbl.ActionDate = DateTime.Now.Date;
        //            arreartbl.EmployeeId = model.EmployeeId;
        //            db.PersonArrearTable.Add(arreartbl);
        //            db.SaveChanges();
        //            result.message = "Arrear Leave Added Successufully";
        //            result.isActionPerformed = true;
        //        }
        //    }


        //    return result;
        //}

        //[HttpPost, Route("Delete-Arrears")]
        //public AdminActionResult DeleteArrearManagement(DeleletPersonArrearViewModel model)
        //{

        //    AdminActionResult result = new AdminActionResult();
        //    using (var ctx = new PhoenixEntities())
        //    {
        //        var DelRecord = (from s in ctx.PersonArrearTable
        //                         where s.ArrearID == model.id
        //                         select s).FirstOrDefault();

        //        ctx.PersonArrearTable.Remove(DelRecord);

        //        int num = ctx.SaveChanges();
        //    }
        //    result.message = "Arrear Leave Deleted Successufully";
        //    result.isActionPerformed = true;
        //    return result;
        //}

        [HttpPost, Route("task/{type:int}")]
        public async Task<AdminActionResult> TakeActionOn(AdminTaskModel model, int type)
        {
            if (!((new int[4] { 0, 1, 2, 3 }).Contains(type)))
                return null;

            AdminActionModel mainModel = new AdminActionModel
            {
                ActionType = (AdminTaskType)type,
                AdminID = RequestContext.GetClaimInt(ClaimTypes.PrimarySid),
                EmployeeID = model.EmployeeID,
                From = model.FromDate,
                To = model.ToDate,
                Quantity = model.Quantity,
                SubType = model.SubType,
                Validated = model.Validated,
                LocationID = model.locationID,
                Comments = model.Comments
            };
            return _service.TakeActionOn(mainModel);
        }

        [HttpPost, Route("delete/{id:int}")]
        public async Task<AdminActionResult> DeleteTask(EmployeeAdminHistoryData model, int id)
        {
            if (!((new int[3] { 0, 1, 2 }).Contains(model.ActionTypeID)))
                return null;

            AdminActionModel mainModel = new AdminActionModel
            {
                ActionType = (AdminTaskType)model.ActionTypeID,
                AdminID = RequestContext.GetClaimInt(ClaimTypes.PrimarySid),
                ID = model.ID,
                EmployeeID = id,
                From = model.FromDate.Value,
                To = model.ToDate == null? model.FromDate.Value: model.ToDate.Value,
                Count = model.Quantity.Value,
                LeaveType = model.LeaveType,
                Comments = model.Narration
            };
            return _service.Delete(mainModel);
        }
    }
}

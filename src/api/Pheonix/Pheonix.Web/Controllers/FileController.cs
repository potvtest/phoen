using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using System.Data;
using System.Linq;
using Pheonix.Helpers;
using Pheonix.DBContext;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.Helpers;
using System.Web.Mvc;
using Pheonix.Models.ViewModels;



namespace Pheonix.Web.Controllers
{
    //[Authorize] 
    [RoutePrefix("v1/File")]
    public class FileController : Controller
    {

        private ITimesheetService _TimesheetService;
        
        public FileController(ITimesheetService TimesheetService)
        {
            this._TimesheetService = TimesheetService;
        }
        //,/{statdate},/{enddate}
        [HttpPost, Route("download")]
        public ActionResult Download(HelpdeskExcelViewModelcs obj)
        {
            //return await _TimesheetService.DownloadExcelReport();

            var db = new PhoenixEntities();
            var reportList = new List<DataTable>();         

            var rmg_chart = db.rpt_HD_tickets(obj.StartDate,obj.EndDate,obj.EmpPrefix).ToList();
            var rmg_chart1 = db.rpt_HD_Issue_Status(obj.StartDate,obj.EndDate,obj.EmpPrefix).ToList();
            var rmg_chart2 = db.rpt_HD_Monthly_Status(obj.StartDate,obj.EndDate,obj.EmpPrefix).ToList();

            var rmg_raw = db.rpt_HD_Detail(obj.StartDate,obj.EndDate,obj.EmpPrefix).ToList();
            var d1 = ExcelExportHelper.ListToDataTable(rmg_chart);
            var d2 = ExcelExportHelper.ListToDataTable(rmg_chart1);
            var d3 = ExcelExportHelper.ListToDataTable(rmg_chart2);
            var d4 = ExcelExportHelper.ListToDataTable(rmg_raw);

            reportList.AddRange(new List<DataTable>{d1,d2,d3});
            reportList.AddRange(new List<DataTable> {d4});

            var headings = new List<string> {"Dashboard","RawData"};

            byte[] filecontent = ExcelExportHelper.ExportExcel(reportList, headings, false);

            return File(filecontent, ExcelExportHelper.ExcelContentType, "Helpdesk Issues List  " + DateTime.Now.ToString("yyyyMMddHHmm") + ".xlsx");
        }
        
    }
}
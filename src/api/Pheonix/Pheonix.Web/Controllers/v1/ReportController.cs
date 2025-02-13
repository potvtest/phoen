using Newtonsoft.Json.Linq;
using Pheonix.Core.v1.Services.Reports;
using Pheonix.DBContext;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Web.Extensions;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Pheonix.Web.Controllers.v1
{
    [Authorize]
    [RoutePrefix("v1/report")]
    public class ReportController : ApiController
    {
        private IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        [HttpPost, Route("{reportType}/view")]
        public List<Dictionary<string, object>> ViewReport([FromUri] string reportType, [FromBody] JObject reportQueryParams)
        {
            return _service.GetReport(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), reportType, reportQueryParams);
        }

        [HttpPost, Route("{reportType}/download")]
        public HttpResponseMessage DownloadReport(string reportType, [FromBody]List<object> reportQueryParams)
        {
            return _service.GetDownloadReport(reportType, reportQueryParams);
        }

        [HttpPost, Route("download")]
        public DownloadHelpdeskReport DownloadExcelReport(HelpdeskExcelViewModelcs obj)
        {
            return _service.GetExcelDownloadReport(obj);
        }

        [HttpGet, Route("GetReportListByPersonId/{personId}/{parentReportId}")]
        public IEnumerable<ReportAccessConfigViewModel> GetReportListByPersonId(int personId, int parentReportId)
        {
            return _service.GetReportListByPersonId(personId, parentReportId);
        }
        [HttpGet, Route("GetAllReportsListByEmployeeID/{personId}")]
        public List<ReportAccessConfigViewModel> GetAllReportsListByEmployeeID(int personID)
        {
            return _service.GetAllReportsListByEmployeeID(personID);
        }        
        [HttpPost, Route("UpdateReports/{personId}")]
        public async Task<List<ReportAccessConfig>> UpdateReports(int personID, List<ReportAccessConfig> model)
        {
            var context = new PhoenixEntities();
            foreach (var item in model)
            {
                var result = context.ReportAccessConfigs.Where(a => a.PersonID == personID && a.ReportID == item.ReportID).FirstOrDefault();
                var kt = context.ReportAccessConfigs.Where(a => a.PersonID == personID && a.ReportID == item.ReportID && a.IsActive == true).FirstOrDefault();
                var kf = context.ReportAccessConfigs.Where(a => a.PersonID == personID && a.ReportID == item.ReportID && a.IsActive == false).FirstOrDefault();
                if (result != null && kt != null)
                {
                    result.IsActive = item.IsActive;
                    await context.SaveChangesAsync();
                }
                else if (result != null && kf != null)
                {
                    result.IsActive = item.IsActive;
                    await context.SaveChangesAsync();
                }
                else if (result == null)
                {
                    context.ReportAccessConfigs.Add(item);
                    await context.SaveChangesAsync();
                }
            }
            return model;
        }
    }
    public class Test
    {
        public int MyProperty { get; set; }
        public int MyProperty1 { get; set; }
    }
}
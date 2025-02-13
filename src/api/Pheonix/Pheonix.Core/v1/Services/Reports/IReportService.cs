using Newtonsoft.Json.Linq;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Reports
{
    public interface IReportService
    {
        List<Dictionary<string, object>> GetReport(int userID, string reportName, JObject reportQueryParams);
        HttpResponseMessage GetDownloadReport(string reportName, List<object> reportQueryParams);
        DownloadHelpdeskReport GetExcelDownloadReport(HelpdeskExcelViewModelcs obj);
        IEnumerable<ReportAccessConfigViewModel> GetReportListByPersonId(int personId, int parentReportId);
        List<ReportAccessConfigViewModel> GetAllReportsListByEmployeeID(int personID);

    }
}
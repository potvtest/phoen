using Newtonsoft.Json.Linq;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Business;
using Pheonix.DBContext;
using Pheonix.Helpers;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Configuration;
using Pheonix.Models.ViewModels;
using AutoMapper;
using System.Globalization;

namespace Pheonix.Core.v1.Services.Reports
{
    public class ReportService : IReportService
    {

        //public ReportService(ITimesheetService TimesheetService)
        // {
        //     this._TimesheetService = TimesheetService;
        // } 
        #region Class Level Variable

        private IBasicOperationsService _service;
        private PhoenixEntities _phoenixEntity;

        #endregion Class Level Variable

        #region Constructor

        public ReportService(IBasicOperationsService service, PhoenixEntities phoenixEntity)
        {
            _service = service;
            _phoenixEntity = phoenixEntity;
        }

        public List<Dictionary<string, object>> GetReport(int userID, string reportName, JObject reportQueryParams)
        {
            ReportHelper helper = new ReportHelper();
            var jsonObject = reportQueryParams;

            if (reportName != "empleavebal")
            {
                jsonObject.Add("LoggedUser", userID.ToString());
            }

            foreach (var item in jsonObject)
            {
                if (item.Key == "Employeecode")
                {
                    if (jsonObject["Employeecode"].ToString() == "")
                    {
                        jsonObject["Employeecode"] = "All";
                    }
                }
                /* The code below will execute only for Employee Details report */
                if (reportName == "employeedetail" || reportName == "exitclearancereport")
                {
                    /* This code to convert array of filter option into stringfied format to pass as SP parameters */
                    if (item.Key == "EmploymentStatus")
                    {
                        jsonObject["EmploymentStatus"] = jsonObject["EmploymntStatusStringifiedList"];
                    }

                    if (item.Key == "DeliveryUnit")
                    {
                        jsonObject["DeliveryUnit"] = jsonObject["DUStringifiedList"];

                    }

                    if (item.Key == "DeliveryTeam")
                    {
                        jsonObject["DeliveryTeam"] = jsonObject["DTStringifiedList"];
                    }
                }

            }

            var values = jsonObject.ToObject<Dictionary<string, object>>();
            return helper.Run(reportName, values);
        }

        public HttpResponseMessage GetDownloadReport(string reportName, List<object> reportQueryParams)
        {
            //JObject json = JObject.Parse(reportQueryParams.ToString());
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            var sw = new StringBuilder();


            if (reportQueryParams.Count > 0)
            {
                JObject json = JObject.Parse(reportQueryParams[0].ToString());
                foreach (var property in json)
                {
                    var key = info.ToTitleCase(property.Key).Replace(" ", string.Empty);
                    sw.Append("\"" + key + "\",");
                }
                sw.Append("\r\n");

                for (var i = 0; i < reportQueryParams.Count; i++)
                {
                    JObject jsonData = JObject.Parse(reportQueryParams[i].ToString());
                    foreach (var property in jsonData)
                    {
                        if (property.Value.Type == JTokenType.Date)
                        {
                            sw.AppendFormat("\"{0}\",", Convert.ToDateTime(property.Value).ToString("MM/dd/yyyy"));
                        }
                        else
                        {
                            sw.AppendFormat("\"{0}\",", property.Value);
                        }

                    }
                    sw.AppendFormat("\r\n");
                }
            }

            Response.Content = new StringContent(sw.ToString());
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            Response.Content.Headers.ContentDisposition.FileName = "RecordExport.csv";
            return Response;

        }

        string GerReportPath()
        {
            string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        //string GerExcelReportPath()
        //{
        //    string path = string.Concat(GerReportPath(), @"PDF\");
        //    if (!Directory.Exists(path))
        //        Directory.CreateDirectory(path);
        //    return path;
        //}
        // Need To Veryfy By Ashwini
        public DownloadHelpdeskReport GetExcelDownloadReport(HelpdeskExcelViewModelcs obj)
        {
            DownloadHelpdeskReport objDownloadHelpdeskReport = new DownloadHelpdeskReport();
            var db = new PhoenixEntities();
            var reportList = new List<DataTable>();

            var rmg_chart = db.rpt_HD_tickets(obj.StartDate, obj.EndDate, obj.EmpPrefix).ToList();
            var rmg_chart1 = db.rpt_HD_Issue_Status(obj.StartDate, obj.EndDate, obj.EmpPrefix).ToList();

            var rmg_chart2 = db.rpt_HD_Monthly_Status(obj.StartDate, obj.EndDate, obj.EmpPrefix).ToList();
            var rmg_raw = db.rpt_HD_Detail(obj.StartDate, obj.EndDate, obj.EmpPrefix).ToList();
            var d1 = ExcelExportHelper.ListToDataTable(rmg_chart);

            var d2 = ExcelExportHelper.ListToDataTable(rmg_chart1);
            var d3 = ExcelExportHelper.ListToDataTable(rmg_chart2);

            DataTable newTable = d3.Copy();
            newTable.Columns.Remove("Sequesnce");

            var d4 = ExcelExportHelper.ListToDataTable(rmg_raw);

            reportList.AddRange(new List<DataTable> { d1, d2, newTable });
            reportList.AddRange(new List<DataTable> { d4 });

            var headings = new List<string> { "Dashboard", "RawData" };

            byte[] filecontent = ExcelExportHelper.ExportExcel(reportList, headings, false);
            string Folderpath = GerReportPath();
            string FileName = "HelpDeskIssuesList.xlsx";
            string ExcelFilepath = string.Concat(Folderpath, FileName);

            System.IO.File.WriteAllBytes(ExcelFilepath, filecontent);

            //File.WriteAllBytes(@"D:\DownloadReport\HelpDeskIssuesList.xlsx", filecontent);

            objDownloadHelpdeskReport.path = ExcelFilepath;

            return objDownloadHelpdeskReport;
        }

        public IEnumerable<ReportAccessConfigViewModel> GetReportListByPersonId(int personId, int parentReportId)
        {
            List<GetAllReportsByPersonId_Result> allReportsData = QueryHelper.GetAllReports(personId, parentReportId);
            var allReportsMappedData = Mapper.Map<IEnumerable<GetAllReportsByPersonId_Result>, IEnumerable<ReportAccessConfigViewModel>>(allReportsData);
            return allReportsMappedData;
        }
        #endregion Constructor

        public List<ReportAccessConfigViewModel> GetAllReportsListByEmployeeID(int personID)
        {
            List<ReportAccessConfigViewModel> reportAccessConfigViewModel = new List<ReportAccessConfigViewModel>();
            var db = new PhoenixEntities();
            var assignedParentAndChildReports = (from rm in db.ReportMasters
                                                 join rac in db.ReportAccessConfigs on
                                                      rm.Id equals rac.ReportMaster.Id
                                                 where rac.PersonID == personID
                                                 select new ReportAccessConfigViewModel()
                                                 {
                                                     ID = rm.Id,
                                                     Name = rm.Name,
                                                     Description = rm.Description,
                                                     ImageUrl = rm.ImageUrl,
                                                     ParentReportID = rm.ParentReportID,
                                                     RedirectionText = rm.RedirectionText,
                                                     IsActive = (bool)rac.IsActive,
                                                     DefaultToAll = (bool)rm.DefaultToAll,
                                                     ReportHeaderText = rm.ReportHeaderText,
                                                     SelectedReportText = rm.SelectedReportText,
                                                     RouteUrl = rm.RouteUrl
                                                 }).ToList();
            reportAccessConfigViewModel.AddRange(assignedParentAndChildReports);
            var subQuery = from rac in db.ReportAccessConfigs
                           where rac.PersonID == personID
                           select rac.ReportID;
            var nonAssignedReports = (from rm in db.ReportMasters
                                      where !subQuery.Contains(rm.Id)
                                      select new ReportAccessConfigViewModel()
                                      {
                                          ID = rm.Id,
                                          Name = rm.Name,
                                          Description = rm.Description,
                                          ImageUrl = rm.ImageUrl,
                                          ParentReportID = rm.ParentReportID,
                                          RedirectionText = rm.RedirectionText,
                                          IsActive = (rm.IsActive == true ? false : false),
                                          DefaultToAll = (bool)rm.DefaultToAll,
                                          ReportHeaderText = rm.ReportHeaderText,
                                          SelectedReportText = rm.SelectedReportText,
                                          RouteUrl = rm.RouteUrl
                                      }).ToList();
            reportAccessConfigViewModel.AddRange(nonAssignedReports);
            return reportAccessConfigViewModel;
        }
    }
}
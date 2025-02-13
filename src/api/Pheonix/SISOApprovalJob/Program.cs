using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using log4net;

namespace SISOApprovalJob
{
    public class Program
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;

        private static readonly string Template_Reminder = "SISO-Reminder";
        private static readonly string Template_AutoApproval = "SISO-Auto-Approval";

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    DateTime currentDate = DateTime.Now.Date;

                    if (args[0] == Template_Reminder)
                        if (ConfigurationManager.AppSettings[Template_Reminder].ToString() == "1")
                            if (entites.JobScheduler.Count(x => x.SchedulerType == Template_Reminder && x.SchedulerDate == currentDate && x.StatusType == false) > 0)
                                SendPendingApprovalReminder();

                    if (args[0] == Template_AutoApproval)
                        if (ConfigurationManager.AppSettings[Template_AutoApproval].ToString() == "1")
                            if (entites.JobScheduler.Count(x => x.SchedulerType == Template_AutoApproval && x.SchedulerDate == currentDate && x.StatusType == false) > 0)
                                AutoApproveSISO();
                }
            }
        }

        private static void SendPendingApprovalReminder()
        {
            try
            {
                Log4Net.Info("SISO Approval (Mail Sending) Job started: " + DateTime.Now);

                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                    var resultData = entites.GetSISOPendingApprovalDateWise("ALL", startDate, endDate).ToList();
                    var resultUsers = entites.GetSISOPendingApprovalUserWise("ALL", startDate, endDate).ToList();

                    if (resultData.Count() > 0)
                    {
                        SendPendingMail(resultData, resultUsers, DateTime.Now);
                        UpdateStatus();
                    }
                    else
                        Log4Net.Warn("SISO Approval (Mail Sending), No Records found, job finished: " + DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method (Mail Sending) - Exception Message: " + ex.Message);
                throw;
            }
            Log4Net.Info("SISO Approval (Mail Sending) job finished: " + DateTime.Now);
        }

        private static void AutoApproveSISO()
        {
            try
            {
                Log4Net.Info("SISO Approval (Auto-Approving) Job started: " + DateTime.Now);

                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    var resultData = entites.GetSISOPendingApprovalUserWise("ALL", null, null).ToList();

                    if (resultData.Count() > 0)
                    {
                        var pendingApprovalList = resultData.GroupBy(x => x.UserID).Select(x => x.FirstOrDefault());
                        int approverID = (int)pendingApprovalList.FirstOrDefault().ApproverID;
                        string userIDs = "";
                        string approverEmailIds = "";

                        foreach (var item in pendingApprovalList)
                        {
                            if (approverID != item.ApproverID && !string.IsNullOrEmpty(userIDs))
                            {
                                entites.ApproveePSISO_Person(approverID, userIDs);
                                userIDs = string.Empty;
                            }
                            userIDs += $"{(userIDs.Length > 0 ? "," : "")}{item.UserID}";

                            if (!approverEmailIds.Contains(item.ApproverOrganizationEmail))
                                approverEmailIds += $"{(approverEmailIds.Length > 0 ? "," : "")}{item.ApproverOrganizationEmail}";

                            approverID = (int)item.ApproverID;
                        }

                        if (!string.IsNullOrEmpty(userIDs))
                        {
                            entites.ApproveePSISO_Person(approverID, userIDs);
                            SendAutoApprovedMail(DateTime.Now, approverEmailIds);
                            UpdateStatus();
                        }
                    }
                    else
                    {
                        Log4Net.Warn("SISO Approval (Auto-Approve), No Records found, job finished: " + DateTime.Now);
                    }
                    CleanReportFiles();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method (Auto-Approve) - Exception Message: " + ex.Message);
                throw;
            }
            Log4Net.Info("SISO Approval (Auto-Approve) job finished: " + DateTime.Now);
        }

        private static void UpdateStatus()
        {
            try
            {
                DateTime currentDate = DateTime.Now.Date;
                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    var model = entites.JobScheduler.FirstOrDefault(x => x.SchedulerDate == currentDate);
                    if (model != null)
                    {
                        model.StatusType = true;
                        entites.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in UpdateStatus - Exception Message: " + ex.InnerException);
            }
        }

        private static void SendPendingMail(IEnumerable<GetSISOPendingApprovalDateWise_Result> model, IEnumerable<GetSISOPendingApprovalUserWise_Result> modelUsers, DateTime currentDateTime)
        {
            try
            {
                _container = UnityRegister.LoadContainer();

                var opsService = _container.Resolve<BasicOperationsService>();
                var ApproverList = model.GroupBy(x => x.ApproverID).Select(x => x.FirstOrDefault());
                PhoenixEntities entites = new PhoenixEntities();
                var templateDetails = opsService.First<EmailTemplate>(x => x.TemplateFor == Template_Reminder);
                string templateContent = templateDetails.Html.Replace("{{date}}", DateTime.Today.ToShortDateString());

                foreach (var item in ApproverList)
                {
                    var approverData = model.Where(x => x.ApproverID == item.ApproverID);
                    var userData = modelUsers.Where(x => x.ApproverID == item.ApproverID);

                    bool isCreated = opsService.Create<Emails>(new Emails
                    {
                        Content = templateContent,
                        Subject = $"{templateDetails.Subjects} {currentDateTime.ToString("dddd, dd MMMM yyyy")}",
                        Date = currentDateTime,
                        EmailFrom = ConfigurationManager.AppSettings["helpdeskEmailId"].ToString(),
                        EmailTo = item.ApproverOrganizationEmail,
                        EmailCC = ConfigurationManager.AppSettings["CCEmailId"].ToString(),
                        Attachments = GetAttachment(approverData, userData, currentDateTime),
                    }, e => e.Id == 0);

                    if (isCreated)
                        opsService.Finalize(true);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("SendPendingSISOMail :: Exception Message: " + ex.Message);
            }
        }

        private static void SendAutoApprovedMail(DateTime currentDateTime, string approverEmailIds)
        {

            try
            {
                _container = UnityRegister.LoadContainer();
                var opsService = _container.Resolve<BasicOperationsService>();

                var templateDetails = opsService.First<EmailTemplate>(x => x.TemplateFor == Template_AutoApproval);
                string templateContent = templateDetails.Html.Replace("{{date}}", DateTime.Today.ToShortDateString());
                string[] approverEmailIdsList = approverEmailIds.Split(',');
                
                foreach (var approverEmailId in approverEmailIdsList.OrderBy(x => x))
                {
                    bool isCreated = opsService.Create<Emails>(new Emails
                    {
                        Subject = $"{templateDetails.Subjects} {currentDateTime.ToString("dddd, dd MMMM yyyy")}",
                        Content = templateContent,
                        Date = currentDateTime,
                        EmailFrom = ConfigurationManager.AppSettings["helpdeskEmailId"].ToString(),
                        EmailTo = approverEmailId,
                        EmailCC = ConfigurationManager.AppSettings["CCEmailId"].ToString()
                    }, e => e.Id == 0);

                    if (isCreated)
                        opsService.Finalize(true);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("SendAutoApprovedSISOMail :: Exception Message: " + ex.Message);
            }
        }

        #region ExcelSheet Creation

        private static string[] GetDataColumnNames() => new string[] { "Employee Code", "Employee Name", "SignInTime", "SignOutTime", "TotalHoursWorked" };

        private static SheetData GenerateDataRows(SheetData sheetData, IEnumerable<GetSISOPendingApprovalDateWise_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    Row newRow = new Row();
                    newRow.AppendChild(GetGeneratedCell(item.UserID.ToString()));
                    newRow.AppendChild(GetGeneratedCell($"{item.FirstName} {item.MiddleName} {item.LastName}"));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.SignInTime)) ? string.Empty : (item.SignInTime).ToString()));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.SignOutTime)) ? string.Empty : (item.SignOutTime).ToString()));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.TotalHoursWorked)) ? string.Empty : (item.TotalHoursWorked).ToString()));

                    sheetData.AppendChild(newRow);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in the method GenerateDataRows Exception Message: " + ex.StackTrace);
                throw;
            }

            return sheetData;
        }

        private static string[] GetDataColumnNames_Users() => new string[] { "Employee Code", "Employee Name", "SignInTime", "SignOutTime" };

        private static SheetData GenerateDataRowsUsers(SheetData sheetData, IEnumerable<GetSISOPendingApprovalUserWise_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    string[] signInSignOutDate = item.SignInSignOutDate.Split(',');
                    string[] signInDate = signInSignOutDate[0].Split('|');
                    string[] signOutDate = signInSignOutDate[signInSignOutDate.Length - 1].Split('|');

                    Row newRow = new Row();
                    newRow.AppendChild(GetGeneratedCell(item.UserID.ToString()));
                    newRow.AppendChild(GetGeneratedCell($"{item.FirstName} {item.MiddleName} {item.LastName}"));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(signInDate[0])) ? string.Empty : (signInDate[0]).ToString()));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(signOutDate[1])) ? string.Empty : (signOutDate[1]).ToString()));

                    sheetData.AppendChild(newRow);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in the method GenerateDataRowsUsers Exception Message: " + ex.StackTrace);
            }

            return sheetData;
        }

        private static Cell GetGeneratedCell(string cellValue)
        {
            Cell objCell = new Cell();
            objCell.DataType = CellValues.String;
            objCell.CellValue = new CellValue(cellValue);
            return objCell;
        }

        private static Row GenerateHeaderRow(string[] dataColumns)
        {
            Row headerRow = new Row();
            List<string> columns = new List<string>();

            for (int n = 0; n < dataColumns.Length; n++)
            {
                columns.Add(dataColumns[n]);
                Cell cell = new Cell();
                cell.DataType = CellValues.InlineString;

                Run run1 = new Run();
                run1.Append(new Text(dataColumns[n]));

                RunProperties run1Properties = new RunProperties();
                run1Properties.Append(new Bold());
                run1.RunProperties = run1Properties;

                InlineString instr = new InlineString();
                instr.Append(run1);
                cell.Append(instr);

                headerRow.AppendChild(cell);
            }

            return headerRow;
        }

        private static WorksheetPart AddColumnsToSheet(WorksheetPart worksheetPart, string[] dataColumns)
        {
            Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
            bool needToInsertColumns = false;

            if (lstColumns == null)
            {
                lstColumns = new Columns();
                needToInsertColumns = true;
            }

            for (UInt32Value i = 1; i <= dataColumns.Length; i++)
                lstColumns.Append(new Column() { Min = i, Max = i, Width = 12, BestFit = true, CustomWidth = true });

            if (needToInsertColumns)
                worksheetPart.Worksheet.InsertAt(lstColumns, 0);

            return worksheetPart;
        }

        private static string GetAttachment(IEnumerable<GetSISOPendingApprovalDateWise_Result> model, IEnumerable<GetSISOPendingApprovalUserWise_Result> userModel, DateTime currentDateTime)
        {
            try
            {
                string fileName = string.Concat("SISOApprovalDetailsReportFor", "_", model.FirstOrDefault().ApproverID, "_", currentDateTime.ToString("yyyy_MM_dd"), ".xlsx");
                string filePath = string.Concat(GetFolderPath(), fileName);

                GenerateAttachmentFile(model, userModel, filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.StackTrace);
                return string.Empty;
            }
        }

        private static bool GenerateAttachmentFile(IEnumerable<GetSISOPendingApprovalDateWise_Result> model, IEnumerable<GetSISOPendingApprovalUserWise_Result> modelUser, string fileName)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
                {
                    var objPhoenixEntity = new PhoenixEntities();
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    Sheets sheets = workbookPart.Workbook.AppendChild<Sheets>(new Sheets()); //typical line. need to write outside of inner scope.
                    UInt32Value sheetCount = 1;

                    if (modelUser != null && modelUser.Count() > 0)
                    {
                        string[] dataColumns = GetDataColumnNames_Users();
                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData = new SheetData();
                        worksheetPart.Worksheet = new Worksheet(sheetData);

                        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetCount, Name = "WFH Summary Attendance" };
                        sheets.Append(sheet);
                        worksheetPart = AddColumnsToSheet(worksheetPart, dataColumns);

                        Row headerRow = GenerateHeaderRow(dataColumns);
                        sheetData.AppendChild(headerRow);
                        sheetData = GenerateDataRowsUsers(sheetData, modelUser);
                        sheetCount++;
                    }

                    if (model != null && model.Count() > 0)
                    {
                        string[] dataColumns = GetDataColumnNames();
                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData1 = new SheetData();
                        worksheetPart.Worksheet = new Worksheet(sheetData1);

                        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetCount, Name = "WFH Details Attendance" };
                        sheets.Append(sheet);
                        worksheetPart = AddColumnsToSheet(worksheetPart, dataColumns);

                        Row headerRow = GenerateHeaderRow(dataColumns);
                        sheetData1.AppendChild(headerRow);
                        sheetData1 = GenerateDataRows(sheetData1, model);
                    }

                    workbookPart.Workbook.Save();
                    document.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in GenerateAttachmentFileAsExcel Method Exception Message: " + ex.StackTrace);
                throw;
            }
            return true;
        }

        private static string GetFolderPath()
        {
            try
            {
                string path = string.Concat(ConfigurationManager.AppSettings["ReportPath"].ToString(), @"SISOApprovalDetailsReport\");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.StackTrace);
                return string.Empty;
            }
        }

        #endregion

        private static void CleanReportFiles()
        {
            var filePath = GetFolderPath();
            try
            {
                DirectoryInfo di = new DirectoryInfo(filePath);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
            catch
            {
            }
        }
    }
}

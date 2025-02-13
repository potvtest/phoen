using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using log4net;
using Microsoft.Practices.Unity;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LeaveDetailsJob
{
    class LeaveDetailsMailSender
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;
        static void Main(string[] args)
        {
            DateTime currentDateTime = DateTime.Now;
            try
            {
                Log4Net.Debug("Leave Details job started: =" + currentDateTime);
                DateTime startDate = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                PhoenixEntities entites = new PhoenixEntities();
                var model = entites.rpt_LeaveDetail("All", "", startDate, endDate, null, 1195).ToList();

                if (model != null)
                    SendLeaveDetailsReport(model, currentDateTime);
                else
                    Log4Net.Error("There is no data for Digital Platform");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method Exception Message: " + ex.StackTrace);
            }
            Log4Net.Debug("Leave Details job finished: =" + currentDateTime);
        }

        private static void SendLeaveDetailsReport(List<rpt_LeaveDetail_Result> model, DateTime currentDateTime)
        {
            try
            {
                string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                string emailTo = Convert.ToString(ConfigurationManager.AppSettings["DigitalPlateformDL"]);
                _container = UnityRegister.LoadContainer();
                var opsService = _container.Resolve<BasicOperationsService>();
                bool isCreated = opsService.Create<Emails>(new Emails
                {
                    Content = "Hi Team, <br /> Please find attached report for leave details. <br /> Thanks.",
                    Date = currentDateTime,
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,
                    EmailCC = string.Empty,
                    Subject = "Leave Details for Digital Platform - " + currentDateTime,
                    Attachments = GetReportDetailsAttachment(model, currentDateTime),

                }, e => e.Id == 0);
                if (isCreated)
                    opsService.Finalize(true);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method SendLeaveDetailsReport, Exception Message: " + ex.StackTrace);
            }
        }

        private static string GetReportDetailsAttachment(List<rpt_LeaveDetail_Result> model, DateTime currentDateTime)
        {
            try
            {
                string fileName = string.Concat("LeaveDetailsReportFor","_", currentDateTime.ToString("yyyy_MM_dd"), ".xlsx");
                string filePath = string.Concat(GetFolderPath(), fileName);
                bool isExcelCreated = false;
                isExcelCreated = GenerateAttachmentFileAsExcel(model, filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.StackTrace);
                return string.Empty;
            }
        }

        static string GetFolderPath()
        {
            try
            {
                string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
                path = string.Concat(path, @"LeaveDetails\");

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


        private static bool GenerateAttachmentFileAsExcel(List<rpt_LeaveDetail_Result> model, string fileName)
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

                    if (model != null && model.Count > 0)
                    {
                        string[] dataColumns = GetDataColumnNames();
                        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        SheetData sheetData = new SheetData();
                        worksheetPart.Worksheet = new Worksheet(sheetData);
                        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetCount, Name = "LeaveDetails" };
                        sheets.Append(sheet);
                        #region //adding columns to sheet

                        worksheetPart = AddColumnsToSheet(worksheetPart, dataColumns);

                        #endregion //end columns to sheet
                        #region //Add Header Row

                        Row headerRow = GenerateHeaderRow(dataColumns);
                        sheetData.AppendChild(headerRow);

                        #endregion //Add Header Row
                        #region //adding data to sheet

                        sheetData = GenerateDataRows(sheetData, model);

                        #endregion //adding data to sheet
                        sheetCount++;
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

        private static SheetData GenerateDataRows(SheetData sheetData, List<rpt_LeaveDetail_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    Row newRow = new Row();

                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Employee_Code)) ? string.Empty : Convert.ToString(item.Employee_Code)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Name) ? string.Empty : item.Employee_Name));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Leave_Type) ? string.Empty : item.Leave_Type));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Start_Date) ? string.Empty : item.Start_Date));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.End_Date) ? string.Empty : item.End_Date));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Leave_Status) ? string.Empty : item.Leave_Status));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Applied_For_Days)) ? string.Empty : Convert.ToString(item.Applied_For_Days)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Consumed_Days)) ? string.Empty : Convert.ToString(item.Consumed_Days)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Approved_By) ? string.Empty : item.Approved_By));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Narration) ? string.Empty : item.Narration));

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
            //adding column
            List<String> columns = new List<string>();

            for (int n = 0; n < dataColumns.Length; n++)
            {
                columns.Add(dataColumns[n]);
                Cell cell = new Cell();
                cell.DataType = CellValues.InlineString;

                //execute run for bold
                Run run1 = new Run();
                run1.Append(new Text(dataColumns[n]));
                RunProperties run1Properties = new RunProperties();
                run1Properties.Append(new Bold());
                run1.RunProperties = run1Properties;
                //complete run
                //create a new inline string and append it
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
            Boolean needToInsertColumns = false;
            if (lstColumns == null)
            {
                lstColumns = new Columns();
                needToInsertColumns = true;
            }
            for (UInt32Value i = 1; i <= dataColumns.Length; i++)
            {
                lstColumns.Append(new Column() { Min = i, Max = i, Width = 10, BestFit = true, CustomWidth = true });
            }
            if (needToInsertColumns)
                worksheetPart.Worksheet.InsertAt(lstColumns, 0);

            return worksheetPart;
        }

        private static string[] GetDataColumnNames()
        {
            string[] objColumnNames = new string[] {
                                                       "Employee Code",
                                                       "Employee Name",
                                                       "Leave Type",
                                                       "Start Date",
                                                       "End Date",
                                                       "Leave Status",
                                                       "Applied For Days",
                                                       "Consumed Days",
                                                       "Approved By",
                                                       "Narration"

            };
            return objColumnNames;
        }
    }
}

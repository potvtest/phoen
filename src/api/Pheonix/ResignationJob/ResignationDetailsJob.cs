using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using System.Configuration;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using Pheonix.DBContext;
using Pheonix.Core.v1.Services;
using Microsoft.Practices.Unity;

namespace ResignationJob
{
    class ResignationDetailsJob 
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;
        static void Main(string[] args)
        {
            try
            {
                Log4Net.Debug("Resignation Details job started: =" + DateTime.Now);
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                PhoenixEntities entites = new PhoenixEntities();
                var model = entites.GetReportForResignationData(startDate, endDate).ToList();

                if (model != null)
                    SendResignationDetailsReport(model, DateTime.Now);
                else
                    Log4Net.Error("There is no Resignation Data");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method Exception Message: " + ex.StackTrace);
            }
            Log4Net.Debug("Resignation Details job finished: =" + DateTime.Now);
        }

        private static void SendResignationDetailsReport(List<GetReportForResignationData_Result> model, DateTime currentDateTime)
        {
            try
            {
                string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                string emailTo = Convert.ToString(ConfigurationManager.AppSettings["HREmailId"]);

                _container = UnityRegister.LoadContainer();

                var opsService = _container.Resolve<BasicOperationsService>();
                bool isCreated = opsService.Create<Emails>(new Emails
                {
                    Content = "Hi Team, <br /> Please find attached report for Resignation details. <br /> Thanks.",
                    Date = currentDateTime,
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,
                    EmailCC = string.Empty,
                    Subject = "Resignation Details Report" + currentDateTime,
                    Attachments = GetReportDetailsAttachment(model, currentDateTime),
                }, e => e.Id == 0);
                if (isCreated)
                    opsService.Finalize(true);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method SendResignationDetailsReport, Exception Message: " + ex.StackTrace);
            }
        }

        private static string GetReportDetailsAttachment(List<GetReportForResignationData_Result> model, DateTime currentDateTime)
        {
            try
            {
                string fileName = string.Concat("ResignationDetailsReportFor", "_", currentDateTime.ToString("yyyy_MM_dd"), ".xlsx");
                string filePath = string.Concat(GetFolderPath(), fileName);
                GenerateAttachmentFileAsExcel(model, filePath);
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
                path = string.Concat(path, @"ResignationDetailsReport\");

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

        private static bool GenerateAttachmentFileAsExcel(List<GetReportForResignationData_Result> model, string fileName)
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
                        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetCount, Name = "ResignationDetails" };
                        sheets.Append(sheet);                        
                        worksheetPart = AddColumnsToSheet(worksheetPart, dataColumns);
                        Row headerRow = GenerateHeaderRow(dataColumns);
                        sheetData.AppendChild(headerRow);
                        sheetData = GenerateDataRows(sheetData, model);
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

        private static SheetData GenerateDataRows(SheetData sheetData, List<GetReportForResignationData_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    Row newRow = new Row();
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.PersonID)) ? string.Empty : Convert.ToString(item.PersonID)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Name) ? string.Empty : item.Employee_Name));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Designation) ? string.Empty : item.Designation));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ResignDate)) ? string.Empty : (item.ResignDate).ToString("dd-MM-yyyy")));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.SystemLastDate)) ? string.Empty : (item.SystemLastDate)?.ToString("dd-MM-yyyy")));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ExpectedDate)) ? string.Empty : (item.ExpectedDate).ToString("dd-MM-yyyy")));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)) ? string.Empty : (item.ApprovalDate)?.ToString("dd-MM-yyyy")));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ActualExitDate)) ? string.Empty : (item.ActualExitDate)?.ToString("dd-MM-yyyy")));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Comments) ? string.Empty : item.Comments));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Skills) ? string.Empty : item.Skills));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.DU) ? string.Empty : item.DU));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.DT) ? string.Empty : item.DT));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Resource_Pool) ? string.Empty : item.Resource_Pool));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.JoiningDate)) ? string.Empty : (item.JoiningDate)?.ToString("dd-MM-yyyy")));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.V2_Experience)) ? string.Empty : Convert.ToString(item.V2_Experience)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Location) ? string.Empty : item.Location));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employment_Status) ? string.Empty : item.Employment_Status));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Reporting_To) ? string.Empty : item.Reporting_To));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Confirmation_Exit_Manager) ? string.Empty : item.Confirmation_Exit_Manager));

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
            
            List<String> columns = new List<string>();

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
                lstColumns.Append(new Column() { Min = i, Max = i, Width = 9, BestFit = true, CustomWidth = true });

            if (needToInsertColumns)
                worksheetPart.Worksheet.InsertAt(lstColumns, 0);

            return worksheetPart;
        }

        private static string[] GetDataColumnNames()
        {
            string[] objColumnNames = new string[] {
                                                       "PersonID",
                                                       "Employee Name",
                                                       "Designation",
                                                       "ResignDate",
                                                       "SystemLastDate",
                                                       "ExpectedDate",
                                                       "ApprovalDate",
                                                       "ActualExitDate",
                                                       "Comments",
                                                       "Skills",
                                                       "DU",
                                                       "DT",
                                                       "Resource Pool",
                                                       "JoiningDate",
                                                       "V2 Experience",
                                                       "Location",
                                                       "Employment Status",
                                                       "Reporting To",
                                                       "Confirmation/Exit Manager"
            };
            return objColumnNames;
        }
    }
}
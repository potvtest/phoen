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
using CelebrationlistJob;
using Pheonix.Core.Helpers;

namespace CelebrationListJob
{
    class CelebrationListMailSender
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;

        static void Main(string[] args)
        {
            DateTime currentDateTime = DateTime.Now;
            lstTemlpates = fillTemplates();
            try
            {
                Log4Net.Debug("Celebration List job started: =" + DateTime.Now);

                DateTime startDate = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                PhoenixEntities entites = new PhoenixEntities();

                var birthDateModel = entites.GetCelebrationListForBirthDate(startDate, endDate).ToList();
                var weddingDateModel = entites.GetCelebrationListForWeddingDate(startDate, endDate).ToList();
                var workAnniModel = entites.GetCelebrationListForWorkAnniversary(startDate, endDate).ToList();

                if (birthDateModel != null & weddingDateModel != null & workAnniModel != null )
                    SendCelebrationListDetails(birthDateModel, weddingDateModel, workAnniModel, DateTime.Now);

                else
                    Log4Net.Error("There is no data");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method Exception Message: " + ex.StackTrace);
            }
            Log4Net.Debug("Celebration List job finished: =" + DateTime.Now);
        }

        private static List<EmailTemplate> fillTemplates()
        {
            using (PhoenixEntities entity = new PhoenixEntities())
                return entity.EmailTemplate.ToList();
        }

        private static List<EmailTemplate> lstTemlpates = null;

        private static void SendCelebrationListDetails(IEnumerable<GetCelebrationListForBirthDate_Result> birthDateModel, IEnumerable<GetCelebrationListForWeddingDate_Result> weddingDateModel, IEnumerable<GetCelebrationListForWorkAnniversary_Result> workAnniModel , DateTime currentDateTime)
        {
            try
            {
                string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                string emailTo = Convert.ToString(ConfigurationManager.AppSettings["HREmailId"]);

                _container = UnityRegister.LoadContainer();
                var opsService = _container.Resolve<BasicOperationsService>();
                var emailTemplate = lstTemlpates.Where(x => x.TemplateFor == EnumExtensions.GetEnumDescription((EnumHelpers.EmailTemplateType.CelebrationList))).First();
                string fileName = string.Concat("CelebrationListReport", "_", currentDateTime.ToString("yyyy_MM_dd"), ".xlsx");
                string filePath = GetFolderPath() + fileName;
                var flag = GenerateAttachmentFileAsExcel(birthDateModel, weddingDateModel, workAnniModel, filePath);
                string template = emailTemplate.Html;
                string currentMonthYear = DateTime.Now.ToString("MMMM yyyy");
                template = template.Replace("{{MonthYear}}", currentMonthYear);
                bool isCreated = opsService.Create<Emails>(new Emails
                {
                    Content = template,
                    Date = currentDateTime,
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,
                    EmailCC = string.Empty,
                    Subject = "Celebration List :: " + currentDateTime.ToString("dddd, dd MMMM yyyy"),
                    Attachments = filePath,

                }, e => e.Id == 0);

                if (isCreated)
                    opsService.Finalize(true);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in Main method SendCelebrationListDetails, Exception Message: " + ex.StackTrace);
            }
        }

        static string GetFolderPath()
        {
            try
            {
                string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
                path = string.Concat(path, @"CelebrationList\");

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

        private static string GenerateAttachmentFileAsExcel(IEnumerable<GetCelebrationListForBirthDate_Result> birthDateModel, IEnumerable<GetCelebrationListForWeddingDate_Result> weddingDateModel, IEnumerable<GetCelebrationListForWorkAnniversary_Result> workAnniModel, string filePath)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    Sheets sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    // Excel sheet 1
                    WorksheetPart worksheetPart1 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet1 = new Worksheet();
                    SheetData sheetData1 = new SheetData();

                    Sheet sheet1 = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart1),
                        SheetId = 1,
                        Name = "Birthday"
                    };
                    sheets.Append(sheet1);

                    string[] dataColumns = GetDataColumnNamesForBirthDate();
                    worksheetPart1.Worksheet = new Worksheet(sheetData1);
                    worksheetPart1 = AddColumnsToSheet(worksheetPart1, dataColumns);
                    Row headerRow = GenerateHeaderRow(dataColumns);
                    sheetData1.AppendChild(headerRow);
                    sheetData1 = GenerateDataRowsForBirthDate(sheetData1, birthDateModel);

                    // Excel sheet 2
                    WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet2 = new Worksheet();
                    SheetData sheetData2 = new SheetData();

                    Sheet sheet2 = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart2),
                        SheetId = 2,
                        Name = "Wedding_Anniversary"
                    };
                    sheets.Append(sheet2);

                    string[] dataColumns2 = GetDataColumnNamesForWeddingDate();
                    worksheetPart2.Worksheet = new Worksheet(sheetData2);
                    worksheetPart2 = AddColumnsToSheet(worksheetPart2, dataColumns2);
                    Row headerRow2 = GenerateHeaderRow(dataColumns2);
                    sheetData2.AppendChild(headerRow2);
                    sheetData2 = GenerateDataRowsForWeddingDate(sheetData2, weddingDateModel);

                    // Excel sheet 2
                    WorksheetPart worksheetPart3 = workbookPart.AddNewPart<WorksheetPart>();
                    Worksheet workSheet3 = new Worksheet();
                    SheetData sheetData3 = new SheetData();

                    Sheet sheet3 = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart3),
                        SheetId = 3,
                        Name = "Work_Anniversary"
                    };
                    sheets.Append(sheet3);

                    string[] dataColumns3 = GetDataColumnNamesForWorkAnniDate();
                    worksheetPart3.Worksheet = new Worksheet(sheetData3);
                    worksheetPart3 = AddColumnsToSheet(worksheetPart3, dataColumns3);
                    Row headerRow3 = GenerateHeaderRow(dataColumns3);
                    sheetData3.AppendChild(headerRow3);
                    sheetData3 = GenearateDataRowsWorkAnniDate(sheetData3, workAnniModel);

                    workbookPart.Workbook.Save();
                    document.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in GenerateAttachmentFileAsExcel Method Exception Message: " + ex.StackTrace);
                throw;
            }
            return "success";
        }

        private static SheetData GenerateDataRowsForBirthDate(SheetData sheetData, IEnumerable<GetCelebrationListForBirthDate_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    Row newRow = new Row();

                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Employee_Code)) ? string.Empty : Convert.ToString(item.Employee_Code)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Name) ? string.Empty : item.Employee_Name));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Date_of_Birth) ? string.Empty : item.Date_of_Birth));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.SpouseName) ? string.Empty : item.SpouseName));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Office_Location) ? string.Empty : item.Office_Location));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.On_Notice_Period) ? string.Empty : item.On_Notice_Period));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Email) ? string.Empty : item.Employee_Email));

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

        private static SheetData GenerateDataRowsForWeddingDate(SheetData sheetData, IEnumerable<GetCelebrationListForWeddingDate_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    Row newRow = new Row();

                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Employee_Code)) ? string.Empty : Convert.ToString(item.Employee_Code)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Name) ? string.Empty : item.Employee_Name));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Wedding_Date) ? string.Empty : item.Wedding_Date));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.SpouseName) ? string.Empty : item.SpouseName));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Office_Location) ? string.Empty : item.Office_Location));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.On_Notice_Period) ? string.Empty : item.On_Notice_Period));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Email) ? string.Empty : item.Employee_Email));

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

        private static SheetData GenearateDataRowsWorkAnniDate(SheetData sheetData, IEnumerable<GetCelebrationListForWorkAnniversary_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    Row newRow = new Row();

                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Employee_Code)) ? string.Empty : Convert.ToString(item.Employee_Code)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Name) ? string.Empty : item.Employee_Name));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Joining_Date) ? string.Empty : item.Joining_Date));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.SpouseName) ? string.Empty : item.SpouseName));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Office_Location) ? string.Empty : item.Office_Location));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.On_Notice_Period) ? string.Empty : item.On_Notice_Period));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Email) ? string.Empty : item.Employee_Email));

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

        private static string[] GetDataColumnNamesForBirthDate()
        {
            string[] objColumnNames = new string[] {
                                                       "Employee Code",
                                                       "Employee Name",
                                                       "Date of Birth",
                                                       "SpouseName",
                                                       "Office Location",
                                                       "On Notice Period",
                                                       "Employee Email"
            };
            return objColumnNames;
        }

        private static string[] GetDataColumnNamesForWeddingDate()
        {
            string[] objColumnNames = new string[] {
                                                       "Employee Code",
                                                       "Employee Name",
                                                       "Wedding Date",
                                                       "SpouseName",
                                                       "Office Location",
                                                       "On Notice Period",
                                                       "Employee Email"
            };
            return objColumnNames;
        }

        private static string[] GetDataColumnNamesForWorkAnniDate()
        {
            string[] objColumnNames = new string[] {
                                                       "Employee Code",
                                                       "Employee Name",
                                                       "Joining Date",
                                                       "SpouseName",
                                                       "Office Location",
                                                       "On Notice Period",
                                                       "Employee Email"
            };
            return objColumnNames;
        }       
    }
}
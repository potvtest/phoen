using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System.Data.Entity;
using System.Configuration;
using Microsoft.Practices.Unity;
using System.IO;
using log4net.Config;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace CurrentAllocationsJob
{
    class CurrentAllocationMailSender
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IUnityContainer _container;

        static void Main(string[] args)
        {
            DateTime currentDateTime = DateTime.Now;
            try
            {
                XmlConfigurator.Configure();

                Log4Net.Debug("Current Allocations job started: =" + currentDateTime);
                PhoenixEntities entites = new PhoenixEntities();
                string deliveryUnit = Convert.ToString(ConfigurationManager.AppSettings["DeliveryUnit"]);
                int deliveryUnitID = entites.DeliveryUnit.Where(x => x.Name.Trim().ToLower().Equals(deliveryUnit.Trim().ToLower())).FirstOrDefault().ID;
                var model = entites.rpt_Current_Allocation1(deliveryUnitID).ToList();

                if (model != null)
                {
                    SendAllocationReport(model, currentDateTime);
                }
                else
                {
                    Log4Net.Error("There is no allocation for delivery unit - " + deliveryUnit);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
            }
            Log4Net.Debug("Current Allocation job finished: =" + DateTime.Now);
        }

        private static void SendAllocationReport(List<rpt_Current_Allocation1_Result> model, DateTime currentDateTime)
        {
            try
            {
                string emailFrom = Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]);
                string emailTo = Convert.ToString(ConfigurationManager.AppSettings["DigitalPlateformDL"]);

                _container = UnityRegister.LoadContainer();

                var opsService = _container.Resolve<BasicOperationsService>();

                bool isCreated = opsService.Create<Emails>(new Emails
                {
                    //Since this a temporary arrangement to send the allocation report to the managers we are not separating the templates from code.
                    Content = "Hi Team, <br /> Please find attached report for current allocations. <br /> Thanks.",
                    Date = currentDateTime,
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,
                    EmailCC = string.Empty,
                    Subject = "Current Allocation for Digital Platform - " + currentDateTime,
                    Attachments = GetCurrentAllocationAttachment(model, currentDateTime),

                }, e => e.Id == 0);

                if (isCreated)
                    opsService.Finalize(true);
            }
            catch
            {
                throw;
            }
        }

        private static string GetCurrentAllocationAttachment(List<rpt_Current_Allocation1_Result> model, DateTime currentDateTime)
        {
            try
            {
                string fileName = string.Concat("CurrentAllocationFor", "_", currentDateTime.ToString("yyyy_MM_dd"), ".xlsx");
                string filePath = string.Concat(GetFolderPath(), fileName);
                bool isExcelCreated = false;

                isExcelCreated = GenerateAttachmentFileAsExcel(model, filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
                return string.Empty;
            }
        }

        private static bool GenerateAttachmentFileAsExcel(List<rpt_Current_Allocation1_Result> model, string fileName)
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

                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetCount, Name = "CurrentAllocations" };

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
            return true;
        }

        private static SheetData GenerateDataRows(SheetData sheetData, List<rpt_Current_Allocation1_Result> model)
        {
            foreach (var item in model)
            {
                Row newRow = new Row();

                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Employee_Code)) ? string.Empty: Convert.ToString(item.Employee_Code)));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Name) ? string.Empty : item.Employee_Name));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Designation) ? string.Empty : item.Designation));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employment_Status) ? string.Empty : item.Employment_Status));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Project_Delivery_Team) ? string.Empty : item.Project_Delivery_Team));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Resource_Utilization)) ? string.Empty : Convert.ToString(item.Resource_Utilization)));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Resource_Status) ? string.Empty : item.Resource_Status));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Resource_Percentage_Loading__)) ? string.Empty : Convert.ToString(item.Resource_Percentage_Loading__)));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Project_Reporting_Manager) ? string.Empty : item.Project_Reporting_Manager));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Org__Reporting_Manager) ? string.Empty : item.Org__Reporting_Manager));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Exit_Process_Manager) ? string.Empty : item.Exit_Process_Manager));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Work_Location) ? string.Empty : item.Work_Location));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Office_Location) ? string.Empty : item.Office_Location));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Employee_Skill_Set) ? string.Empty : item.Employee_Skill_Set));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Project_Name) ? string.Empty : item.Project_Name));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Start_Date) ? string.Empty : item.Start_Date));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.End_Date) ? string.Empty : item.End_Date));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Release_Date) ? string.Empty : item.Release_Date));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Project_Role) ? string.Empty : item.Project_Role));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Project_Delivery_Unit) ? string.Empty : item.Project_Delivery_Unit));
                newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Resource_Pool) ? string.Empty : item.Resource_Pool));

                sheetData.AppendChild(newRow);
            }
            return sheetData;
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
                lstColumns.Append(new Column() { Min = i, Max = i, Width = 20, BestFit = true, CustomWidth = true });
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
                                                        "Designation",
                                                        "Employment Status",
                                                        "Project Delivery Team",
                                                        "Resource Utilization",
                                                        "Resource Status",
                                                        "Resource Percentage/ Loading %",
                                                        "Project Reporting Manager",
                                                        "Org.Reporting Manager",
                                                        "Exit Process Manager",
                                                        "Work Location",
                                                        "Office Location",
                                                        "Employee Skill Set",
                                                        "Project Name",
                                                        "Start Date",
                                                        "End Date",
                                                        "Release Date",
                                                        "Project Role",
                                                        "Project Delivery Unit",
                                                        "Resource Pool"
                    };
            return objColumnNames;
        }

        private static Cell GetGeneratedCell(string cellValue)
        {
            Cell objCell = new Cell();
            objCell.DataType = CellValues.String;
            objCell.CellValue = new CellValue(cellValue);
            return objCell;
        }

        static string GetFolderPath()
        {
            try
            {
                string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
                path = string.Concat(path, @"Allocation\");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
                return string.Empty;
            }
        }

    }
}



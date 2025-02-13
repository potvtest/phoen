using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Pheonix.Core.v1.Builders
{
    public class ReportBuilder
    {
        public static ReportSetting GetReportProcedureSettings(string reportName)
        {
            ReportSetting setting;
            setting = GetReportSetting(reportName);
            List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
            setting.Transformer = (reader) =>
            {
                executeReader(reader, data);

                return data;
            };

            return setting;
        }

        static void executeReader(IDataReader reader, List<Dictionary<string, object>> data)
        {
            while (reader.Read())
            {
                Dictionary<string, object> itemData = new Dictionary<string, object>();
                itemData = Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue);
                data.Add(itemData);
            }

            if (reader.NextResult())
                executeReader(reader, data);

            reader.Close();
        }

        private static ReportSetting GetReportSetting(string reportName)
        {
            ReportSetting setting = null;
            switch (reportName)
            {
                case "attendance":
                    setting = AttendanceReport("rpt_" + reportName);
                    break;
                case "attendancesiso":
                    setting = AttendanceSISOReport("rpt_attendanceSISO");
                    break;
                case "rmg":
                    setting = HelpdeskGraphReport("rpt_HD_All");
                    break;
                case "rmgdetail":
                    setting = HelpdeskDetailReport("rpt_HD_detail");
                    break;
                case "rmgbenchlessthan":
                    setting = RMGBenchLessThanReport("rpt_bench_less_than_100_new");
                    break;
                case "rmgbenchnew":
                    setting = RMGBenchNewReport("rpt_bench_report_new");
                    break;
                case "rmgcurrentallocation":
                    setting = RMGCurrentAllocationReport("rpt_Current_Allocation");
                    break;
                case "rmgmonthallocation":
                    setting = RMGMonthAllocationReport("rpt_Monthly_Allocation");
                    break;
                case "employeedetail":
                    setting = EmployeeDetailReport("rpt_" + reportName);
                    break;
                case "leavesummary":
                    setting = LeaveLedgerReport("rpt_LeaveLedgerSummary");
                    break;
                case "leavedetail":
                    setting = LeaveDetailReport("rpt_" + reportName);
                    break;
                case "helpdesk":
                    setting = HelpdeskReport("rpt_" + reportName);
                    break;
                case "leavetransaction":
                    setting = LeaveTransactionReport("rpt_" + reportName);
                    break;
                case "siso":
                    setting = SisoReport("rpt_" + reportName);
                    break;
                case "managerleave":
                    setting = ManagerLeaveReport("rpt_" + reportName);
                    break;
                case "empleavebal":
                    setting = EmpLeaveBalReport("rpt_" + reportName);
                    break;
                case "helpdeskdetails":
                    setting = HelpdeskDetails("rpt_" + reportName);
                    break;
                case "empskilldetails":
                    setting = EmpSkillDetails("rpt_" + reportName);
                    break;
                case "exitclearancereport":
                    setting = ExitClearanceReport("rpt_" + reportName);
                    break;
                case "empskillupdate":
                    setting = Empskillupdate("rpt_" + reportName);
                    break;
                case "empcoordinates":
                    setting = EmpCoordinates("rpt_" + reportName);
                    break;
                case "empicdetails":
                    setting = EmpICDetails("rpt_" + reportName);
                    break;
                case "vwallocation":
                    setting = VWAllocation("rpt_VW_Allocation");
                    break;
                case "vwmonthlyallocation":
                    setting = VWMonthlyAllocation("rpt_VW_Monthly_Allocation");
                    break;
                case "vwbench":
                    setting = VWBench("rpt_VW_Bench");
                    break;
                case "vwbenchlessthan100":
                    setting = VWBenchLessThan100("rpt_VW_Bench_less_than_100");
                    break;
                case "invoicing":
                    setting = Invoicing("rpt_invoicing");
                    break;
                case "resourceallocationmo":
                    setting = Invoicing("rpt_resourceAllocationMO");
                    break;
                //case "customerbgclist":
                //    setting = CustomerbgclistReport("rpt_CustomerBGCList");
                //    break;
                case "rmgobjective":
                    setting = RMGObjectiveReport("rpt_RMGObjective");
                    break;
                case "employeebgstatus":
                    setting = EmployeeBGStatusReport("rpt_EmployeeBGStatus");
                    break;
                //case "bgcompatibility":
                //    setting = BGCompatibilityReport("rpt_BGCompatibility");
                //    break;
                case "helpdeskgraph":
                    setting = HelpdeskGraphReport("rpt_HD_All");
                    break;
                case "empresignationdetail":
                    setting = EmpResignationDetail("rpt_" + reportName);
                    break;
                default:
                    break;
            }
            return setting;
        }

        private static ReportSetting ManagerLeaveReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }
            };
        }

        private static ReportSetting EmpLeaveBalReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("UserID", ""), new SqlParameter("Year", "") }              
            };
        }

        private static ReportSetting LeaveTransactionReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("UserID", "") }
                //ProcedureParams = new List<SqlParameter> { new SqlParameter("UserID", "") }
                //ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("UserID", ""), new SqlParameter("LeaveType", "") }
            };
        }

        private static ReportSetting AttendanceReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }

        private static ReportSetting HelpdeskGraphReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("EmpPrefix", "") }
            };
        }
        private static ReportSetting RMGGraphReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }// new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }

        private static ReportSetting RMGDetailReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }// new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }
        private static ReportSetting RMGBenchLessThanReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }// new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }
        private static ReportSetting RMGBenchNewReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }// new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }

        private static ReportSetting RMGCurrentAllocationReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }// new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }

        private static ReportSetting RMGMonthAllocationReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()) }
            };
        }

        private static ReportSetting Empskillupdate(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("Empcode", "") }
            };
        }

        private static ReportSetting EmpSkillDetails(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("Empcode", ""), new SqlParameter("Reportingmngr", ""), new SqlParameter("Skillid", "") }
            };
        }

        private static ReportSetting ExitClearanceReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("EmploymentStatus", ""), new SqlParameter("EmployeeCode", ""), new SqlParameter("DeliveryUnit", ""), new SqlParameter("DeliveryTeam", ""), new SqlParameter("StartDate", ""), new SqlParameter("EndDate", "") }
            };
        }


        private static ReportSetting SisoReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }

        private static ReportSetting EmployeeDetailReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("EmploymentStatus", ""), new SqlParameter("EmployeeStatus", ""), new SqlParameter("EmployeeName", ""), new SqlParameter("DeliveryUnit", ""), new SqlParameter("DeliveryTeam", "") }
            };
        }

        private static ReportSetting LeaveLedgerReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("UserID", ""), new SqlParameter("Location", ""), new SqlParameter("DeliveryUnit", "") }
            };
        }
        private static ReportSetting LeaveDetailReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("UserID", ""), new SqlParameter("Location", ""), new SqlParameter("LeaveType", ""), new SqlParameter("StartDate", ""), new SqlParameter("EndDate", ""), new SqlParameter("LoggedUser", "") }
            };
        }
        private static ReportSetting HelpdeskReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("issuestartdate", ""), new SqlParameter("issueenddate", ""), new SqlParameter("category", ""), new SqlParameter("assigneduser", "") }
            };
        }
        private static ReportSetting HelpdeskDetails(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("HelpdeskID", "") }
            };
        }

        //private static ReportSetting CustomerbgclistReport(string name)
        //{
        //    return new ReportSetting
        //    {

        //        ReportName = name,
        //        ProcedureParams = new List<SqlParameter>{ new SqlParameter("FilterType", ""),new SqlParameter("FilterValue","")
        //         }
        //    };
        //}
        private static ReportSetting RMGObjectiveReport(string name)
        {
            return new ReportSetting
            {

                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString())
            }
            };
        }
        private static ReportSetting EmployeeBGStatusReport(string name)
        {
            return new ReportSetting
            {

                ReportName = name,
                ProcedureParams = new List<SqlParameter>{ new SqlParameter("FilterType", ""),new SqlParameter("FilterValue","")
            }
            };
        }
        //private static ReportSetting BGCompatibilityReport(string name)
        //{
        //    return new ReportSetting
        //    {

        //        ReportName = name,
        //        ProcedureParams = new List<SqlParameter> { new SqlParameter("projectID","" )
        //    }
        //    };
        //}

        private static ReportSetting Invoicing(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()) }
            };
        }

        private static ReportSetting ResourceAllocationMO(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()) }
            };
        }
        private static ReportSetting AttendanceSISOReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("FilterType", ""), new SqlParameter("Employeecode", "") }
            };
        }

        private static ReportSetting HelpdeskDetailReport(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToShortDateString()), new SqlParameter("EndDate", new DateTime(DateTime.Now.Year, DateTime.Now.Month, 22).ToShortDateString()), new SqlParameter("EmpPrefix", "") }// , new SqlParameter("Employeecode", ""), new SqlParameter("LoggedUser", "") }
            };
        }

        private static ReportSetting EmpCoordinates(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("Empcode", "") }
            };
        }

        private static ReportSetting EmpICDetails(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("Empcode", "") }
            };
        }

        private static ReportSetting VWAllocation(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("OfficeLocation", "") , new SqlParameter("DeliveryUnit", 0) }
            };
        }

        private static ReportSetting VWMonthlyAllocation(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", ""), new SqlParameter("EndDate", ""), new SqlParameter("deliveryunit", 0) }
            };
        }

        private static ReportSetting VWBench(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }
            };
        }

        private static ReportSetting VWBenchLessThan100(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { }
            };
        }

        private static ReportSetting EmpResignationDetail(string name)
        {
            return new ReportSetting
            {
                ReportName = name,
                ProcedureParams = new List<SqlParameter> { new SqlParameter("StartDate", ""), new SqlParameter("EndDate", ""), new SqlParameter("EmployeeCode", null), new SqlParameter("DeliveryUnitId", null), new SqlParameter("OfficeLocationId", null) }
            };
        }
    }
}
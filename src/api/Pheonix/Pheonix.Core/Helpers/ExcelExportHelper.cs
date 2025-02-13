using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Data.SqlClient;

namespace Pheonix.Helpers
{
    public static class ExcelExportHelper
    {
        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }

        public static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static byte[] ExportExcel(List<DataTable> dataTableList, List<string> headings, bool showSrNo = false)
        {

            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                for (int i = 0; i < headings.Count; i++)
                {
                    var heading = headings[i];
                    //var dataTable = dataTableList[i];
                    var workSheet = package.Workbook.Worksheets.Add(headings[i]);
                    int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 1;
                    int startRowFrom1 = String.IsNullOrEmpty(heading) ? 1 : 2;

                    if (headings[i] == "Dashboard")
                    {
                        if (dataTableList[0] != null)
                        {
                            workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTableList[0], true);
                        }
                        if (dataTableList[1] != null)
                        {
                            workSheet.Cells["F" + startRowFrom].LoadFromDataTable(dataTableList[1], true);
                        }
                        if (dataTableList[2] != null)
                        {
                            workSheet.Cells["J" + startRowFrom].LoadFromDataTable(dataTableList[2], true);
                        }
                    }
                    if (headings[i] == "RawData")
                    {
                        if (dataTableList[3] != null)
                        {
                            workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTableList[3], true);
                        }
                    }

                    //if (!String.IsNullOrEmpty(heading))
                    //{
                    //    workSheet.Cells["F1"].Value = heading;
                    //    workSheet.Cells["F1"].Style.Font.Size = 20;
                    //    workSheet.InsertColumn(1, 1);
                    //    workSheet.InsertRow(1, 1);
                    //    workSheet.Column(1).Width = 5;
                    //}
                    

                }
                result = package.GetAsByteArray();
            }

            return result;
        }
    }
}


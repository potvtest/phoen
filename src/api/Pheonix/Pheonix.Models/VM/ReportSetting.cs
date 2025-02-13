using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Pheonix.Models.VM
{
    public class ReportSetting
    {
        public string ReportName { get; set; }
        public List<SqlParameter> ProcedureParams { get; set; }
        public Func<SqlDataReader, List<Dictionary<string, object>>> Transformer { get; set; }
    }
}
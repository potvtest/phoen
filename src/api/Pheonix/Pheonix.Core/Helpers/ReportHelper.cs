using Newtonsoft.Json.Linq;
using Pheonix.Core.v1.Builders;
using Pheonix.DBContext.DataAccess;
using Pheonix.Models.VM;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pheonix.Core.Helpers
{
    public class ReportHelper
    {
        public List<Dictionary<string, object>> Run(string reportName, Dictionary<string, object> paramameters)
        {
            ReportSetting setting = ReportBuilder.GetReportProcedureSettings(reportName);

            /* Update SQL Params with latest Value comign in DATA this jas to be a valid JObject */

            foreach (var item in paramameters)
            {
                if (setting.ProcedureParams.Where(t => t.ParameterName == item.Key).Count() > 0)
                {
                    if (item.Value is JArray)
                    {
                        string[] arr = ((IEnumerable)item.Value).Cast<object>()
                                      .Select(x => x.ToString())
                                      .ToArray();
                        var result = string.Join(",", arr);
                        setting.ProcedureParams.Where(t => t.ParameterName == item.Key).FirstOrDefault().Value = result;
                    }
                    else
                    {
                        setting.ProcedureParams.Where(t => t.ParameterName == item.Key).FirstOrDefault().Value = item.Value;
                    }
                }

            }

            return (new ExecuteProcedure("PhoenixSqlEntities")).ExecuteDataReader(setting.ProcedureParams, setting.ReportName, setting.Transformer);
        }
    }
}
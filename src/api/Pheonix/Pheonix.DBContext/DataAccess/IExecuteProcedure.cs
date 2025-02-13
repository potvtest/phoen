using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Pheonix.DBContext.DataAccess
{
    internal interface IExecuteProcedure
    {
        T ExecuteDataReader<T>(List<SqlParameter> paramList, string procName, Func<SqlDataReader, T> output);

        string Name { get; set; }
    }
}
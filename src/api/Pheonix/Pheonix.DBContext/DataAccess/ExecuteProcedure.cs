using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Pheonix.DBContext.DataAccess
{
    public class ExecuteProcedure : IExecuteProcedure
    {
        private SqlConnection _con = null;
        private string connectionString = string.Empty;
        private string _proc = string.Empty;

        public ExecuteProcedure(string conectionStringName)
        {
            connectionString = ConfigurationManager.ConnectionStrings[conectionStringName].ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("Provided Connection string is not present in Connection String Section of Config");
        }

        public ExecuteProcedure()
        {
        }

        public string Name
        {
            get { return _proc; }
            set { _proc = value; }
        }

        public T ExecuteDataSet<T>(List<SqlParameter> paramList, string procName, CommandType commandType, Func<SqlDataAdapter, T> output)
        {
            Name = procName;
            SqlDataAdapter adapt = new SqlDataAdapter();

            using (_con = new SqlConnection(connectionString))
            {
                if (_con.State != ConnectionState.Open)
                    _con.Open();

                using (SqlCommand command = new SqlCommand(procName, _con))
                {
                    command.CommandType = commandType;

                    if (paramList != null)
                    {
                        foreach (var param in paramList)
                            command.Parameters.Add(param);
                    }

                    adapt.SelectCommand = command;

                    return output(adapt);
                }
            }
        }

        public T ExecuteDataReader<T>(List<SqlParameter> paramList, string procName, Func<SqlDataReader, T> output)
        {
            Name = procName;
            
            using (_con = new SqlConnection(connectionString))
            {
                if (_con.State != ConnectionState.Open)
                    _con.Open();

                using (SqlCommand command = new SqlCommand(procName, _con))
                {
                    command.CommandTimeout = 300;
                    command.CommandType = CommandType.StoredProcedure;

                    if (paramList != null)
                    {
                        foreach (var param in paramList)
                            command.Parameters.Add(param);
                    }
                    return output(command.ExecuteReader());
                }
            }
        }

        public int ExecuteNonQuery(List<SqlParameter> paramList, string procName)
        {
            Name = procName;
            using (_con = new SqlConnection(connectionString))
            {
                if (_con.State != ConnectionState.Open)
                    _con.Open();

                using (SqlCommand command = new SqlCommand(procName, _con))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (paramList != null)
                    {
                        foreach (var param in paramList)
                            command.Parameters.Add(param);
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(List<SqlParameter> paramList, string queryString, CommandType commandType)
        {
            using (_con = new SqlConnection(connectionString))
            {
                if (_con.State != ConnectionState.Open)
                    _con.Open();

                using (SqlCommand command = new SqlCommand(queryString, _con))
                {
                    command.CommandType = commandType;  //CommandType.StoredProcedure;

                    if (paramList != null)
                    {
                        foreach (var param in paramList)
                            command.Parameters.Add(param);
                    }

                    return command.ExecuteScalar();
                }
            }
        }

        public async Task<int> ExecuteNonQueryAsync(List<SqlParameter> paramList, string procName)
        {
            Name = procName;
            using (_con = new SqlConnection(connectionString))
            {
                if (_con.State != ConnectionState.Open)
                    _con.Open();

                using (SqlCommand command = new SqlCommand(procName, _con))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (paramList != null)
                    {
                        foreach (var param in paramList)
                            command.Parameters.Add(param);
                    }

                    return await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
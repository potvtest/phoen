using Pheonix.Core.v1.Services;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using Pheonix.DBContext;
using System.Data.SqlClient;

namespace Pheonix.Core.v1.Services.Approval
{
    public class ApprovalStatusFactory
    {
        public static Dictionary<string, List<DashboardApprovalStatus>> ExecuteAllFactories(IBasicOperationsService service, int userId)
        {
            var db = new PhoenixEntities();
            var dashboardApprovals = new Dictionary<string, List<DashboardApprovalStatus>>();

            using (var connection = db.Database.Connection)
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "EXEC [dbo].[GetStatusCount] @UserId";
                command.Parameters.Add(new SqlParameter("@UserId", userId));

                using (var reader = command.ExecuteReader())
                {
                    // Process Helpdesk data
                    var helpDesk = new List<DashboardApprovalStatus>();
                    while (reader.Read())
                    {
                        helpDesk.Add(new DashboardApprovalStatus
                        {
                            Status = Convert.ToInt32(reader["Status"]),
                            Count = Convert.ToInt32(reader["Count"]),
                            Url = Convert.ToString(reader["Url"])
                        });
                    }
                    dashboardApprovals.Add("helpDesk", helpDesk);

                    reader.NextResult();

                    // Process Travel data
                    var travel = new List<DashboardApprovalStatus>();
                    while (reader.Read())
                    {
                        travel.Add(new DashboardApprovalStatus
                        {
                            Status = Convert.ToInt32(reader["Status"]),
                            Count = Convert.ToInt32(reader["Count"]),
                            Url = Convert.ToString(reader["Url"])
                        });
                    }
                    dashboardApprovals.Add("travel", travel);

                    reader.NextResult();

                    // Process SeparationApproval data
                    var separationApproval = new List<DashboardApprovalStatus>();
                    while (reader.Read())
                    {
                        separationApproval.Add(new DashboardApprovalStatus
                        {
                            Status = Convert.ToInt32(reader["Status"]),
                            Count = Convert.ToInt32(reader["Count"]),
                            Url = Convert.ToString(reader["Url"])
                        });
                    }
                    dashboardApprovals.Add("separationApproval", separationApproval);

                    reader.NextResult();

                    // Process Expense data
                    var expense = new List<DashboardApprovalStatus>();
                    while (reader.Read())
                    {
                        expense.Add(new DashboardApprovalStatus
                        {
                            Status = Convert.ToInt32(reader["Status"]),
                            Count = Convert.ToInt32(reader["Count"]),
                            Url = Convert.ToString(reader["Url"])
                        });
                    }
                    dashboardApprovals.Add("expense", expense);
                }
            }
            return dashboardApprovals;
        }
    }
}
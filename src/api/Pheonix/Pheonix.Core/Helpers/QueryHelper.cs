using Pheonix.DBContext;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Pheonix.Core.Helpers
{
    public class QueryHelper
    {
        public static List<int> GetApprovalsForUser(int userId, int requestType)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
               return (from d in context.ApprovalDetail
                             join a in context.Approval
                             on d.ApprovalID equals a.ID
                             where d.ApproverID == userId && a.RequestType == requestType 
                                   && (a.Status == null || a.Status == 0)
                             select a.RequestID ?? -1).ToList();
            }
        }

        public static List<int> GetApprovalsForUser2(int userId, int requestType)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalDetail1 = context.ApprovalDetail.Where(s => s.ApproverID == userId
                    && (s.Status == 0 || s.Status == 3)
                    && s.Approval.RequestType == requestType
                    && s.Approval.ApprovalDetail.Where(t => t.Stage < s.Stage && (t.Status == 0 || t.Status == 3)).Count() == 0
                    ).Select(c => c.Approval.RequestID ?? 0).ToList();

                    return approvalDetail1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<int> GetApprovalsForExecutiveRole(int userId, int requestType, int logedInUserId)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalDetail1 = context.ApprovalDetail.Where(s => s.ApproverID == userId
                    && (s.Status == 0 || (s.Status == 1 && s.Approval.RequestBy == logedInUserId))
                    && s.Approval.RequestType == requestType
                    && s.Approval.ApprovalDetail.Where(t => t.Stage < s.Stage && (t.Status == 0 || t.Status == 3)).Count() == 0
                    ).Select(t => t.Approval.RequestID ?? 0).ToList();

                    return approvalDetail1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<int> GetApprovalsForConfirmationRole(int requestType)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalDetail = context.ApprovalDetail.Where(s =>
                    s.Status == 1 && s.Stage == 1 && s.Approval.RequestType == requestType
                    ).Select(t => t.Approval.RequestID ?? 0).ToList();

                    return approvalDetail;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<int> GetApprovalsForUser1(int userId, int requestType)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetApprovalsFor(userId, requestType).Select(x => x.RequestID.Value).ToList();
            }
        }

        public static int GetTotalStages(int expenseId, int? personId)   // not in use commented wherever used.
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var data = context.GetTotalStages(expenseId, personId).FirstOrDefault();
                return data != null ? data.TotalStages.Value : 0;
            }
        }

        public static List<GetMySubmittedExpenses_Result> GetMySubmittedExpenses(int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetMySubmittedExpenses(userId).ToList();
            }
        }

        public static GetReportingManager_Result GetManger(int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetReportingManager(id).FirstOrDefault();
            }
        }
        
        //public static List<GetTimesheet_Result> GetTimesheetResult(PhoenixEntities entites, int id, string startdate, string enddate)
        public static List<GetTimesheet_Result> GetTimesheetResult( int? id, string startdate, string enddate)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetTimesheet(id, startdate, enddate).ToList();
            }
        }
        public static List<GetTimesheetByProjectID_Result> GetTimesheetByProjectID(int? id, int? managerId,DateTime startdate, DateTime enddate)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetTimesheetByProjectID(id, managerId, startdate, enddate).ToList();
            }
        }

        public static GetReportingManager_Result GetDesingatedManger(int id, List<int> managerDesignations)
        {

            using (PhoenixEntities context = new PhoenixEntities())
            {
                var manager = GetManger(id);
                var managerEmployment = context.PersonEmployment.FirstOrDefault(t => t.PersonID == manager.ID);
                if (!managerDesignations.Contains(managerEmployment.DesignationID ?? 0))
                {
                    return GetDesingatedManger( manager.ID, managerDesignations);
                }
                return manager;
            }
        }


        public static GetExitProcessManager_Result GetExitProcessManager(int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var manager = context.GetExitProcessManager(id).FirstOrDefault();
                return manager;
            }
        }

        public static List<GetExpenseToApprove_Result> GetApprovalsToApprove(int personId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetExpenseToApprove(personId).ToList();
            }
        }

        public static List<string> GetFinanceEmail( int roldId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetFinanceEmailId(roldId).ToList();
            }
        }

        public static List<GetApprovedExpenses_Result> GetApprovedExpenses(int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetApprovedExpenses(userId).ToList();
            }
        }

        public static List<GetExpenseApprovers_Result> GetExpenseApprovers(int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetExpenseApprovers(userId).ToList();
            }
        }

        public static List<GetLatestActivity_Result> GetLatestActivity(int personId, int requestType)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetLatestActivity(personId, requestType).ToList();
            }
        }

        public static int changeRepotingManger(int peronId, int repotingId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.ReportingChange(peronId, repotingId);
            }
        }

        public static int changeExitManager(int personId, int oldExitManagerId, int newExitManagerId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.ChangeExitManager(personId, oldExitManagerId, newExitManagerId);
            }
        }

        public static List<GetTravelToApprove_Result> GetTravelRequestsToApprove(int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetTravelToApprove(id).ToList();
            }
        }

        public static List<int> GetApprovalForTravel(int requestType, int roleId, int approverId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetApprovalForTravel(requestType, roleId, approverId).Select(x => x.RequestID.Value).ToList();
            }
        }

        public static List<GetMySubmittedTravel_Result> GetMySubmittedTravel( int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetMySubmittedTravel(id).ToList();
            }
        }

        public static List<GetApprovedTravelRequests_Result> GetApprovedTravelRequests(int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetApprovedTravelRequests(id).ToList();
            }
        }

        public static List<GetAppraisalSummary_Result> GetAppraisalSummary(int? deliveryUnit, int? delivertTeam, int? rating, int? year)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetAppraisalSummary(rating, delivertTeam, deliveryUnit, year).ToList();
            }
        }

        public static int ResetApproval(int requestId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                context.DeleteApproval(requestId);
                return context.SaveChanges();
            }
        }

        /// <summary>
        /// Get no of days Separation release process should starts.
        /// </summary>
        /// <returns></returns>
        public static T GetConfigKeyValue<T>(string keyName)
        {
            return (T)Convert.ChangeType(ConfigurationManager.AppSettings[keyName], typeof(T));
        }

        public void CreditCompOffAfterWithdraw(DateTime ResignDate, int personID)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                context.CreditCompOffAfterWithdraw(ResignDate, personID);
            }
        }

        public static List<int> GetMyApprovedSeparationDetl(int userId, int requestType)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalDetail1 = context.ApprovalDetail.Where(s => s.ApproverID == userId
                    && (s.Status == 1 || s.Status == 0)
                    && s.Approval.RequestType == requestType
                    && s.Approval.ApprovalDetail.Where(t => t.Stage < s.Stage && (t.Status == 0 || t.Status == 1)).Count() == 0);

                    return approvalDetail1.Select(t => t.Approval.RequestID ?? 0).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<rpt_AppraisalReport_Result> GetAppraisalReport(string location, int? status, int? grade, int? empID, int? year)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.rpt_AppraisalReport(status, grade, location, empID,year).ToList();
            }
        }

        public static List<rpt_PendingAppraisalStatus_Result> GetPendingAppraisalStatus()
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.rpt_PendingAppraisalStatus().ToList();
            }
        }

        public static int BulkSISO(DateTime fromDate, DateTime toDate, int userid, string comments, int loggedInUser)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                int val = context.bulksiso(fromDate, toDate, userid, comments, loggedInUser);
                return val;
            }
        }

        public static bool IsFinanceApprover(int userID)
        {
            bool flag = false;
            using (PhoenixEntities context = new PhoenixEntities())
            {
                List<int> personInRole = context.PersonInRole.Where(role => role.RoleID == 21 || role.RoleID == 23 || role.RoleID == 33).
                                           Select(role => role.PersonID).ToList();
                if (personInRole.Contains(userID))
                    flag = true;
                return flag;
            }
        }

        public static bool IsRecruiter(int userID)
        {
            bool flag = false;
            using (PhoenixEntities context = new PhoenixEntities())
            {
                List<int> personInRole = context.PersonInRole.Where(role => role.RoleID == 46 || role.RoleID == 48).
                                           Select(role => role.PersonID).ToList();
                if (personInRole.Contains(userID))
                    flag = true;
                return flag;
            }
        }

        public static bool IsInvoiceApprover(int userID)
        {
            bool flag = false;
            using (PhoenixEntities context = new PhoenixEntities())
            {
                List<int> personInRole = context.PMSConfiguration.Where(config => config.Role == 6).// 6 defines group head
                                           Select(config => config.PersonID.Value).ToList();
                if (personInRole.Contains(userID))
                    flag = true;
                return flag;
            }
        }

        public static List<GetCelebrationList_Result> GetCelebrationList()
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetCelebrationList().ToList();
            }
        }

        public static List<GetAllReportsByPersonId_Result> GetAllReports(int personId, int parentReportId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetAllReportsByPersonId(personId, parentReportId).ToList();
            }
        }

        public static List<GetProjectAssignedWithOtherList_Result> GetProjectAssignedWithOtherList(int personId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetProjectAssignedWithOtherList(personId).ToList();
            }
        }

        public static List<GetTimesheetList_Result> GetTimesheetList(int personId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
                return context.GetTimesheetList(personId, null, null).ToList();
        }

        public static List<GetLedgerReport_Result> GetLedgerReport(DateTime reportStartDate, DateTime reportEndDate, int projectId, int? personId, int? subProjectId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetLedgerReport(reportStartDate, reportEndDate, projectId, personId, subProjectId).ToList();
            }
        }

        public static List<GetSummaryReport_Result> GetSummaryReport(DateTime reportStartDate, DateTime reportEndDate, int personId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                return context.GetSummaryReport(reportStartDate, reportEndDate, personId).ToList();
            }
        }

        public static GetGroupHeadEmail_Result GetGroupHeadEmail(int personId,int grade)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    return context.GetGroupHeadEmail(personId, grade).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int InsertEmail(string template, string emailFrom, string strDistinctEmailTo, string strDistinctEmailCC, string subject)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    return context.InsertEmail(template, emailFrom, strDistinctEmailTo, strDistinctEmailCC, subject);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
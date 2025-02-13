using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pheonix.Core.v1.Services.Approval
{
    public class ApprovalStrategyFactory
    {
        public static IApproverStrategy GetStrategy(ApprovalStrategy strategyType, int currentUserId = 0, int approverId = 0, int roleType = 0, Func<int> lastSgateApprover = null)
        {
            IApproverStrategy strategy;

            switch (strategyType)
            {
                case ApprovalStrategy.HrOnly:
                    strategy = new HROnlyApproverStrategy();
                    break;

                case ApprovalStrategy.OneLevelOnly:
                    strategy = new OneLevelUpApprovalStrategy(currentUserId);
                    break;

                case ApprovalStrategy.MultiLevel:
                    strategy = new MultiLevelApprovalStrategy(2, currentUserId, approverId, roleType, lastSgateApprover);
                    break;

                default:
                    throw new ArgumentNullException("Wrong strategy selected");
            }
            return strategy;
        }
    }

    public enum ApprovalStrategy
    {
        HrOnly,
        OneLevelOnly,
        MultiLevel
    }

    public interface IApproverStrategy
    {
        int[] FetchApprovers();

        IBasicOperationsService opsService { get; set; }
    }

    public class HROnlyApproverStrategy : IApproverStrategy
    {
        public int[] FetchApprovers()
        {
            // Using this strategy it doesnot matter what the input values are... it will only pick the first User mapped to HR ROle.

            var hrID = opsService.Top<PersonInRole>(10, t => t.RoleID == 12).First().PersonID;

            return new int[] { hrID };
        }

        public IBasicOperationsService opsService { get; set; }
    }

    public class OneLevelUpApprovalStrategy : IApproverStrategy
    {
        private int _currentUserId;

        public OneLevelUpApprovalStrategy(int currentUserId)
        {
            _currentUserId = currentUserId;
        }

        public int[] FetchApprovers()
        {
            //In this count will alway remain 1
            var reportingManagerID = opsService.Top<PersonReporting>(10, t => t.ReportingTo != null && t.PersonID == _currentUserId).First().ReportingTo;
            return new int[] { reportingManagerID };
        }

        public IBasicOperationsService opsService { get; set; }
    }

    public class MultiLevelApprovalStrategy : IApproverStrategy
    {
        private int _count;
        private int _currentUserId;
        private Func<int> _getLastStageApprover;
        private int _approverId;
        private int _roleType;

        public MultiLevelApprovalStrategy(int count, int currentUserId, int approverId, int roleType = 0, Func<int> getLastStageApprover = null)
        {
            _count = count;
            _currentUserId = currentUserId;
            _getLastStageApprover = getLastStageApprover;
            _approverId = approverId;
            _roleType = roleType;
        }

        public int[] FetchApprovers()
        {
            //In this count will alway remain 1
            List<int> approvers = new List<int>();
            int reportingManager = opsService.Top<PersonInRole>(10, t => t.RoleID == _roleType).First().PersonID;
            approvers.Insert(approvers.Count(), _approverId);

            if (reportingManager != _approverId)
            {
                if (_getLastStageApprover == null)
                {
                    // ND: We have to get the Finance role from DB. For now I have added it into the DB, but will be needing to modify it.
                    // For expense it is Finance so added 21, but it will not always be Finance.
                    approvers.Insert(approvers.Count(), reportingManager);
                }
                else
                {
                    approvers.Insert(approvers.Count(), _getLastStageApprover());
                }
            }
            return approvers.ToArray();
        }

        public IBasicOperationsService opsService { get; set; }
    }
}

public enum RoleType
{
    Expense = 21,
    Travel = 33
}
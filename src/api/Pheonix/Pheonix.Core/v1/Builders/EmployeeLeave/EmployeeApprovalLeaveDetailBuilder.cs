using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pheonix.Core.v1.Builders
{
    public class EmployeeApprovalLeaveDetailBuilder<T> : EmployeeLeaveBuilder<T>
    {
        private IBasicOperationsService _service;

        public EmployeeApprovalLeaveDetailBuilder(IBasicOperationsService service)
        {
            _service = service;
        }

        protected override IEnumerable<T> BuildViewModel(int userID, DateTime? fromDate, DateTime? toDate)
        {
                IEnumerable<PersonLeave> e;
                var approvalIds = QueryHelper.GetApprovalsForUser(userID, 1);
                e = _service.Top<PersonLeave>(0, a => approvalIds.Contains(a.ID) && a.Status == 1).OrderByDescending(t=>t.RequestDate);
                var empLeaves = Mapper.Map<IEnumerable<PersonLeave>, IEnumerable<T>>(e);
                return empLeaves;
        }
    }
}
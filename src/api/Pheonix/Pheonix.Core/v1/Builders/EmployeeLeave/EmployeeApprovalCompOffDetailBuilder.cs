using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pheonix.Core.v1.Builders
{
    public class EmployeeApprovalCompOffDetailBuilder<T> : EmployeeLeaveBuilder<T>
    {
        private IBasicOperationsService _service;

        public EmployeeApprovalCompOffDetailBuilder(IBasicOperationsService service)
        {
            _service = service;
        }

        protected override IEnumerable<T> BuildViewModel(int userID, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<CompOff> e;
            var approvalIds = QueryHelper.GetApprovalsForUser(userID, 4);
            e = _service.Top<CompOff>(0, a => approvalIds.Contains(a.ID)).OrderByDescending(t =>t.ForDate);
            var empLeaves = Mapper.Map<IEnumerable<CompOff>, IEnumerable<T>>(e);
            return empLeaves;
        }
    }
}
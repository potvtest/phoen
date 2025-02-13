using AutoMapper;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pheonix.Core.v1.Builders
{
    public class EmployeeLeaveDetailBuilder<T> : EmployeeLeaveBuilder<T>
    {
        private IBasicOperationsService _service;

        public EmployeeLeaveDetailBuilder(IBasicOperationsService service)
        {
            _service = service;
        }

        protected override AvailableLeaves BuildAvailableLeaves(int userID, int year)
        {
            var empLeavesTaken = _service.Top<PersonLeaveLedger>(0, a => (userID == -1 || a.Person.ID == userID) && a.Year == year).ToList();
            AvailableLeaves availLeaves = new AvailableLeaves();

            using (PhoenixEntities context = new PhoenixEntities())
            {
                var leaveConsumed = context.GetLeaveData(userID, year).ToList();
                if (empLeavesTaken.Count() != 0)
                {
                    availLeaves.TotalLeaves = empLeavesTaken[0].OpeningBalance + (leaveConsumed?.First()?.CreditLeaves ?? 0);
                    availLeaves.CompOff = empLeavesTaken[0].CompOffs;

                }
                else
                {
                    availLeaves.TotalLeaves = (leaveConsumed?.First()?.CreditLeaves ?? 0);
                    availLeaves.CompOff = 0;
                }

                availLeaves.LeavesTaken = leaveConsumed?.First()?.LeavesConsumed ?? 0;
                availLeaves.LeavesApplied = leaveConsumed?.First()?.LeavesApplied ?? 0;
                availLeaves.LeavesAvailable = availLeaves.TotalLeaves - availLeaves.LeavesTaken - availLeaves.LeavesApplied;
                
                availLeaves.CompOffAvailable = leaveConsumed?.First()?.CompOffAvailable ?? 0;
                availLeaves.LWP = leaveConsumed?.First()?.LWPApplied ?? 0;
                availLeaves.CompOffConsumed = leaveConsumed?.First()?.CompOffConsumed ?? 0;

                availLeaves.CLCredited = leaveConsumed?.First()?.CLCredited ?? 0;
                availLeaves.CLDebited = leaveConsumed?.First()?.CLDebited ?? 0;
                availLeaves.CLUtilized = leaveConsumed?.First()?.CLUtilized ?? 0;
                availLeaves.CLApplied = leaveConsumed?.First()?.CLApplied ?? 0;

                availLeaves.SLCredited = leaveConsumed?.First()?.SLCredited ?? 0;
                availLeaves.SLDebited = leaveConsumed?.First()?.SLDebited ?? 0;
                availLeaves.SLUtilized = leaveConsumed?.First()?.SLUtilized ?? 0;
                availLeaves.SLApplied = leaveConsumed?.First()?.SLApplied ?? 0;
            }

            return availLeaves;
        }

        protected override IEnumerable<EmployeeAdminHistoryData> BuildAdminLeaveViewModel(int userID, int year)
        {
            List<EmployeeAdminHistoryData> lstEmployeeLeaveViewModel = new List<EmployeeAdminHistoryData>();
            List<EmployeeAdminHistoryData> lstPLAdminHistoryData = new List<EmployeeAdminHistoryData>();
            List<EmployeeAdminHistoryData> lstCLAdminHistoryData = new List<EmployeeAdminHistoryData>();
            List<EmployeeAdminHistoryData> lstSLAdminHistoryData = new List<EmployeeAdminHistoryData>();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                lstPLAdminHistoryData = context.PersonLeaveCredit.Where(x => x.PersonID == userID && x.Year == year)
                                        .Select(x => new EmployeeAdminHistoryData
                                        {
                                            CreatedDate = x.DateEffective,
                                            Quantity = Math.Abs(x.CreditBalance),
                                            LeaveType = 1,
                                            ActionType = x.CreditBalance > 0 ? "CR" : "DR",
                                            Narration = x.Narration
                                        }).ToList();

                lstCLAdminHistoryData = context.PersonCLCredit.Where(x => x.PersonID == userID && x.Year == year)
                                       .Select(x => new EmployeeAdminHistoryData
                                       {
                                           CreatedDate = x.DateEffective,
                                           Quantity = Math.Abs(x.CreditBalance),
                                           LeaveType = 9,
                                           ActionType = x.CreditBalance > 0 ? "CR" : "DR",
                                           Narration = x.Narration
                                       }).ToList();

                lstEmployeeLeaveViewModel = lstPLAdminHistoryData.Concat(lstCLAdminHistoryData).OrderByDescending(x => x.CreatedDate).ToList();

                lstSLAdminHistoryData = context.PersonSLCredits.Where(x => x.PersonID == userID && x.Year == year)
                                       .Select(x => new EmployeeAdminHistoryData
                                       {
                                           CreatedDate = x.DateEffective,
                                           Quantity = Math.Abs(x.CreditBalance),
                                           LeaveType = 11,
                                           ActionType = x.CreditBalance > 0 ? "CR" : "DR",
                                           Narration = x.Narration
                                       }).ToList();

                lstEmployeeLeaveViewModel = lstEmployeeLeaveViewModel.Concat(lstSLAdminHistoryData).OrderByDescending(x => x.CreatedDate).ToList();

            }

            return lstEmployeeLeaveViewModel;

        }

        protected override IEnumerable<T> BuildViewModel(int userID, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<PersonLeave> e;

            e = _service.Top<PersonLeave>(0, a => (userID == -1 || a.Person.ID == userID) && ((a.FromDate >= fromDate && a.ToDate <= toDate) || (a.FromDate.Year == fromDate.Value.Year) || (a.ToDate.Year == fromDate.Value.Year)) && a.IsDeleted == false);
            e = e.OrderByDescending(t => t.FromDate); //For :#149624287 -To display leaves as per From date instead of Leave Request date 
            var empLeaves = Mapper.Map<IEnumerable<PersonLeave>, IEnumerable<T>>(e);

            /*

               Add Loginc for new entry types...

             */


            return empLeaves;
        }

        protected override IEnumerable<CompOffViewModel> BuildCompOffViewModel(int userID, int year)
        {
            var compoffs = _service.Top<CompOff>(0, x => x.Year == year && x.PersonID == userID).OrderBy(t => t.ExpiresOn);
            var mapCompOff = Mapper.Map<IEnumerable<CompOff>, IEnumerable<CompOffViewModel>>(compoffs);
            return mapCompOff;
        }
    }
}
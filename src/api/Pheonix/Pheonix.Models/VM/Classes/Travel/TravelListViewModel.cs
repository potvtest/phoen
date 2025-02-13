using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class TravelListViewModel
    {
        public int id { get; set; }
        public DateTime createdDate { get; set; }
        public int primaryApproverId { get; set; }
        public string primaryApproverName { get; set; }
        public string adminApproverName { get; set; }
        public EmployeeProfile employeeProfile { get; set; }
        public ClientInformation clientInformation { get; set; }
        public TravelDetails travelDetails { get; set; }
        public TravelStatus travelStatus { get; set; }
        public int? IsTravelExtensionHistoryExisting { get; set; }
        public class ClientInformation
        {
            public string clientName { get; set; }
        }

        public class EmployeeProfile
        {
            public int id { get; set; }
            public string firstName { get; set; }
            public string middleName { get; set; }
            public string lastName { get; set; }
            public string currentDesignation { get; set; }
            public string imagePath { get; set; }
            public string email { get; set; }
            public string mobile { get; set; }
            public string olText { get; set; }
        }

        public class TravelDetails
        {
            public string source { get; set; }
            public string destination { get; set; }
            public DateTime? journeyDate { get; set; }
            public DateTime? returnJourneyDate { get; set; }
            public int? requestType { get; set; }
            public int? tripType { get; set; }
        }

        public class TravelStatus
        {
            public int? stage1_approverID { get; set; }
            public int? stage1_stage { get; set; }
            public int? stage1_status { get; set; }
            public string stage1_statusComment { get; set; }
            public int? stage2_approverID { get; set; }
            public int? stage2_stage { get; set; }
            public int? stage2_status { get; set; }
            public string stage2_statusComment { get; set; }
        }
    }
}

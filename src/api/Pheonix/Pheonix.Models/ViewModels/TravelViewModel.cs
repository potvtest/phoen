using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Travel;
using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class TravelViewModel : ITravelModel
    {
        public int Id { get; set; }
        public string travelTitle { get; set; }
        public DateTime createdDate
        {
            get { return DateTime.Now; }

            set
            {
                value = DateTime.Now;
            }
        }
        public int primaryApproverId { get; set; }
        public string primaryApproverName { get; set; }
        public string adminApproverName { get; set; }
        public int totalStages { get; set; }
        public int financeAdmin { get; set; }
        public int travelAdmin { get; set; }

        public TravelDetailsVM travelDetails { get; set; }
        public IEnumerable<NomineeDetailsVM> nomineeDetails { get; set; }
        public ClientInformationVM clientInformation { get; set; }
        public EmployeePassport employeePassport { get; set; }
        public EmployeeBasicProfile employeeProfile { get; set; }
        public IEnumerable<StageStatus> travelStatus { get; set; }
        public IEnumerable<MoneyTransactionViewModel> moneyTransactions { get; set; }
        public IEnumerable<UploadedDocumentViewModel> uploadedDocuments { get; set; }
        public IEnumerable<EmployeeVisa> employeeVisas { get; set; }
        public IEmployeeOrganizationDetails employeeOrganizationdetails { get; set; }
        public IEnumerable<EmployeeEmergencyContact> employeeEmergencyContacts { get; set; }
        public IEnumerable<TravelHotelBooking> hotelBooking { get; set; }
        public IEnumerable<TravelFlight> flightBooking { get; set; }
        public IEnumerable<TravelExtension> travelExtension { get; set; }
    }
}
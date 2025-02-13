using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public interface ITravelModel
    {
        int Id { get; set; }
        string travelTitle { get; set; }
        DateTime createdDate
        {
            get;
            set;
        }
        int primaryApproverId { get; set; }
        int totalStages { get; set; }

        TravelDetailsVM travelDetails { get; set; }
        ClientInformationVM clientInformation { get; set; }
        IEnumerable<NomineeDetailsVM> nomineeDetails { get; set; }
        EmployeePassport employeePassport { get; set; }
        EmployeeBasicProfile employeeProfile { get; set; }
        IEnumerable<StageStatus> travelStatus { get; set; }
        IEnumerable<MoneyTransactionViewModel> moneyTransactions { get; set; }
        IEnumerable<UploadedDocumentViewModel> uploadedDocuments { get; set; }
        IEnumerable<EmployeeVisa> employeeVisas { get; set; }
        IEmployeeOrganizationDetails employeeOrganizationdetails { get; set; }
    }
}

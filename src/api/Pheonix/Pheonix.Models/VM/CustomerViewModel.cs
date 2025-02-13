using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class CustomerViewModel : IViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string CustomerCode { get; set; }
        public string ExternalMarketSegmentation { get; set; }
        public int CustomerRegion { get; set; }
        public System.DateTime ContractEffectiveDate { get; set; }
        public System.DateTime ValidTill { get; set; }
        public List<int> BgcParameter { get; set; }
        public string CreditPeriod { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public int Status { get; set; }        
    }

    public class CustomerAddressViewModel : CustomerViewModel
    {
        public int CustomerID { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public int CountryID { get; set; }
        public bool Isdeleted { get; set; }
    }

    public class CustomerContactPersonViewModel : CustomerViewModel
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> Type { get; set; }
        public string Designation { get; set; }
        public string Role { get; set; }
        public string Skype { get; set; }
        public string Email { get; set; }
        public string OfficeTelephone { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string FaxNo { get; set; }
    }

    public class CustomerContractViewModel : CustomerViewModel
    {
        public int CustomerID { get; set; }
        public string ContractName { get; set; }
        public string ContractSummary { get; set; }
        public string ContractDetails { get; set; }
        public Nullable<int> ContractType { get; set; }
        public Nullable<System.DateTime> CommencementDate { get; set; }
        public Nullable<System.DateTime> DateEffective { get; set; }
        public Nullable<System.DateTime> DateValidTill { get; set; }
        public string ContractFile { get; set; }
        public List<ContractAttachmentViewModel> ContractFiles { get; set; }
    }

    public class ContractAttachmentViewModel
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ContractFileName { get; set; }     
    }
    
}
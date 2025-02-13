namespace Pheonix.Models.VM
{
    public interface IEmployeeAddress : IBaseModel
    {
        string AddressLabel { get; set; }
        string CurrentAddress { get; set; }
        string CurrentAddressCountry { get; set; }
        bool IsCurrent { get; set; }
        string City { get; set; }
        string State { get; set; }
        string Pin { get; set; }
    }
}
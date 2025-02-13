namespace Pheonix.Models.VM
{
    public interface IEmployeeEmergencyContact : IBaseModel
    {
        string ContactPersonName { get; set; }
        int Relation { get; set; }
        string EmergencyContactNo { get; set; }
        string EmergencyEmail { get; set; }
        bool IsDeleted { get; set; }
        string ContactAddress { get; set; }
    }
}
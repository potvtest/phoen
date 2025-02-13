namespace Pheonix.Models.VM
{
    public interface IEmployeeMedicalHistory : IBaseModel
    {
        string Description { get; set; }
        int Year { get; set; }
        bool IsDeleted { get; set; }
    }
}
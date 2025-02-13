namespace Pheonix.Models.VM
{
    public interface IEmployeeQualification : IBaseModel
    {
        string University { get; set; }
        int QualificationID { get; set; }
        string QualificationName { get; set; }
        string Specialization { get; set; }
        int? PassingYear { get; set; }
        string Institute { get; set; }
        string QualificationType { get; set; }
        string Grade_Class { get; set; }
        int? StatusId { get; set; }
        bool IsDeleted { get; set; }
    }
}
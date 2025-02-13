using System;

namespace Pheonix.Models.ViewModels
{
    public class EmployeePersonalDetailViewModel
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string MaritalStatus { get; set; }
        public DateTime WeddingDate { get; set; }
        public string SpouseName { get; set; }
        public DateTime SpouseBirthDate { get; set; }
        public int NumberOfChildren { get; set; }
        public string Hobbies { get; set; }
    }
}
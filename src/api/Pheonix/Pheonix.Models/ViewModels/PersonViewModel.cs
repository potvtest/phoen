using Newtonsoft.Json;
using Pheonix.Models.ViewModels;
using System;

namespace Pheonix.Models
{
    public class PersonViewModel:IViewModel
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string Salutation { get; set; }

        [JsonProperty("a")]
        public bool Active { get; set; }

        public PersonEmploymentViewModel PersonEmployment { get; set; }

        public int? EmploymentStatus { get; set; }
    }
}
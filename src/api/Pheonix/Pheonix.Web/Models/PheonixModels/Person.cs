using System;

namespace Pheonix.Web.Models
{
    public class Person
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string Salutation { get; set; }

        public bool Active { get; set; }

        public PersonEmployment PersonEmployment { get; set; }
    }
}
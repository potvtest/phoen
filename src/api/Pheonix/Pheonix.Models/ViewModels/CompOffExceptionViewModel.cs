using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class CompOffExceptionViewModel
    {
        public int ID { get; set; }
        public int PersonID { get; set; }
    }

    public class ActiveEmpViewModel
    {
       public List<PersonViewModel> personviewmodel { get; set; }
       public List<ExceptedEmpViewModel> exceptedEmpViewModel { get; set; }
    }

    public class ExceptedEmpViewModel : IViewModel
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string Salutation { get; set; }
    }
}

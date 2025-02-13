using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Report
{
    public class ReportTerminationDataModel
    {
        public int ID { get; set; }
        public string Salutation { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }
        public Nullable<int> PersonID { get; set; }
        public string Address { get; set; }

        public void Calculate(IContextRepository repository)
        {
            var employment = repository.GetAll<PersonEmployment>();
            var designation = repository.GetAll<Designation>();
            var emp = employment.FirstOrDefault(t => t.PersonID == PersonID);
            if (emp != null && emp.Person != null)
            {
                var address = emp.Person.PersonAddresses.ToList();
                if (address.Count > 0)
                {
                    var permanentadd = address.FirstOrDefault(x => x.IsCurrent == false);
                    if (permanentadd != null)
                    {
                        Address = permanentadd.Address;
                        City = permanentadd.City;
                        State = permanentadd.State;
                        Country = permanentadd.Country;
                        Pin = permanentadd.Pin;
                    }
                    else
                    {
                        var currentadd = address.FirstOrDefault(x => x.IsCurrent == true);
                        Address = currentadd.Address;
                        City = currentadd.City;
                        State = currentadd.State;
                        Country = currentadd.Country;
                        Pin = currentadd.Pin;
                    }
                }
                //var designationDesc = designation.FirstOrDefault(x => x.ID == emp.DesignationID).Description;
                //if (designationDesc != null)
                //    CurrentDesignation = designationDesc;
            }
        }
    }
}

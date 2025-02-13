using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Report
{
    public class ReportConfirmationDataModel
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime ReviewDate { get; set; }
        public Nullable<int> ConfirmationStatus { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime ProbationReviewDate { get; set; }
        public DateTime ExtFrom { get; set; }
        public DateTime ExtTo { get; set; }
        public int ExtNoOfdays { get; set; }
        public DateTime InitiatedDate { get; set; }
        public Nullable<int> ExtendedTill { get; set; }
        public Nullable<int> PIPTill { get; set; }
        public Nullable<int> ReportingManagerId { get; set; }
        public string ReportingManager { get; set; }
        public string TrainingFeedback { get; set; }
        public string BehaviourFeedback { get; set; }
        public string OverallFeedback { get; set; }
        public Nullable<int> PersonId { get; set; }
        public string Salutation { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }

        public void Calculate(IContextRepository repository)
        {
            try
            {
                var employment = repository.GetAll<PersonEmployment>();
                var emp = employment.FirstOrDefault(t => t.PersonID == PersonId);
                if (emp != null && emp.Person != null)
                {
                    var address = emp.Person.PersonAddresses.ToList();
                    if(address.Count > 0)
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

                    emp.ProbationReviewDate = emp.ProbationReviewDate;

                    switch (ConfirmationStatus)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            ExtFrom = ProbationReviewDate.AddDays((ExtendedTill ?? 0) * -1);
                            ExtFrom = ExtFrom.AddDays(1);
                            ExtTo = ProbationReviewDate;
                            break;
                        case 4:
                            ExtFrom = ProbationReviewDate.AddDays((PIPTill ?? 0) * -1);
                            ExtTo = ProbationReviewDate;
                            break;
                    }

                    ExtNoOfdays = ExtTo.Subtract(ExtFrom).Days+1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

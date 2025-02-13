using System;
using System.Collections.Generic;

namespace Pheonix.Models.ViewModels
{
    public class PersonKRADetailViewModel
    {
        public int Id { get; set; }
        public int KRACategoryId { get; set; }
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string KRA { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public int YearId { get; set; }
        public string Description { get; set; }
        public int Weightage { get; set; }
        public bool IsManagerEdit { get; set; }
        public bool IsEmployeeEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCloned { get; set; }
        public bool IsKRADone { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime ModifiedOn { get; set; }
        public System.DateTime? KRADoneOn { get; set; }
        public int KRAInitiationId { get; set; }
        public bool IsValid { get; set; }
        public string Comments { get; set; }
        public int KRAPercentageCompletion { get; set; }
        public System.DateTime KRAStartDate { get; set; }
        public System.DateTime KRAEndDate { get; set; }
        public bool IsKRAQuarterCompleted { get; set; }
        public bool IsKRAIntitiationCompleted { get; set; }
        public System.DateTime? KRAInitiationEndDate { get; set; }
        public bool IsKRAAvailableForClone { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int ParentKRAId { get; set; }
        public int KRAClonedBy { get; set; }
        public System.DateTime KRAClonedDate { get; set; }
        public int KRAHistoryClonedBy { get; set; }
        public System.DateTime? KRAHistoryClonedDate { get; set; }
        public int KRAHistoryId { get; set; }
        public bool IsClonedFromHistory { get; set; }
        public int KRAGoalId { get; set; }

        public List<KRADetail> KRADetails { get; set; }
    }

    public class KRADetail
    {
        public int Id { get; set; }
        public int KRAInitiationId { get; set; }
        public int ParentKRAId { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public int YearId { get; set; }
    }
}
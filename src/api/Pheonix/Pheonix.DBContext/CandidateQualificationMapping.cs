//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pheonix.DBContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class CandidateQualificationMapping
    {
        public int ID { get; set; }
        public int CandidateID { get; set; }
        public Nullable<int> QualificationID { get; set; }
        public string Specialization { get; set; }
        public string Institute { get; set; }
        public string University { get; set; }
        public Nullable<int> Year { get; set; }
        public string QualificationType { get; set; }
        public string Percentage { get; set; }
        public string Grade_Class { get; set; }
        public Nullable<int> StatusId { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual Candidate Candidate { get; set; }
        public virtual Qualification Qualification { get; set; }
    }
}

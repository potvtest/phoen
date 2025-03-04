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
    
    public partial class TARRF
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TARRF()
        {
            this.TAInterviewer = new HashSet<TAInterviewer>();
            this.TASkills = new HashSet<TASkills>();
        }
    
        public int Id { get; set; }
        public Nullable<int> RRFNo { get; set; }
        public bool IsDraft { get; set; }
        public Nullable<System.DateTime> RequestDate { get; set; }
        public Nullable<int> DeliveryUnit { get; set; }
        public Nullable<int> Designation { get; set; }
        public Nullable<int> Position { get; set; }
        public Nullable<int> EmploymentType { get; set; }
        public Nullable<int> MinYrs { get; set; }
        public Nullable<int> MaxYrs { get; set; }
        public Nullable<int> PrimaryApprover { get; set; }
        public Nullable<int> JD { get; set; }
        public string RequestorComments { get; set; }
        public string PrimaryApproverComments { get; set; }
        public int Requestor { get; set; }
        public Nullable<int> HRApprover { get; set; }
        public Nullable<int> SLA { get; set; }
        public string HRApproverComments { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> RRFStatus { get; set; }
        public Nullable<System.DateTime> ExpectedClosureDate { get; set; }
        public string OtherSkills { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAInterviewer> TAInterviewer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TASkills> TASkills { get; set; }
    }
}

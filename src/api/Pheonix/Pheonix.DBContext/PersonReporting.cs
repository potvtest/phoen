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
    
    public partial class PersonReporting
    {
        public int ID { get; set; }
        public int PersonID { get; set; }
        public int ReportingTo { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}

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
    
    public partial class PMSActionMapping
    {
        public int Id { get; set; }
        public int PMSRoleMapID { get; set; }
        public int ActionID { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual PMSAction PMSAction { get; set; }
        public virtual PMSRoleMapping PMSRoleMapping { get; set; }
    }
}

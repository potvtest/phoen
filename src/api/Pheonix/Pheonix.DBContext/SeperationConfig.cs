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
    
    public partial class SeperationConfig
    {
        public int ID { get; set; }
        public string ChecklistItem { get; set; }
        public int RoleID { get; set; }
        public bool IsActive { get; set; }
        public Nullable<bool> ChecklistType { get; set; }
        public Nullable<int> Sequence { get; set; }
    }
}

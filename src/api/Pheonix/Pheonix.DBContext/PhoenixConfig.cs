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
    
    public partial class PhoenixConfig
    {
        public int ID { get; set; }
        public string ConfigKey { get; set; }
        public string Description { get; set; }
        public string ConfigValue { get; set; }
        public string AppliedTo { get; set; }
        public Nullable<bool> Active { get; set; }
        public bool IsDeleted { get; set; }
        public int Year { get; set; }
        public Nullable<int> Location { get; set; }
    }
}

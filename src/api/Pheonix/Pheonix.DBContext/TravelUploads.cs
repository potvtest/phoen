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
    
    public partial class TravelUploads
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int TravelId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string FileName { get; set; }
    
        public virtual Traveller Traveller { get; set; }
    }
}

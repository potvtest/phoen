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
    
    public partial class vwCurrentAllocation
    {
        public int Employee_Code { get; set; }
        public string Employee_Name { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> Joining_Date { get; set; }
        public Nullable<System.DateTime> Exit_Date { get; set; }
        public string Employment_Status { get; set; }
        public string Work_Location { get; set; }
        public string Office_Location { get; set; }
        public string Designation { get; set; }
        public string Project_Delivery_Unit { get; set; }
        public string Project_Delivery_Team { get; set; }
        public string Resource_Pool { get; set; }
        public string Project_Reporting_Manager { get; set; }
        public Nullable<int> ProjectID { get; set; }
        public string Project_Name { get; set; }
        public string Project_Role { get; set; }
        public string Resource_Status { get; set; }
        public System.DateTime Start_Date { get; set; }
        public System.DateTime End_Date { get; set; }
        public Nullable<System.DateTime> Release_Date { get; set; }
        public int Resource_Percentage_Loading__ { get; set; }
        public Nullable<decimal> Resource_Utilization { get; set; }
        public Nullable<int> duid { get; set; }
        public Nullable<int> dtid { get; set; }
        public Nullable<int> Grade { get; set; }
        public string Competency { get; set; }
        public string SkillDescription { get; set; }
    }
}

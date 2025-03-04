﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

public partial class Person
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Person()
    {
        this.PersonEmployments = new HashSet<PersonEmployment>();
    }

    public int ID { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public Nullable<System.DateTime> DateOfBirth { get; set; }
    public Nullable<int> Gender { get; set; }
    public bool Active { get; set; }
    public string Salutation { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public virtual ICollection<PersonEmployment> PersonEmployments { get; set; }
}

public partial class PersonEmployment
{
    public int ID { get; set; }
    public Nullable<int> PersonID { get; set; }
    public Nullable<System.DateTime> JoiningDate { get; set; }
    public Nullable<int> DesignationID { get; set; }
    public Nullable<System.DateTime> SeparationDate { get; set; }
    public string OfficialEmail { get; set; }
    public string PersonalEmail { get; set; }

    public virtual Person Person { get; set; }
}

public partial class PersonSkillMapping
{
    public int ID { get; set; }
    public int SkillID { get; set; }
    public int PersonID { get; set; }
    public int ExperienceYears { get; set; }
    public Nullable<int> ExperienceMonths { get; set; }
    public bool HasCoreCompetency { get; set; }
    public int SkillRating { get; set; }
}

public partial class SkillMatrix
{
    public int ID { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
    public string SkillCategory { get; set; }
}

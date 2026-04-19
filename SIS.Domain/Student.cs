using SIS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Domain
{
    public class Student : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public string Password { get; set; } = string.Empty; // Will be hashed
    public string StudentNumber { get; set; } = string.Empty;
    
    // Navigation (no EF attributes)
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    
    // Domain helper
    public string FullName => $"{FirstName} {LastName}".Trim();
}
}

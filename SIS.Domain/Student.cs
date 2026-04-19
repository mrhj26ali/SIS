using SIS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Domain
{
    public class Student : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public string Password { get; set; }
        public string StudentNumber { get; set; }

        // Navigation Property
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}

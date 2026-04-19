using SIS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Domain
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        // Navigation Property
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}

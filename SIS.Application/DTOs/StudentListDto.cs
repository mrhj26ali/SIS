using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Application.DTOs
{
    public class StudentListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }  // Combined first and last names.
        public string StudentNumber { get; set; }
        public List<string> CourseNames { get; set; }
    }
}

using SIS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
namespace SIS.Domain
{
public class Course : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}}
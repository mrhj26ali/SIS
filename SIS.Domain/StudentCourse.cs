using SIS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
namespace SIS.Domain
{
public class StudentCourse : BaseEntity
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!; // EF will populate
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!; // EF will populate
}}
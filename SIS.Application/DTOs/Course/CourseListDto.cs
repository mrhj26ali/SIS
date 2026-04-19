namespace SIS.Application.DTOs.Course;

public class CourseListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int EnrolledStudentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
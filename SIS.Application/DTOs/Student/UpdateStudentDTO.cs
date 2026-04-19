namespace SIS.Application.DTOs.Student;

public class UpdateStudentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public List<int> CourseIds { get; set; } = new();
}
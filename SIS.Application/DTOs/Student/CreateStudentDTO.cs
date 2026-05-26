namespace SIS.Application.DTOs.Student;

public class CreateStudentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Age { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string? IdentityUserId { get; set; }
    public List<int> CourseIds { get; set; } = new();
}
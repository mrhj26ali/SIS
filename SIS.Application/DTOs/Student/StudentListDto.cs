public class StudentListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public List<string> CourseNames { get; set; } = new();
}
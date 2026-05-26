namespace Logging.Domain.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}

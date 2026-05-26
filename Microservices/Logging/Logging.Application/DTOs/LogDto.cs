using System.ComponentModel.DataAnnotations;

namespace Logging.Application.DTOs
{
    public class LogDto
    {
        public int Id { get; set; }

        [Required]
        public string? Message { get; set; }
        [Required]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

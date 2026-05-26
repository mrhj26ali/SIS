using System.ComponentModel.DataAnnotations;

namespace Logging.Application.DTOs
{
    public class CreateLogDto
    {
        [Required]
        public string? Message { get; set; }
        [Required]
        public string? CreatedBy { get; set; }
    }

}

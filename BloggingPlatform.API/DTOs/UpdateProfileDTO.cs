using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.DTOs
{
    public class UpdateProfileDTO
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
    }
}
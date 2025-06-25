using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
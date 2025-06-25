using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BloggingPlatform.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        [JsonIgnore]
        public ICollection<Article> Articles { get; set; } = new List<Article>();

        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Helper property for full name
        public string FullName => $"{FirstName} {LastName}";
    }
}
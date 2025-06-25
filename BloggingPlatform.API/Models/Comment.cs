using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int ArticleId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Article Article { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(20)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Image file path stored in the database
        public string? ImagePath { get; set; }

        // Flag to mark featured articles
        public bool IsFeatured { get; set; } = false;

        // Foreign key
        public int AuthorId { get; set; }

        // Navigation properties
        public User Author { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
    }
}
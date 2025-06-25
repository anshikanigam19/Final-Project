using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        // Navigation property for many-to-many relationship with Article
        public ICollection<ArticleCategory> ArticleCategories { get; set; } = new List<ArticleCategory>();
    }
}
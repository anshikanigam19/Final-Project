namespace BloggingPlatform.API.Models
{
    public class ArticleCategory
    {
        // Composite key is defined in DbContext
        public int ArticleId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public Article Article { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}
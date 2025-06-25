using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.DTOs
{
    public class ArticleCreateDTO
    {
        [Required]
        [MinLength(5)]
        [MaxLength(150)]
        [RegularExpression(@"^[\w\s.,!?'""():;\-]+$", ErrorMessage = "Title can only contain letters, numbers, spaces, and basic punctuation.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(20)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one category must be selected.")]
        [MaxLength(3, ErrorMessage = "Maximum 3 categories can be selected.")]
        public List<int> CategoryIds { get; set; } = new List<int>();
        
        // The image file will be handled separately in the controller
    }

    public class ArticleUpdateDTO
    {
        [Required]
        [MinLength(5)]
        [MaxLength(150)]
        [RegularExpression(@"^[\w\s.,!?'""():;\-]+$", ErrorMessage = "Title can only contain letters, numbers, spaces, and basic punctuation.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(20)]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one category must be selected.")]
        [MaxLength(3, ErrorMessage = "Maximum 3 categories can be selected.")]
        public List<int> CategoryIds { get; set; } = new List<int>();
        
        // The image file will be handled separately in the controller
        public bool RemoveExistingImage { get; set; } = false;
    }

    public class ArticleResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ImagePath { get; set; }
        public UserDTO Author { get; set; } = null!;
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public List<CommentResponseDTO> Comments { get; set; } = new List<CommentResponseDTO>();
    }

    public class ArticleSummaryDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ImagePath { get; set; }
        public UserDTO Author { get; set; } = null!;
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public int CommentCount { get; set; }
    }

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryWithCountDTO : CategoryDTO
    {
        public int ArticleCount { get; set; }
    }

    public class SearchResultDTO
    {
        public List<ArticleSummaryDTO> Results { get; set; } = new List<ArticleSummaryDTO>();
        public int TotalCount { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
    }
}
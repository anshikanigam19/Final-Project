using System.ComponentModel.DataAnnotations;

namespace BloggingPlatform.API.DTOs
{
    public class CommentCreateDTO
    {
        [Required]
        public int ArticleId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentDTO
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentResponseDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserDTO User { get; set; } = null!;
        public int ArticleId { get; set; }
        public string? ArticleTitle { get; set; }
    }
}
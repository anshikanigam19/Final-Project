using BloggingPlatform.API.DTOs;

namespace BloggingPlatform.API.Services
{
    public interface ICommentService
    {
        Task<CommentResponseDTO> CreateCommentAsync(CommentCreateDTO commentDto, int userId);
        Task<bool> DeleteCommentAsync(int id, int userId);
        Task<List<CommentResponseDTO>> GetCommentsByArticleIdAsync(int articleId);
        Task<List<CommentResponseDTO>> GetCommentsByUserIdAsync(int userId);
        Task<CommentResponseDTO> UpdateCommentAsync(int id, string content, int userId);
    }
}
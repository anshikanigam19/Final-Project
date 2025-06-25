using BloggingPlatform.API.DTOs;

namespace BloggingPlatform.API.Services
{
    public interface IArticleService
    {
        Task<ArticleResponseDTO?> GetArticleByIdAsync(int id);
        Task<List<ArticleSummaryDTO>> GetAllArticlesAsync(int page = 1, int pageSize = 10);
        Task<List<ArticleSummaryDTO>> GetArticlesByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);
        Task<List<ArticleSummaryDTO>> GetArticlesByAuthorAsync(int authorId, int page = 1, int pageSize = 10);
        Task<ArticleResponseDTO> CreateArticleAsync(ArticleCreateDTO articleDto, int authorId, string? imagePath = null);
        Task<ArticleResponseDTO?> UpdateArticleAsync(int id, ArticleUpdateDTO articleDto, int userId, string? imagePath = null, bool removeExistingImage = false);
        Task<bool> DeleteArticleAsync(int id, int userId);
        Task<SearchResultDTO> SearchArticlesAsync(string searchTerm, int page = 1, int pageSize = 10);
        Task<List<ArticleSummaryDTO>> GetFeaturedArticlesAsync(int limit = 3);
    }
}
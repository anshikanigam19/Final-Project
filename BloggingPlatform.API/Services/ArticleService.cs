using BloggingPlatform.API.Data;
using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BloggingPlatform.API.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ArticleResponseDTO?> GetArticleByIdAsync(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleCategories)
                    .ThenInclude(ac => ac.Category)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return null;

            return MapToArticleResponseDTO(article);
        }

        public async Task<List<ArticleSummaryDTO>> GetAllArticlesAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleCategories)
                    .ThenInclude(ac => ac.Category)
                .Include(a => a.Comments)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => MapToArticleSummaryDTO(a))
                .ToListAsync();
        }

        public async Task<List<ArticleSummaryDTO>> GetArticlesByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            return await _context.ArticleCategories
                .Where(ac => ac.CategoryId == categoryId)
                .Include(ac => ac.Article)
                    .ThenInclude(a => a.Author)
                .Include(ac => ac.Article)
                    .ThenInclude(a => a.ArticleCategories)
                        .ThenInclude(ac => ac.Category)
                .Include(ac => ac.Article)
                    .ThenInclude(a => a.Comments)
                .Select(ac => ac.Article)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => MapToArticleSummaryDTO(a))
                .ToListAsync();
        }

        public async Task<List<ArticleSummaryDTO>> GetArticlesByAuthorAsync(int authorId, int page = 1, int pageSize = 10)
        {
            return await _context.Articles
                .Where(a => a.AuthorId == authorId)
                .Include(a => a.Author)
                .Include(a => a.ArticleCategories)
                    .ThenInclude(ac => ac.Category)
                .Include(a => a.Comments)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => MapToArticleSummaryDTO(a))
                .ToListAsync();
        }

        public async Task<ArticleResponseDTO> CreateArticleAsync(ArticleCreateDTO articleDto, int authorId, string? imagePath = null)
        {
            // Validate category count
            if (articleDto.CategoryIds.Count < 1 || articleDto.CategoryIds.Count > 3)
                throw new ArgumentException("Articles must have between 1 and 3 categories.");

            // Validate categories exist
            var categoryIds = await _context.Categories
                .Where(c => articleDto.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (categoryIds.Count != articleDto.CategoryIds.Count)
                throw new ArgumentException("One or more selected categories do not exist.");

            // Create article
            var article = new Article
            {
                Title = articleDto.Title,
                Content = articleDto.Content,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                ImagePath = imagePath // Set the image path if provided
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            // Add categories
            foreach (var categoryId in articleDto.CategoryIds)
            {
                _context.ArticleCategories.Add(new ArticleCategory
                {
                    ArticleId = article.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            // Reload article with relationships
            return await GetArticleByIdAsync(article.Id) ?? throw new InvalidOperationException("Failed to retrieve created article.");
        }

        public async Task<ArticleResponseDTO?> UpdateArticleAsync(int id, ArticleUpdateDTO articleDto, int userId, string? imagePath = null, bool removeExistingImage = false)
        {
            var article = await _context.Articles
                .Include(a => a.ArticleCategories)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return null;

            // Check if user is the author
            if (article.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only edit your own articles.");

            // Validate category count
            if (articleDto.CategoryIds.Count < 1 || articleDto.CategoryIds.Count > 3)
                throw new ArgumentException("Articles must have between 1 and 3 categories.");

            // Validate categories exist
            var categoryIds = await _context.Categories
                .Where(c => articleDto.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (categoryIds.Count != articleDto.CategoryIds.Count)
                throw new ArgumentException("One or more selected categories do not exist.");

            // Update article
            article.Title = articleDto.Title;
            article.Content = articleDto.Content;
            article.UpdatedAt = DateTime.UtcNow;
            
            // Handle image path updates
            if (imagePath != null)
            {
                article.ImagePath = imagePath;
            }
            else if (removeExistingImage)
            {
                article.ImagePath = null;
            }

            // Update categories
            // Remove existing categories
            _context.ArticleCategories.RemoveRange(article.ArticleCategories);

            // Add new categories
            foreach (var categoryId in articleDto.CategoryIds)
            {
                _context.ArticleCategories.Add(new ArticleCategory
                {
                    ArticleId = article.Id,
                    CategoryId = categoryId
                });
            }

            await _context.SaveChangesAsync();

            // Reload article with relationships
            return await GetArticleByIdAsync(article.Id);
        }

        public async Task<bool> DeleteArticleAsync(int id, int userId)
        {
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
                return false;

            // Check if user is the author
            if (article.AuthorId != userId)
                throw new UnauthorizedAccessException("You can only delete your own articles.");

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<SearchResultDTO> SearchArticlesAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty.");

            // Normalize search term
            searchTerm = searchTerm.Trim().ToLower();

            // Query articles that match the search term in title or content
            var query = _context.Articles
                .Where(a => a.Title.ToLower().Contains(searchTerm) || a.Content.ToLower().Contains(searchTerm))
                .Include(a => a.Author)
                .Include(a => a.ArticleCategories)
                    .ThenInclude(ac => ac.Category)
                .Include(a => a.Comments)
                .OrderByDescending(a => a.CreatedAt);

            // Get total count
            var totalCount = await query.CountAsync();

            // Get paginated results
            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new SearchResultDTO
            {
                Results = articles.Select(MapToArticleSummaryDTO).ToList(),
                TotalCount = totalCount,
                SearchTerm = searchTerm
            };
        }

        // Helper methods for mapping entities to DTOs
        private static ArticleResponseDTO MapToArticleResponseDTO(Article article)
        {
            return new ArticleResponseDTO
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                ImagePath = article.ImagePath,
                Author = new UserDTO
                {
                    Id = article.Author.Id,
                    FirstName = article.Author.FirstName,
                    LastName = article.Author.LastName,
                    Email = article.Author.Email
                },
                // Ensure Categories is always initialized as an empty list if ArticleCategories is null
                Categories = article.ArticleCategories?.Select(ac => new CategoryDTO
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Description = ac.Category.Description
                }).ToList() ?? new List<CategoryDTO>(),
                // Ensure Comments is always initialized as an empty list if Comments is null
                Comments = article.Comments?.OrderBy(c => c.CreatedAt).Select(c => new CommentResponseDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ArticleId = c.ArticleId,
                    User = new UserDTO
                    {
                        Id = c.User.Id,
                        FirstName = c.User.FirstName,
                        LastName = c.User.LastName,
                        Email = c.User.Email
                    }
                }).ToList() ?? new List<CommentResponseDTO>()
            };
        }

        private static ArticleSummaryDTO MapToArticleSummaryDTO(Article article)
        {
            // Create an excerpt from the content (first 200 characters)
            var excerpt = StripHtmlTags(article.Content);
            if (excerpt.Length > 200)
                excerpt = excerpt.Substring(0, 200) + "...";

            return new ArticleSummaryDTO
            {
                Id = article.Id,
                Title = article.Title,
                Excerpt = excerpt,
                CreatedAt = article.CreatedAt,
                ImagePath = article.ImagePath,
                Author = new UserDTO
                {
                    Id = article.Author.Id,
                    FirstName = article.Author.FirstName,
                    LastName = article.Author.LastName,
                    Email = article.Author.Email
                },
                // Ensure Categories is always initialized as an empty list if ArticleCategories is null
                Categories = article.ArticleCategories?.Select(ac => new CategoryDTO
                {
                    Id = ac.Category.Id,
                    Name = ac.Category.Name,
                    Description = ac.Category.Description
                }).ToList() ?? new List<CategoryDTO>(),
                CommentCount = article.Comments?.Count ?? 0
            };
        }

        private static string StripHtmlTags(string html)
        {
            return Regex.Replace(html, @"<[^>]*>", string.Empty);
        }

        public async Task<List<ArticleSummaryDTO>> GetFeaturedArticlesAsync(int limit = 3)
        {
            // Get the most recent articles as featured articles
            // No longer filtering by image path to ensure we get articles even if none have images
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleCategories)
                    .ThenInclude(ac => ac.Category)
                .Include(a => a.Comments)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .Select(a => MapToArticleSummaryDTO(a))
                .ToListAsync();
        }
    }
}
using BloggingPlatform.API.Data;
using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatform.API.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommentResponseDTO> CreateCommentAsync(CommentCreateDTO commentDto, int userId)
        {
            // Check if article exists
            var article = await _context.Articles.FindAsync(commentDto.ArticleId);
            if (article == null)
                throw new ArgumentException("Article not found.");

            // Create comment
            var comment = new Comment
            {
                Content = commentDto.Content,
                ArticleId = commentDto.ArticleId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload comment with user information
            var createdComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (createdComment == null)
                throw new InvalidOperationException("Failed to retrieve created comment.");

            return new CommentResponseDTO
            {
                Id = createdComment.Id,
                Content = createdComment.Content,
                CreatedAt = createdComment.CreatedAt,
                ArticleId = createdComment.ArticleId,
                User = new UserDTO
                {
                    Id = createdComment.User.Id,
                    FirstName = createdComment.User.FirstName,
                    LastName = createdComment.User.LastName,
                    Email = createdComment.User.Email
                }
            };
        }

        public async Task<bool> DeleteCommentAsync(int id, int userId)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return false;

            // Check if user is the author of the comment
            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<CommentResponseDTO>> GetCommentsByArticleIdAsync(int articleId)
        {
            return await _context.Comments
                .Where(c => c.ArticleId == articleId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponseDTO
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
                })
                .ToListAsync();
        }

        public async Task<CommentResponseDTO> UpdateCommentAsync(int id, string content, int userId)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                throw new ArgumentException("Comment not found.");

            // Check if user is the author of the comment
            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own comments.");

            // Update comment
            comment.Content = content;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            // Reload comment with user information
            var updatedComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (updatedComment == null)
                throw new InvalidOperationException("Failed to retrieve updated comment.");

            return new CommentResponseDTO
            {
                Id = updatedComment.Id,
                Content = updatedComment.Content,
                CreatedAt = updatedComment.CreatedAt,
                ArticleId = updatedComment.ArticleId,
                User = new UserDTO
                {
                    Id = updatedComment.User.Id,
                    FirstName = updatedComment.User.FirstName,
                    LastName = updatedComment.User.LastName,
                    Email = updatedComment.User.Email
                }
            };
        }

        public async Task<List<CommentResponseDTO>> GetCommentsByUserIdAsync(int userId)
        {
            return await _context.Comments
                .Where(c => c.UserId == userId)
                .Include(c => c.User)
                .Include(c => c.Article)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponseDTO
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ArticleId = c.ArticleId,
                    ArticleTitle = c.Article.Title,
                    User = new UserDTO
                    {
                        Id = c.User.Id,
                        FirstName = c.User.FirstName,
                        LastName = c.User.LastName,
                        Email = c.User.Email
                    }
                })
                .ToListAsync();
        }
    }
}
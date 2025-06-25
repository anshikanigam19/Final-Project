using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BloggingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly IFileService _fileService;

        public ArticlesController(IArticleService articleService, IFileService fileService)
        {
            _articleService = articleService;
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ArticleSummaryDTO>>> GetAllArticles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var articles = await _articleService.GetAllArticlesAsync(page, pageSize);
            return Ok(articles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleResponseDTO>> GetArticleById(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);

            if (article == null)
                return NotFound(new { message = "Article not found." });

            return Ok(article);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<List<ArticleSummaryDTO>>> GetArticlesByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var articles = await _articleService.GetArticlesByCategoryAsync(categoryId, page, pageSize);
            return Ok(articles);
        }

        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<List<ArticleSummaryDTO>>> GetArticlesByAuthor(int authorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var articles = await _articleService.GetArticlesByAuthorAsync(authorId, page, pageSize);
            return Ok(articles);
        }

        [HttpGet("search")]
        public async Task<ActionResult<SearchResultDTO>> SearchArticles([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { message = "Search term cannot be empty." });

            var results = await _articleService.SearchArticlesAsync(searchTerm, page, pageSize);
            return Ok(results);
        }

        [HttpGet("featured")]
        public async Task<ActionResult<List<ArticleSummaryDTO>>> GetFeaturedArticles([FromQuery] int limit = 3)
        {
            var articles = await _articleService.GetFeaturedArticlesAsync(limit);
            return Ok(articles);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ArticleResponseDTO>> CreateArticle([FromForm] ArticleCreateDTO articleDto, IFormFile? image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                
                // Handle image upload if provided
                string? imagePath = null;
                if (image != null && image.Length > 0)
                {
                    try
                    {
                        imagePath = await _fileService.SaveImageAsync(image);
                    }
                    catch (ArgumentException ex)
                    {
                        return BadRequest(new { message = ex.Message });
                    }
                }
                
                var article = await _articleService.CreateArticleAsync(articleDto, userId, imagePath);
                return CreatedAtAction(nameof(GetArticleById), new { id = article.Id }, article);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ArticleResponseDTO>> UpdateArticle(int id, [FromForm] ArticleUpdateDTO articleDto, IFormFile? image)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                
                // Get the current article to check if we need to delete an existing image
                var currentArticle = await _articleService.GetArticleByIdAsync(id);
                if (currentArticle == null)
                    return NotFound(new { message = "Article not found." });
                
                // Handle image upload or removal
                string? imagePath = null;
                bool deleteExistingImage = articleDto.RemoveExistingImage;
                
                if (image != null && image.Length > 0)
                {
                    try
                    {
                        // Upload new image
                        imagePath = await _fileService.SaveImageAsync(image);
                        deleteExistingImage = true; // Replace old image with new one
                    }
                    catch (ArgumentException ex)
                    {
                        return BadRequest(new { message = ex.Message });
                    }
                }
                
                // Delete existing image if requested
                if (deleteExistingImage && !string.IsNullOrEmpty(currentArticle.ImagePath))
                {
                    _fileService.DeleteImage(currentArticle.ImagePath);
                }
                
                var article = await _articleService.UpdateArticleAsync(id, articleDto, userId, imagePath, deleteExistingImage);

                if (article == null)
                    return NotFound(new { message = "Article not found." });

                return Ok(article);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteArticle(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                var result = await _articleService.DeleteArticleAsync(id, userId);

                if (!result)
                    return NotFound(new { message = "Article not found." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
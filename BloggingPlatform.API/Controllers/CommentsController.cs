using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloggingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("article/{articleId}")]
        public async Task<ActionResult<List<CommentResponseDTO>>> GetCommentsByArticleId(int articleId)
        {
            var comments = await _commentService.GetCommentsByArticleIdAsync(articleId);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentResponseDTO>> CreateComment(CommentCreateDTO commentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                var comment = await _commentService.CreateCommentAsync(commentDto, userId);
                return CreatedAtAction(nameof(GetCommentsByArticleId), new { articleId = comment.ArticleId }, comment);
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
        public async Task<ActionResult> DeleteComment(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                var result = await _commentService.DeleteCommentAsync(id, userId);

                if (!result)
                    return NotFound(new { message = "Comment not found." });

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<CommentResponseDTO>> UpdateComment(int id, [FromBody] UpdateCommentDTO updateCommentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                var updatedComment = await _commentService.UpdateCommentAsync(id, updateCommentDto.Content, userId);
                return Ok(updatedComment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<List<CommentResponseDTO>>> GetUserComments()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User not authenticated."));
                var comments = await _commentService.GetCommentsByUserIdAsync(userId);
                return Ok(comments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
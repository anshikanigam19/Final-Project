using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloggingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("with-count")]
        public async Task<ActionResult<List<CategoryWithCountDTO>>> GetCategoriesWithCount()
        {
            var categories = await _categoryService.GetCategoriesWithCountAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }
    }
}
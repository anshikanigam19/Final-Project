using BloggingPlatform.API.Data;
using BloggingPlatform.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatform.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<List<CategoryWithCountDTO>> GetCategoriesWithCountAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryWithCountDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ArticleCount = c.ArticleCategories.Count
                })
                .ToListAsync();
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return null;

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
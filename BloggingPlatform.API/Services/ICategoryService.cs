using BloggingPlatform.API.DTOs;

namespace BloggingPlatform.API.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDTO>> GetAllCategoriesAsync();
        Task<List<CategoryWithCountDTO>> GetCategoriesWithCountAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int id);
    }
}
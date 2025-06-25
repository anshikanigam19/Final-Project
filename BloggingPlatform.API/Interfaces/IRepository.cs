namespace BloggingPlatform.API.Interfaces
{
    // This is a placeholder interface to ensure the namespace exists
    // You can extend this with actual repository interfaces as needed
    public interface IRepository<T> where T : class
    {
        // Common repository methods can be defined here
        // For example:
        // Task<IEnumerable<T>> GetAllAsync();
        // Task<T> GetByIdAsync(int id);
        // Task<T> AddAsync(T entity);
        // Task UpdateAsync(T entity);
        // Task DeleteAsync(T entity);
    }
}
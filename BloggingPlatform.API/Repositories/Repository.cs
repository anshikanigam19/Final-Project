using BloggingPlatform.API.Data;
using BloggingPlatform.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloggingPlatform.API.Repositories
{
    // This is a placeholder repository class to ensure the namespace exists
    // You can extend this with actual repository implementations as needed
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Implement the interface methods here if needed
    }
}
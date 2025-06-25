using BloggingPlatform.API.Models;
using System;
using System.Linq;

namespace BloggingPlatform.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Database creation is now handled in Program.cs
            // No data seeding - only fetch from database
            Console.WriteLine("Database initialization complete. No data seeding performed.");
            
            // Ensure database is created
            context.Database.EnsureCreated();
            
            // Log the number of existing records (if any)
            int categoriesCount = context.Categories.Count();
            int usersCount = context.Users.Count();
            int articlesCount = context.Articles.Count();
            
            Console.WriteLine($"Database contains: {categoriesCount} categories, {usersCount} users, {articlesCount} articles.");
            
            // No data seeding - removed all test data
        }
        }
    }

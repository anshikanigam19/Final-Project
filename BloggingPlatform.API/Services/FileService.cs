using Microsoft.AspNetCore.Http;

namespace BloggingPlatform.API.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine("uploads", "images");
            
            // Ensure the uploads directory exists
            var fullPath = Path.Combine(_environment.WebRootPath, _uploadsFolder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was provided");
            }

            // Validate file is an image
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("File is not a valid image. Allowed formats: jpg, jpeg, png, gif");
            }

            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var relativePath = Path.Combine(_uploadsFolder, fileName);
            var filePath = Path.Combine(_environment.WebRootPath, relativePath);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative path that will be stored in the database
            return relativePath.Replace("\\", "/"); // Ensure forward slashes for web URLs
        }

        public bool DeleteImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            // Convert to system path format
            filePath = filePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
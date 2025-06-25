namespace BloggingPlatform.API.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Saves an image file to the server and returns the file path
        /// </summary>
        /// <param name="file">The image file to save</param>
        /// <returns>The relative path to the saved file</returns>
        Task<string> SaveImageAsync(IFormFile file);

        /// <summary>
        /// Deletes an image file from the server
        /// </summary>
        /// <param name="filePath">The relative path to the file</param>
        /// <returns>True if the file was deleted, false otherwise</returns>
        bool DeleteImage(string filePath);
    }
}
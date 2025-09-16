using ITSL_Administration.Models;

namespace ITSL_Administration.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<UploadedFile> UploadFileAsync(IFormFile file, FileContentType contentType, string userId);

        // CHANGED: return a Stream instead of byte[] for efficiency
        Task<Stream> DownloadFileAsync(string fileId);

        Task<bool> DeleteFileAsync(string fileId);

        // CHANGED: made async to fix concurrency issues
        Task<string?> GetFilePathAsync(string fileId);
    }
}

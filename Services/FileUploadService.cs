
using ITSL_Administration.Data;
using ITSL_Administration.Models;
using ITSL_Administration.Services.Interfaces;

namespace ITSL_Administration.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string _uploadBasePath;

        private readonly string[] _permittedExtensions =
        {
        ".pdf", ".ppt", ".pptx", ".doc", ".docx",
        ".xls", ".xlsx", ".mp4", ".mp3",".mov", ".jpg",
        ".jpeg", ".png", ".txt", ".zip", ".rar",
        ".cpp", ".java", ".py"
    };

        private const long _maxFileSize = 10 * 1024 * 1024;

        public FileUploadService(AppDbContext context, IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;

            _uploadBasePath = Path.Combine(_environment.ContentRootPath, "UploadedFiles");

            if (!Directory.Exists(_uploadBasePath))
            {
                Directory.CreateDirectory(_uploadBasePath);
            }
        }

        public async Task<UploadedFile> UploadFileAsync(IFormFile file, FileContentType contentType, string userId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("No file uploaded or file is empty.");

                if (file.Length > _maxFileSize)
                    throw new ArgumentException(
                        $"File size exceeds the maximum limit of {_maxFileSize / (1024 * 1024)}MB.");

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !_permittedExtensions.Contains(fileExtension))
                {
                    var allowedExtensions = string.Join(", ", _permittedExtensions);
                    throw new ArgumentException($"File type not permitted. Allowed types: {allowedExtensions}");
                }


                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new ArgumentException("Invalid user. Cannot upload file.");

                var fileId = Guid.NewGuid().ToString();
                var storedFileName = $"{fileId}{fileExtension}";
                var storedFilePath = Path.Combine(_uploadBasePath, storedFileName);

                using (var stream = new FileStream(storedFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var uploadedFile = new UploadedFile
                {
                    FileId = fileId,
                    FileName = file.FileName,
                    FilePath = $"/UploadedFiles/{storedFileName}",
                    StoredFilePath = storedFilePath,
                    FileSize = file.Length,
                    ContentType = contentType,
                    UploadedByUserID = userId,
                    User = user
                };

                _context.UploadedFiles.Add(uploadedFile);
                await _context.SaveChangesAsync();

                return uploadedFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }


        public async Task<Stream> DownloadFileAsync(string fileId)
        {
            var uploadedFile = await _context.UploadedFiles.FindAsync(fileId);
            if (uploadedFile == null)
                throw new FileNotFoundException("File not found in database.");

            if (!File.Exists(uploadedFile.StoredFilePath))
                throw new FileNotFoundException("File not found on disk.");

            return new FileStream(uploadedFile.StoredFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            var uploadedFile = await _context.UploadedFiles.FindAsync(fileId);
            if (uploadedFile == null)
                return false;

            if (File.Exists(uploadedFile.StoredFilePath))
            {
                File.Delete(uploadedFile.StoredFilePath);
            }

            _context.UploadedFiles.Remove(uploadedFile);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string?> GetFilePathAsync(string fileId)
        {
            var uploadedFile = await _context.UploadedFiles.FindAsync(fileId);
            return uploadedFile?.FilePath;
        }
    }
}

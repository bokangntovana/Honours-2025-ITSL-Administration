using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSL_Administration.Models
{
    //This stores the uploaded files information//metadata in the database.
    //It would be useful for archiving, retrieval, and management of files uploaded by users.
    public class UploadedFile
    {
        [Key]
        public string FileId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string StoredFilePath { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        // Is the uploaded file for User personal info for registration /Courses content/Assignment Instructions/Submissions for assignments
        public FileContentType ContentType { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        // The user who uploaded the file, a foreign key to the User table
        public string UploadedByUserID { get; set; } = string.Empty;

        // Navigation properties for relationships
        [ForeignKey("UploadedByUserID")]
        public required User User { get; set; }
    }
}

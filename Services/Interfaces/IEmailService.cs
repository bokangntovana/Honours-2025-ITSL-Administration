namespace ITSL_Administration.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, List<IFormFile>? attachments = null);
        Task SendEmailToManyAsync(IEnumerable<string> toEmails, string subject, string body, List<IFormFile>? attachments = null);
    }

}

using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public interface IMailDeliveryService
    {
        public Task SendEmailAsync(string fromAddress, List<string> toAddress, string subject, string body);
        public Task SendConfirmationEmailAsync(MailDeliveryConfirmationContentModel details);
    }
}

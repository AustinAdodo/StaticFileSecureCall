using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public interface IMailDeliveryService
    {
        /// <summary>
        /// Dependencies: 
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public Task SendEmailAsync(string fromAddress, List<string> toAddress, string subject, string body);
        public Task SendConfirmationEmailAsync(MailDeliveryConfirmationContentModel details);
    }
}

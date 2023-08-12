namespace StaticFileSecureCall.Services
{
    public interface IMailDeliveryService
    {
        public Task DeliverAsync(string subject, string body, string recipientEmail, string clientId, string clientSecret);
        public Task SendEmailAsync(string fromAddress, List<string> toAddress, string subject, string body);
    }
}

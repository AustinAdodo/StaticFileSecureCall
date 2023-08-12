namespace StaticFileSecureCall.Services
{
    public interface MailDeliveryService
    {
        public Task DelieverAsync(string Subject, string Body, string RecipientEmail, string Username, string Pass);
    }
}

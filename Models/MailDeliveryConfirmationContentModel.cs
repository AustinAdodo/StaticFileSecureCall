namespace StaticFileSecureCall.Models
{
    public class MailDeliveryConfirmationContentModel
    {
        public string Filename { get; set; } = string.Empty;
        public string FileId { get; set; } = string.Empty;
        public string UserIpAddress { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
    }
}

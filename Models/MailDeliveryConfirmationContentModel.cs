namespace StaticFileSecureCall.Models
{
    /// <summary>
    /// A simple Content Model created to help get necessary paramters required to be sent for a confirmation email.
    /// </summary>
    public class MailDeliveryConfirmationContentModel
    {
        public string Filename { get; set; } = string.Empty;
        public string FileId { get; set; } = string.Empty;
        public string UserIpAddress { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
    }
}

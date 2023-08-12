using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices;


namespace StaticFileSecureCall.Services
{
    /// <summary>
    /// Optimized by Austin. email service is introduced to other assemblies through Dependency Injection.
    /// </summary>
    public class MailManager : MailDeliveryService
    {
        private readonly ILogger<MailManager> _logger;
        public MailManager(ILogger<MailManager> logger)
        {
            _logger = logger;
        }
        // Using port 465 to ensure SSL/TLS is configured. //string smtpHost = Configuration["SmtpConfig:SmtpHost"];
        public async Task DelieverAsync(string Subject, string Body, string RecipientEmail, string Username, string Pass)
        {
            int smtpPort = 465;
            string smtpHost = "smtp.gmail.com";
            //int smtpPort = 587;
            //string smtpUsername = RetrieveKeyAsync().Result[1];
            //string smtpPassword = RetrieveKeyAsync().Result[0];
            string smtpUsername = Username;
            string smtpPassword = Pass;
            bool enableSsl = true;

            // Create SMTP client
            using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = enableSsl;
                //smtpClient.Security
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = Subject,
                    Body = Body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(RecipientEmail);
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (IOException ioex)
                {
                    _logger.LogError($"Error sending email to {RecipientEmail}: {ioex.Message}");
                }
                catch (SmtpException smtpex)
                {
                    _logger.LogError($"Error sending email to {RecipientEmail}: {smtpex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending email to {RecipientEmail}: {ex.Message}");
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}

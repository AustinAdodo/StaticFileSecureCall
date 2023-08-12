//using System.Net.Mail;
//using System.Net;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MailKit.Security;
//using System.Net.Mail;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace StaticFileSecureCall.Services
{
    /// <summary>
    /// Optimized by Austin. email service is introduced to other assemblies through Dependency Injection.
    /// Install-Package SendGrid
    /// </summary>
    
    public class MailManager : IMailDeliveryService
    {
        private readonly ILogger<MailManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly AmazonSimpleEmailServiceClient _sesClient;

        public MailManager(ILogger<MailManager> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        // Using port 465 to ensure SSL/TLS is configured. //string smtpHost = Configuration["SmtpConfig:SmtpHost"];
        //public async Task DeliverAsync1(string Subject, string Body, string RecipientEmail, string Username, string Pass)
        //{
        //    int smtpPort = 465;
        //    string smtpHost = "smtp.gmail.com";
        //    //int smtpPort = 587;
        //    string smtpUsername = Username;
        //    string smtpPassword = Pass;
        //    bool enableSsl = true;
        //    // Create SMTP client
        //    using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
        //    {
        //        smtpClient.UseDefaultCredentials = false;
        //        smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        //        smtpClient.EnableSsl = enableSsl;
        //        //smtpClient.Security
        //        MailMessage mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(smtpUsername),
        //            Subject = Subject,
        //            Body = Body,
        //            IsBodyHtml = false
        //        };
        //        mailMessage.To.Add(RecipientEmail);
        //        try
        //        {
        //            await smtpClient.SendMailAsync(mailMessage);
        //        }
        //        catch (IOException ioex)
        //        {
        //            _logger.LogError($"Error sending email to {RecipientEmail}: {ioex.Message}");
        //        }
        //        catch (SmtpException smtpex)
        //        {
        //            _logger.LogError($"Error sending email to {RecipientEmail}: {smtpex.Message}");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Error sending email to {RecipientEmail}: {ex.Message}");
        //            throw new Exception(ex.Message);
        //        }
        //    }
        //}

        public async Task DeliverAsync(string subject, string body, string recipientEmail, string clientId, string clientSecret)
        {
            string refreshToken = string.Empty;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Austin", "your-email@gmail.com"));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Plain) { Text = body };

            using (var client = new SmtpClient())
            {
                // Connect using OAuth2 authentication
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                var oauth2 = new SaslMechanismOAuth2(clientId, clientSecret); //refresh token
                client.Authenticate(oauth2);
                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }

        public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string body)
        {
            //(~/.aws/credentials) and (~/.aws/config)
            var credentials = new Amazon.Runtime.BasicAWSCredentials("YOUR_ACCESS_KEY_ID", "YOUR_SECRET_ACCESS_KEY");
            var sesClient = new Amazon.SimpleEmail.AmazonSimpleEmailServiceClient(credentials, Amazon.RegionEndpoint.EUWest1);
            var sendRequest = new SendEmailRequest
            {
                Source = fromAddress,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { toAddress }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Text = new Content(body)
                    }
                }
            };
            await _sesClient.SendEmailAsync(sendRequest);
        }
    }
}

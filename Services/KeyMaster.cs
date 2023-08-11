using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Amazon.SecretsManager;
using Amazon;
using Amazon.SecretsManager.Model;
using Amazon.Runtime;
//using Amazon.SecretsManager.Extensions.Caching;

namespace StaticFileSecureCall.Services
{
    public class KeyMaster : IKeyGenerator
    {
        private readonly string randomString;
        private readonly string name;
        private readonly ILogger<KeyMaster> _logger;
        public KeyMaster(ILogger<KeyMaster> logger)
        {
            randomString = GenerateRandomString();
            name = $"request{DateTime.Now:HH:mm:ss}";
            _logger = logger;
        }

        private void SendGeneratedKey()
        {
            // Email configuration
            string smtpHost = "smtp.example.com";
            int smtpPort = 587;
            string smtpUsername = "info.kygosystems@gmail.com";
            string smtpPassword = "your-email-password";  //retrieve from secret manager.
            bool enableSsl = true;

            // Email addresses
            string[] recipientEmails = { "kdonaldresources@gmail.com", "abtesting911@gmail.com", "subzbelow@gmail.com" };

            // Create SMTP client
            using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = enableSsl;
                //send to AWS vault.
                foreach (string recipientEmail in recipientEmails)
                {
                    // Create the email message
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUsername),
                        Subject = $"Secret Key and Name at {DateTime.Now.ToString(@"dddd")}, {DateTime.Now.ToString(@"dd")} " +
                        $"{DateTime.Now:HH:mm:ss}",
                        Body = $"Hi there,\n here's your Name and Secret respectively;\n Name: {name}\n Secret: {randomString} " +
                        $"\n\n\n regards Austin.live.ai.",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(recipientEmail);
                    // Send the email
                    try
                    {
                        smtpClient.Send(mailMessage);
                        Console.WriteLine($"Email sent successfully to {recipientEmail}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email to {recipientEmail}: {ex.Message}");
                    }
                }
            }
        }

        //KeyGen
        public static string GenerateRandomString(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789£#~&%_-$@?/";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //save to AWS secret Manager.
        public async Task ConfigureKeyAsync()
        {
            SendGeneratedKey();
            AWSCredentials credentials = new BasicAWSCredentials(name, randomString);
            AmazonSecretsManagerClient secretsManagerClient = new AmazonSecretsManagerClient(
                credentials,
                RegionEndpoint.EUWest1 // Determine the best region discussing with Tunde.
            );
            string secretName = name;
            string secretValue = randomString;
            var request = new PutSecretValueRequest
            {
                SecretId = secretName,
                SecretString = secretValue
            };
            PutSecretValueResponse response = await secretsManagerClient.PutSecretValueAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK) // Check if the response indicates success
            {
                _logger.LogInformation("Secret successfully deployed from KeyGen to AWS");
            }
            secretsManagerClient.Dispose();
        }


        //retrieve from Amazon Secret Manager.
        public void RetrieveKey()
        {
            //    private const string MySecretName = "MySecret";

            //private SecretsManagerCache cache = new SecretsManagerCache();

            //public async Task<Response> FunctionHandlerAsync(string input, ILambdaContext context)
            //{
            //    string MySecret = await cache.GetSecretString(MySecretName);

            //    // Use the secret, return success

            //}
        }
    }
}


using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Amazon.SecretsManager;
using Amazon;
using Amazon.SecretsManager.Model;
using Amazon.Runtime;
using System.Runtime.Intrinsics.X86;
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
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 465;  // Using port 465 to ensure SSL/TLS is configured.
            string smtpUsername = "info.kygosystems@gmail.com";
            string smtpPassword = RetrieveKey().Result;
            bool enableSsl = true;

            // Email addresses
            string[] recipientEmails = { "kdonaldresources@gmail.com", "abtesting911@gmail.com", "subzbelow@gmail.com" };

            // Create SMTP client
            using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = enableSsl;
                //smtpClient.Security
                foreach (string recipientEmail in recipientEmails)
                {
                    // Create the email message
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUsername),
                        Subject = $"Secret Key and Name at {DateTime.Now.ToString(@"dddd")}, {DateTime.Now.ToString(@"dd")} " +
                        $"{DateTime.Now:HH:mm:ss}",
                        Body = $"Hi there,\n here's your Name and Secret respectively;\n Name: {name}\n Secret: {randomString} Pertinent to " +
                        $"note that secrets refresh every 1 hour." +
                        $"\n\n\n regards Austin.live.ai.",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(recipientEmail);
                    try
                    {
                        smtpClient.Send(mailMessage);
                        _logger.LogInformation($"Email sent successfully to {recipientEmail}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending email to {recipientEmail}: {ex.Message}");
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

        //KeyGen
        //Key Generation Algorith developed by Austin.
        //Further updates to be considered.
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

        //retrieve from AWS Secret Manager.
        public async Task<string> RetrieveKey()
        {
            string secretName = "mailpass";
            string secretValue = string.Empty;
            using (var secretsManagerClient = new AmazonSecretsManagerClient(RegionEndpoint.USWest2))
            {
                var getRequest = new GetSecretValueRequest
                {
                    SecretId = secretName
                };
                try
                {
                    var getResponse = await secretsManagerClient.GetSecretValueAsync(getRequest);
                    secretValue = getResponse.SecretString;
                    _logger.LogInformation("Secret successfully retrieved.");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error retrieving secret: " + ex.Message);
                }
            }
            return secretValue;
            //private SecretsManagerCache cache = new SecretsManagerCache();
            //public async Task<Response> FunctionHandlerAsync(string input, ILambdaContext context)
            //{
            //    string MySecret = await cache.GetSecretString(MySecretName);
            //    // Use the secret, return success
            //}
        }
    }
}


using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
//using Amazon.SecretsManager.Extensions.Caching;

namespace StaticFileSecureCall.Services
{
    public class KeyMaster : IKeyGenerator
    {
        public KeyMaster()
        {
        }

        public void GenerateKey()
        {
            string randomString = GenerateRandomString();
            string name = $"request{DateTime.Now:HH:mm:ss}";

            // Email configuration
            string smtpHost = "smtp.example.com";
            int smtpPort = 587;
            string smtpUsername = "info.kygosystems@gmail.com";
            string smtpPassword = "your-email-password";
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
                        $"\n\n\n regards Austin live ai.",
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
                this.ConfigureKey(randomString);
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
        public void ConfigureKey(string s)
        {
            throw new NotImplementedException();
        }

        //persist to Amaon Secret Manager.
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


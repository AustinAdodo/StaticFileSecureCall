using System.Net.Mail;
using System.Net;

namespace StaticFileSecureCall.Services
{
    public class KeyMaster : IKeyGenerator
    {
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
            string[] recipientEmails = { "kdonaldresources@gmail.com", "abtesting911@gmail.com","subzbelow@gmail.com" };

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
                        Subject = $"SecretKey and Name at {DateTime.Now.ToString(@"ddd")}, {DateTime.Now.ToString(@"dd")} " +
                        $"{DateTime.Now:HH:mm:ss}",
                        Body = $"Hi there,\n here's your Name and Secret respectively; Name: {name} Secret: {randomString} \n regards Austin.",
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
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void ConfigureKey()
        {
            throw new NotImplementedException();
        }

        public void RetrieveKey()
        {
            throw new NotImplementedException();
        }
    }
}


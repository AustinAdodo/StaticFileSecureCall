﻿using System.Net.Mail;
using System.Net;

namespace StaticFileSecureCall.Services
{
    public class KeyMaster : IKeyGenerator
    {
        public void GenerateKey()
        {
            string randomString = GenerateRandomString();

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
                foreach (string recipientEmail in recipientEmails)
                {
                    // Create the email message
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUsername),
                        Subject = "SecretKey and Name",
                        Body = $"Here's your random string: {randomString}",
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
        static string GenerateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}


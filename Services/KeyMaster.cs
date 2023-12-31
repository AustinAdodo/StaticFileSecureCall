﻿using System.Net.Mail;
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
    /// <summary>
    ///********Key Generation Algorithm developed by Austin.
    ///********Further updates to be considered.
    /// </summary>

    public class KeyMaster : IKeyGenerator
    {
        private readonly string randomString;
        private readonly IMailDeliveryService _mailDeliveryService;
        private readonly string name;
        private readonly ILogger<KeyMaster> _logger;
        private readonly ICredentialService _credentialService;
        public KeyMaster(ILogger<KeyMaster> logger, IMailDeliveryService mailDeliveryService, ICredentialService credentialService = null)
        {
            randomString = GenerateRandomString();
            name = $"request{DateTime.Now:HH:mm:ss}";
            _logger = logger;
            _mailDeliveryService = mailDeliveryService;
            _credentialService = credentialService;
        }

        //email delivery.
        private async Task SendGeneratedKey()
        {
            //string Username = RetrieveKeyAsync().Result[1];
            //string smtpPassword = RetrieveKeyAsync().Result[0];
            string Username = "info.kygosystems.com";
            string subject = $"[Autogenerated]Secret Key and Name at {DateTime.Now.ToString(@"dddd")}, {DateTime.Now.ToString(@"dd")} " +
                        $"{DateTime.Now:HH:mm:ss}";
            string Body = $"Hi there,\n here's your Name and Secret respectively;\n Name: {name}\n Secret: {randomString}\n sent securely with SSH/TLS" +
                        $"note that secrets refresh every 1 hour." +
                        $"\n\n\n regards Austin.live.ai.";
            string[] recipientEmails = { "subzbelow@gmail.com", "kdonaldresources@gmail.com", "abtesting911@gmail.com" };
                await _mailDeliveryService.SendEmailAsync(Username,recipientEmails.ToList(),subject, Body);
                _logger.LogInformation(message: $"Email sent successfully to these emails {string.Join(",", recipientEmails)}");
        }

        //KeyGen
        public static string GenerateRandomString(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789£#~&%_-$@?/";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ConfigureKeyAsync()
        {
            await SendGeneratedKey();
            string secretName = name;
            string secretValue = randomString;
            await _credentialService.ExportCredentialAsync(secretName, secretValue);    
        }

        //retrieve from AWS Secret Manager.
        public async Task<string[]> RetrieveKeyAsync()
        {
            string secretName = "mailpass";
            string secretEmail = "mailname";
            string result1 = await _credentialService.ImportCredentialAsync(secretName);
            string result2  = await _credentialService.ImportCredentialAsync(secretEmail);
            return new string[] { result1, result2 };   
            }
    }
}
//private SecretsManagerCache cache = new SecretsManagerCache();
//public async Task<Response> FunctionHandlerAsync(string input, ILambdaContext context)
//{
//    string MySecret = await cache.GetSecretString(MySecretName);
//    // Use the secret, return success
//}


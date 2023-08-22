﻿using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Amazon.Runtime;
using System.Net;
using Amazon;
using System.Configuration;
using Amazon.Extensions.NETCore.Setup;

namespace StaticFileSecureCall.Services
{
    /// <summary>
    /// Asyncronously handling Retrieval and persistence of credentials from AWS.
    /// </summary>
    public class CredentialManager : ICredentialService
    {
        private readonly ILogger<CredentialManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public CredentialManager(ILogger<CredentialManager> logger, IWebHostEnvironment environment, IConfiguration configuration)
        {
            this._logger = logger;
            _environment = environment;
            _configuration = configuration;
        }

        /// <summary>
        /// replace * with the appropriate AWS access key ID and secret access key obtained from your AWS account.
        /// For a list of the exceptions thrown, see  https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
        /// AWS access credentials
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AuthenticateCredentials()
        {
            string accessKeyId = string.Empty; string secretAccessKey = string.Empty;
            var valiadationResult = await ValidateAndAssumeRole(accessKeyId, secretAccessKey);
            if (valiadationResult == CredentialValidationResult.Success)
            {
                _logger.LogInformation("Successful handshake with AWS authentication.");
                return true;
            }
            if (_environment.IsDevelopment() || _environment.IsStaging()) _logger.LogError("Something went wrong with AWS authentication."); ;
            if (_environment.IsProduction()) _logger.LogError("Something went wrong with AWS authentication.");
            return false;
        }

        /// <summary>
        /// Export autogenerated credentials to AWS.
        /// </summary>
        /// <param name="CredentialName"></param>
        /// <param name="CredentialSecret"></param>
        /// <returns></returns>
        public async Task ExportCredentialAsync(string CredentialName, string CredentialSecret)
        {
            AWSCredentials credentials = new BasicAWSCredentials(CredentialName, CredentialSecret);
            AmazonSecretsManagerClient secretsManagerClient = new AmazonSecretsManagerClient(
                credentials,
                RegionEndpoint.USEast1 // Determine the best region discussing with Tunde.
            );
            var request = new PutSecretValueRequest
            {
                SecretId = CredentialName,
                SecretString = CredentialSecret
            };
            PutSecretValueResponse response = await secretsManagerClient.PutSecretValueAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK) // Check if the response indicates success
            {
                _logger.LogInformation("Secret successfully deployed from KeyGen to AWS");
            }
            secretsManagerClient.Dispose();
        }

        public async Task<string> ImportCredentialAsync(string CrentialName)
        {
            var awsOptions = _configuration.GetAWSOptions();
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(awsOptions.Region.SystemName)
            };
            using (var secretsManagerClient = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.USWest1))
            {
                var getRequest = new GetSecretValueRequest
                {
                    SecretId = CrentialName
                };
                try
                {
                    var getResponse = await secretsManagerClient.GetSecretValueAsync(getRequest);
                    string secretValue = getResponse.SecretString;
                    _logger.LogInformation("Secrets successfully retrieved.");
                    return secretValue;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error retrieving secrets: " + ex.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// installed the package : Install-Package AWSSDK.SecurityToken
        /// Primary Role of this function is to return a success Result upon credential confirmation handshake.
        /// Replace YOUR_ACCOUNT_ID with your AWS account ID and YOUR_ROLE_NAME with the name of the IAM role you want to assume.
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="secretAccessKey"></param>
        /// <returns></returns>
        public async Task<CredentialValidationResult> ValidateAndAssumeRole(string accessKeyId, string secretAccessKey)
        {
            AmazonSecurityTokenServiceClient stsClient = new AmazonSecurityTokenServiceClient(accessKeyId, secretAccessKey);
            AssumeRoleRequest assumeRoleRequest = new AssumeRoleRequest
            {
                RoleArn = "arn:aws:iam::YOUR_ACCOUNT_ID:role/YOUR_ROLE_NAME",
                RoleSessionName = "AssumedRoleSession"
            };
            try
            {
                AssumeRoleResponse assumeRoleResponse = await stsClient.AssumeRoleAsync(assumeRoleRequest);
                return CredentialValidationResult.Success;
            }
            catch (AmazonSecurityTokenServiceException)
            {
                return CredentialValidationResult.Failure;
            }
        }
        public enum CredentialValidationResult
        {
            Success,
            Failure
        }
    }
}
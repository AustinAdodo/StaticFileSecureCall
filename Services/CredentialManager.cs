using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Amazon.Runtime;
using System.Net;
using Amazon;

namespace StaticFileSecureCall.Services
{
    /// <summary>
    /// Asyncronously handling Retrieval and persistence of credentials from AWS.
    /// </summary>
    public class CredentialManager : ICredentialService
    {
        private readonly ILogger<CredentialManager> _logger;

        public CredentialManager(ILogger<CredentialManager> logger)
        {
            this._logger = logger;
        }
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
    }
}

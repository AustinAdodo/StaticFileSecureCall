using AspNetCore.Authentication.ApiKey;

namespace StaticFileSecureCall
{
    public class ApiKeyProvider : IApiKeyProvider
    {
        // Implement the methods defined in the IApiKeyProvider interface
        // ValidateApiKeyAsync for validating the API key
        // ProvideApiKeyAsync for providing the API key
        public Task<IApiKey> ProvideAsync(string key)
        {
            throw new NotImplementedException();
        }
    }
}

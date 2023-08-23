namespace StaticFileSecureCall.Services
{
    public interface ICredentialService
    {
        /// <summary>
        /// Dependencies: 
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public Task<bool> AuthenticateCredentials();
        public Task ExportCredentialAsync(string CredentialName , string CredentialSecret );
        public Task<string> ImportCredentialAsync(string CrentialName);
    }
}

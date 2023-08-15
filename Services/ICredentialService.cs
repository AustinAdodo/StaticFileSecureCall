namespace StaticFileSecureCall.Services
{
    public interface ICredentialService
    {
        public Task ExportCredentialAsync(string CredentialName , string CredentialSecret );
        public Task<string> ImportCredentialAsync(string CrentialName);
    }
}

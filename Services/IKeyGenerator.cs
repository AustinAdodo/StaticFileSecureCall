namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
        Task ConfigureKeyAsync();
        Task<string> RetrieveKey();
    }
}

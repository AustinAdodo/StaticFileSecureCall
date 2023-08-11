namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
       public Task ConfigureKeyAsync();
       public Task<string[]> RetrieveKeyAsync();
    }
}

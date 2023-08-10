namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
        Task ConfigureKeyAsync();
        void RetrieveKey();
    }
}

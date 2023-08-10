namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
        void ConfigureKey();
        void RetrieveKey();
    }
}

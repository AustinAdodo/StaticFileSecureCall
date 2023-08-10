namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
        void GenerateKey();
        void ConfigureKey();
        void RetrieveKey();
    }
}

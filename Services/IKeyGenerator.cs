namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
        void GenerateKey();
        void ConfigureKey(string s);
        void RetrieveKey();
    }
}

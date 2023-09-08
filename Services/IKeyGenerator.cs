namespace StaticFileSecureCall.Services
{
    public interface IKeyGenerator
    {
        /// <summary>
        /// Dependencies: 
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public Task ConfigureKeyAsync();
       public Task<string[]> RetrieveKeyAsync();
    }
}

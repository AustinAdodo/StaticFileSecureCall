using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public class PersistenceService : IPersistence
    {
        public void DeleteFileAsync(string internalId)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileRepository>> GetAllFilesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FileRepository> GetFileAsync(string internalId)
        {
            throw new NotImplementedException();
        }

        public void SaveFileAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FileRepository> UpdateFileAsync(string internalId)
        {
            throw new NotImplementedException();
        }
    }
}

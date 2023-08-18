using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public interface IPersistence
    {
        public Task<FileRepository> GetFileAsync(string internalId);
        Task<FileRepository> UpdateFileAsync(string internalId);
        public Task<IEnumerable<FileRepository>> GetAllFilesAsync();
        public Task SaveFileAsync(string fileName);
        void DeleteFileAsync(string internalId);
    }
}

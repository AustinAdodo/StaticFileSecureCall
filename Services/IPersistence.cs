using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public interface IPersistence
    {
        public FileRepository GetFile(string internalId);
        Task<FileRepository> UpdateFileAsync(string internalId);
        Task<List<FileRepository>> GetAllFilesAsync();
        public Task SaveFileAsync(string fileName);
        void DeleteFileAsync(string internalId);
    }
}

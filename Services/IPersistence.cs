using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public interface IPersistence
    {
        public FileRepository GetFile(string internalId);
        Task<FileRepository> UpdateFileAsync(string internalId);
        Task<List<FileRepository>> GetAllFilesAsync();
        void SaveFileAsync();
        void DeleteFileAsync(string internalId);
    }
}

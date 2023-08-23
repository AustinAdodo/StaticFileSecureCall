using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public interface IPersistence
    {
        /// <summary>
        /// Dependencies: 
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public Task<FileRepository> GetFileAsync(string internalId);
        Task<FileRepository> UpdateFileAsync(string internalId);
        public Task<IQueryable<FileRepository>> GetAllFilesAsync();
        public Task SaveFileAsync(string fileName);
        void DeleteFileAsync(string internalId);
    }
}

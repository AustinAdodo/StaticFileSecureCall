using Microsoft.EntityFrameworkCore;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
    public class PersistenceService : IPersistence
    {
        private readonly ILogger<PersistenceService> _logger;
        private readonly AppDbContext _appContext;
        public PersistenceService(AppDbContext appContext, ILogger<PersistenceService> logger)
        {
            _appContext = appContext;
            _logger = logger;
        }
        public void DeleteFileAsync(string internalId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<FileRepository>> GetAllFilesAsync()
        {
            var all = await _appContext.FileRepositories.ToListAsync();
            return all;
        }

        public FileRepository GetFile(string internalId)
        {
            var File = GetAllFilesAsync().Result.Where(a => a.InternalId == internalId).First();
            return File;
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

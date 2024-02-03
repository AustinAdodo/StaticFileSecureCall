using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Models;
using System.Data.Entity.Infrastructure;

namespace StaticFileSecureCall.Services
{
    /// <summary>
    /// This is a layer on the Query Provider : Must Implement IDbAsyncEnumerable<FileRepository> as specified when using EF6.
    /// IQueryable operations are not executed immediately; they build up an expression tree that represents the query.
    /// </summary>
    public class PersistenceService : IPersistence
    {
        private readonly ILogger<PersistenceService> _logger;
        private readonly AppDbContext _appContext;
        private IWebHostEnvironment _webHostEnvironment;
        public PersistenceService(AppDbContext appContext, ILogger<PersistenceService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _appContext = appContext;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        
        public void DeleteFileAsync(string internalId)
        {
            // Implement your delete logic asynchronously
            throw new NotImplementedException();
        }

        public async Task<IQueryable<FileRepository>> GetAllFilesAsync()
        {
            var all =  _appContext.FileRepositories.AsQueryable();
            return all;
        }

        public async Task<FileRepository> GetFileAsync(string internalId)
        {
            var file = await _appContext.FileRepositories.FirstOrDefaultAsync(a => a.InternalId == internalId);
            return file;
        }
        
        public async Task SaveFileAsync(string fileName)
        {
            var model = new FileRepository()
            {
                Filename = fileName,
                InternalId = Guid.NewGuid().ToString(),
                Address = Path.Combine(_webHostEnvironment.WebRootPath, "ServeStaticFiles", fileName),
                Reference = "On"
            };

            _appContext.FileRepositories?.Add(model);
            await _appContext.SaveChangesAsync();
        }
        
        public async Task<FileRepository> UpdateFileAsync(string internalId)
        {
            // Implement your update logic asynchronously
            throw new NotImplementedException();
        }
    }

}

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using StaticFileSecureCall.DataManagement;
using StaticFileSecureCall.Models;

namespace StaticFileSecureCall.Services
{
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

        public async Task SaveFileAsync(string fileName)
        {
            var model = new FileRepository()
            {
                Filename = fileName,
                InternalId = Guid.NewGuid().ToString(),
                Address = Path.Combine(_webHostEnvironment.WebRootPath, "ServeStaticFiles", fileName),
                Reference = "On"
            };
            _appContext.FileRepositories.Add(model);
            await _appContext.SaveChangesAsync();
        }

        public Task<FileRepository> UpdateFileAsync(string internalId)
        {
            throw new NotImplementedException();
        }
    }
}

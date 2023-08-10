using Microsoft.AspNetCore.Mvc;
using StaticFileSecureCall.Services;
using System.IO;


namespace StaticFileSecureCall.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly string[] _authorizedIpAddresses;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IKeyGenerator _generator;
        public HomeController(IConfiguration configuration, IKeyGenerator generator, IHttpContextAccessor contextAccessor, ILogger logger)
        {
            _authorizedIpAddresses = configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>();
            _generator = generator;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        [HttpGet("check")]
        public IActionResult Index()
        {
            string message = $"Api works fine and is ready to go! :)";
            return Ok(message);
        }

        [HttpGet("reqCurrent")]
        public IActionResult ReqCurrent()
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (_authorizedIpAddresses.Contains(remoteIpAddress))
            {
                // Authorized logic
                string message = $"use the same API endpoint with secretkey provided to you by admin to activate Download.";
                return Ok(message);
            }
            else
            {
                return Forbid(); // 403 Forbidden
            }
        }

        [HttpGet("{fileName}")]
        private IActionResult Download(string fileName)
        {
            //retrieve cached Generated Password secret from AWS vault.
            //aws filepath
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), Path.GetFileName(filePath));
        }

        private string GetContentType(string path)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using StaticFileSecureCall.Services;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;


namespace StaticFileSecureCall.Controllers
{
    [Route("/")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly string[] _authorizedIpAddresses;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IKeyGenerator _generator;

        public HomeController(IConfiguration configuration, IKeyGenerator generator, IHttpContextAccessor contextAccessor, ILogger<HomeController> logger)
        {
            _authorizedIpAddresses = (configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>())?? new string[] {"192.168.1.1" };
            _generator = generator;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult Index()
        {
            string message = $"Api works fine and is ready to go! :)";
            _logger.LogInformation("Success health check initiated successfully");
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
                _generator.ConfigureKey();   
                return Ok(message);
            }
            else
            {
                return Forbid(); // 403 Forbidden
            }
        }

        [HttpGet("reqCurrent/{name}")]
        public IActionResult ReqCurrent([FromBody] string secret)
        {
            //retrieve cached Generated Password secret from AWS vault.
            string? name = _contextAccessor.HttpContext?.Request.Query["name"].ToString();
            string filename = name; //dummy
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (_authorizedIpAddresses.Contains(remoteIpAddress)) // and name and secret matches.
            {
                return RedirectToAction("Download", new { name = filename });
            }
            else
            {
                return Forbid("Unauthorized Request"); // 403 Forbidden
            }
        }

        [HttpGet("{fileName}")]
        private IActionResult Download(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();
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
            var provider = new FileExtensionContentTypeProvider();
            string? contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}


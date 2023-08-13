using Microsoft.AspNetCore.Mvc;
using StaticFileSecureCall.Services;
using Microsoft.AspNetCore.StaticFiles;
using System.Net;
using System.IO;
using StaticFileSecureCall.Validation;

namespace StaticFileSecureCall.Controllers
{
    [Route("/")]
    [ApiController]
    [ServiceFilter(typeof(RateLimitFilter))]
    public class HomeController : Controller
    {
        public const string baseuri = "api";
        private const string _errorMessage = "Unauthorized access detected, contact admin";
        private const string _errorMessagekey = "Unauthorized key detected, your access will be blocked if this persists";
        private readonly string[] _authorizedIpAddresses;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IKeyGenerator _generator;

        public HomeController(IConfiguration configuration, IKeyGenerator generator,
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger)
        {
            _authorizedIpAddresses = (configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>()) ?? new string[] { "192.168.1.1" };
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
            //var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(); 
            var remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
            string? formattedIpAddress = remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
               ? remoteIpAddress.MapToIPv4().ToString()
               : remoteIpAddress.ToString();
            if (_authorizedIpAddresses.Contains(formattedIpAddress))
            {
                // Authorized logic
                string message = $"A token has been sent to the admin, kindly request for this token" +
                    $"use {baseuri}/reqCurrent/name, where 'name' is the Secret Name provided by the admin." +
                    $"This token will expire in 45 minutes.";
                _generator.ConfigureKeyAsync();
                return Ok(message);
            }
            else
            {
                return Forbid(_errorMessage); // 403 Forbidden
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
                return Forbid(_errorMessagekey); // 403 Forbidden
            }
        }

        [HttpGet("{fileName}")]
        private IActionResult Download(string fileName)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound("The file pathname or directory could not be located");
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


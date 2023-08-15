using Microsoft.AspNetCore.Mvc;
using StaticFileSecureCall.Services;
using Microsoft.AspNetCore.StaticFiles;
using System.Net;
using System.IO;
using StaticFileSecureCall.Validation;
using StaticFileSecureCall.Decorators;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using StaticFileSecureCall.Models;
using System.Reflection.Metadata.Ecma335;

namespace StaticFileSecureCall.Controllers
{
    /// <summary>
    /// **************Rate Limiting can be configured to each Endpoint independently.
    /// **************For example Here we have configured to allow maximum of two requests for window of five seconds in "Status" Action . 
    /// **************Whenever there is a third request within the windows of five seconds
    /// </summary>

    [Route("/")]
    [ApiController]
    //[ServiceFilter(typeof(RateLimitFilter))]
    public class HomeController : Controller
    {
        public const string baseuri = "api";
        private const string _errorMessage = "Unauthorized access detected, contact admin";
        private const string _errorMessagekey = "Unauthorized key detected, your access will be blocked if this persists";
        private readonly string[] _authorizedIpAddresses;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IKeyGenerator _generator;
        private readonly IPersistence _persistenceService;
        private readonly IMailDeliveryService _emailService;

        public HomeController(IConfiguration configuration, IKeyGenerator generator, IMailDeliveryService emailService,
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger, IPersistence persistenceService)
        {
            _authorizedIpAddresses = (configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>()) ?? new string[] { "192.168.1.1" };
            _generator = generator;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _persistenceService = persistenceService;
            _emailService = emailService;
        }

        [HttpGet("status")]
        [LimitRequest(MaxRequests = 5, TimeWindow = 10)]
        public IActionResult Index()
        {
            string message = $"Api works fine and is ready to go! :)";
            _logger.LogInformation("Success health check initiated successfully");
            return Ok(message);
        }

        [HttpGet("confirmAccess")]
        [LimitRequest(MaxRequests = 3, TimeWindow = 10)]
        public IActionResult Accessibility()
        {
            var remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
            string? formattedIpAddress = remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
               ? remoteIpAddress.MapToIPv4().ToString()
               : remoteIpAddress.ToString();
            if (_authorizedIpAddresses.Contains(formattedIpAddress))
            {
                string message = $"You're in, you can proceed.";
                return Ok(message);
            }
            else
            {
                return StatusCode(404,$"{_errorMessage} contact admin to register IP address.");
            }
        }

        [HttpGet("reqCurrent")]
        [LimitRequest(MaxRequests = 5, TimeWindow = 3600)]
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

        [HttpGet("reqCurrent/{refid}")]
        //[LimitRequest(MaxRequests = 3, TimeWindow = 3600)]
        public IActionResult ProceedToDownload()
        {
            //retrieve cached Generated Password secret from AWS vault.
            string? refid = _contextAccessor.HttpContext?.Request.Query["refid"].ToString();
            FileRepository result = new FileRepository();
            refid = "9CC8E423-C217-4C9C-B3FD-C82E286B0F0C";
            try
            {
                var all = _persistenceService.GetAllFilesAsync().Result;
                foreach (var file in all) { 
                Console.WriteLine(file);    
                }
                result = all.Where(a => a.InternalId == refid).First();
            }
            catch (Exception ex)
            {
                string Errormsg = $"Error Retrieving From database {ex.Message}";
                _logger.LogInformation(message: $"{Errormsg}");
            }
            try
            {
                string? name = _contextAccessor.HttpContext?.Request.Query["name"].ToString();
                string filename = result.Filename; //dummy
                var remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
                string? formattedIpAddress = remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
              ? remoteIpAddress.MapToIPv4().ToString()
              : remoteIpAddress.ToString();
                if (_authorizedIpAddresses.Contains(formattedIpAddress)) // and name and secret matches.
                {
                    Download(filename);
                    return Ok("Download Initiated");
                }
                else
                {
                    return Forbid(_errorMessagekey); // 403 Forbidden
                }
            }
            catch (Exception)
            {
                String msg = $"The attemepted Access to the file with id {refid} returned the following error \n";
                string Errormsg = "This File has either been used or does not exist.";
                _logger.LogInformation(message: $"{msg}{Errormsg}");
                return StatusCode(404, Errormsg);
            }
        }

        private IActionResult Download(string fileName)
        {
            var Details = new MailDeliveryConfirmationContentModel()
            {
                Filename = fileName,
            };
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound("The file pathname or directory could not be located");
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open)) stream.CopyTo(memory);
            memory.Position = 0;
            var result = File(memory, GetContentType(filePath), Path.GetFileName(filePath));
            _emailService.SendConfirmationEmailAsync(Details);//download is successfull.
            return result;
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


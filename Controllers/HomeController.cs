namespace StaticFileSecureCall.Controllers
{
    /// <summary>
    /// **************All Controllers developed by Austin.
    /// **************Rate Limiting can be configured to each Endpoint independently.
    /// **************For example Here we have configured to allow maximum of two requests for window of five seconds in "Status" Action . 
    /// **************Whenever there is a third request within the windows of five seconds
    /// **************Developed by Austin.
    /// **************Internal members should use an API key to execute am upload
    /// </summary>

    [Route("/")]
    [ApiController]
    //[ServiceFilter(typeof(RateLimitFilter))]
    public class HomeController : Controller
    {
        public readonly string _currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToString();
        public const string baseuri = "http://assetcapitalfiat.us-east-1.elasticbeanstalk.com/";
        private const string _errorMessage = "Unauthorized access detected, contact admin";
        private const string _errorMessagekey = "Unauthorized key detected, your access will be blocked if this persists";
        private readonly string[] _authorizedIpAddresses;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IKeyGenerator _generator;
        private readonly IPersistence _persistenceService;
        private readonly IMailDeliveryService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICredentialService _credenialService;
        public HomeController(IConfiguration configuration, IKeyGenerator generator, IMailDeliveryService emailService,
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger, IPersistence persistenceService, IWebHostEnvironment webHostEnvironment, ICredentialService credenialService)
        {
            _authorizedIpAddresses = (configuration.GetSection("AppSettings:AuthorizedIpAddresses").Get<string[]>()) ?? new string[] { "192.168.1.1" };
            _generator = generator;
            _contextAccessor = contextAccessor;
            _logger = logger;
            _persistenceService = persistenceService;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _credenialService = credenialService;
        }

        /// <summary>
        /// Confirm If API is Up and Running , NB: This endpoint is rate Limted.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/")]
        [HttpGet("status")]
        [LimitRequest(MaxRequests = 5, TimeWindow = 10)]
        public IActionResult Index()
        {
            string message = $"Api works fine and is ready to go! :)";
            _logger.LogInformation("Success health check initiated successfully");
            return Ok(message);
        }

        /// <summary>
        /// Only Zip Files can be uploaded to the server.
        /// Requires API key
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Operations/Uploadfolder")]
        //[Authorize(Policy = "ApiKeyPolicy")]
        public async Task<IActionResult> UploadFile([FromForm] UploadDirectoryModel model)
        {
            string uploadDirectory = Path.Combine($"{_webHostEnvironment.WebRootPath}", "ServeStaticFiles", $"Check{Guid.NewGuid()}");
            if (model.DirectoryZipFile == null || model.DirectoryZipFile.Length == 0)
            {
                return BadRequest("No zip file provided.");
            }
            using (var memoryStream = new MemoryStream())
            {
                await model.DirectoryZipFile.CopyToAsync(memoryStream);
                using (var archive = new ZipArchive(memoryStream))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fullPath = Path.Combine(uploadDirectory, entry.FullName);
                        if (entry.Length == 0)
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                            using (var entryStream = entry.Open())
                            using (var fileStream = new FileStream(fullPath, FileMode.Create))
                            {
                                entryStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            }
            return Ok("Directory uploaded successfully.");
        }

        private async Task<string> WriteFile(IFormFile file, string DirectoryPath)
        {
            string fileName = file.FileName;
            try
            {
                var extension = $".{file.FileName.Split(".")[file.FileName.Split(".").Length - 1]}";
                var filepath = Path.Combine(DirectoryPath, fileName);
                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Failed to upload file: {ex.Message}");
            }
            return fileName;
        }

        /// <summary>
        /// For Clients using this API toconfirm if they have access, this uses the clients API and other variables
        /// NB: This endpoint is rate Limited.
        /// </summary>
        /// <returns></returns>
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
                return StatusCode(404, $"{_errorMessage} contact admin to register IP address.");
            }
        }

        /// <summary>
        /// Clients Must use the Endpoint if they desire to download a file, Upon using the endpoint , a token will be sent to the Admin
        /// for the clients to use on the endpoint. reqCurrent/{refid}.
        /// This Endpoint is rate Limited.
        /// </summary>
        /// <returns></returns>
        [HttpGet("reqCurrent")]
        [LimitRequest(MaxRequests = 5, TimeWindow = 3600)]
        public IActionResult ReqCurrent()
        {
            //var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(); 
            var remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
            string? formattedIpAddress = remoteIpAddress?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
               ? remoteIpAddress.MapToIPv4().ToString()
               : remoteIpAddress.ToString();
            if (_authorizedIpAddresses.Contains(formattedIpAddress))
            {
                // Authorized logic
                _generator.ConfigureKeyAsync();
                string message = $"A token has been sent to the admin, kindly request for this token, " +
                    $"use {baseuri}/reqCurrent/name, where 'name' is the Secret Name provided by the admin." +
                    $"This token will expire in 45 minutes."+
                    $"Input the provided 'Secret' and 'Secret Name' on the input body tag. ";
                return Ok(message);
            }
            else
            {
                return StatusCode(StatusCodes.Status403Forbidden,_errorMessage); // 403 Forbidden
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivedkeySecret"></param>
        /// <param name="receivedkeyName"></param>
        /// <returns></returns>
        [HttpGet("reqCurrent/{refid}")]
        [LimitRequest(MaxRequests = 3, TimeWindow = 3600)]
        public async Task<IActionResult> ProceedToDownload([FromQuery] string receivedkeySecret, [FromQuery] string receivedkeyName)
        {
            //retrieve KeyName for Secret from AWS vault.
            string? refid = _contextAccessor.HttpContext?.Request.Query["refid"].ToString();
            FileRepository result = new FileRepository();
            receivedkeyName = "TestCredential";
            refid = "9CC8E423-C217-4C9C-B3FD-C82E286B0F0C";
            bool condition = false;
            try
            {
                //add retries
                string resultKey = await _credenialService.ImportCredentialAsync(receivedkeyName);
                condition = resultKey == receivedkeySecret;
                if (condition == false) return StatusCode(StatusCodes.Status406NotAcceptable, "Failed Credential Verification");
            }
            catch (AWSCommonRuntimeException ex)
            {
                if (_currentEnvironment == Environments.Development) throw;
                if (_currentEnvironment == Environments.Production) return StatusCode(406, ex.Message);
            }
            if (condition)
            {
                try
                {
                    //add retries
                    var all = await _persistenceService.GetAllFilesAsync().Result.ToListAsync();
                    result = all.Where(a => a.InternalId == refid).First();
                }
                catch (Exception ex)
                {
                    string Errormsg = $"Error Retrieving From database {ex.Message}";
                    _logger.LogInformation(message: $"{Errormsg}");
                    return StatusCode(404, $"Problem reading database, Details : {ex.Message}");
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
                        DownloadZip(result);
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
            else return StatusCode(StatusCodes.Status417ExpectationFailed, _errorMessagekey);
        }

        /// <summary>
        /// This is private and Inaccessible.
        /// This function handles binary deployment to the download stream and download.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private IActionResult DownloadZip(FileRepository model)
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "ServeStaticFiles", $"{model.Filename}.zip");
            bool condition = System.IO.File.Exists(filePath);
            if (condition)
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;
                var contentType = "application/zip"; // MIME type for ZIP files
                // Serve the ZIP file for download
                var result = File(memory, contentType, Path.GetFileName(filePath));
                // Send the confirmation email
                var remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
                string? formattedIpAddress = remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
              ? remoteIpAddress.MapToIPv4().ToString()
              : remoteIpAddress.ToString();
                var details = new MailDeliveryConfirmationContentModel
                {
                    Filename = model.Filename,
                    UserIpAddress = formattedIpAddress,
                    FileId = model.InternalId,
                    EmailAddress = ""
                };
                _emailService.SendConfirmationEmailAsync(details);
                return result;
            }
            else
            {
                return NotFound("The ZIP File you're attempting to download couldn't be located or isn't 'Switched On'");
            }
        }

        /// <summary>
        /// Resolve File Content Type of th File to be downloaded.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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
    public class UploadDirectoryModel
    {
        public IFormFile DirectoryZipFile { get; set; }
    }
}
















//var memory = new MemoryStream();
//using (var stream = new FileStream(filePath, FileMode.Open))
//{
//    stream.CopyTo(memory);
//}
//memory.Position = 0;
//var contentType = GetContentType(filePath);
//// Serve the file for download
//var result = File(memory, contentType, Path.GetFileName(filePath));
//// Send the confirmation email
//var remoteIpAddress = _contextAccessor.HttpContext?.Connection.RemoteIpAddress;
//string? formattedIpAddress = remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
//? remoteIpAddress.MapToIPv4().ToString()
//: remoteIpAddress.ToString();
//var details = new MailDeliveryConfirmationContentModel
//{
//    Filename = model.Filename,
//    UserIpAddress = formattedIpAddress,
//    FileId = model.InternalId,
//    EmailAddress = ""
//};
//_emailService.SendConfirmationEmailAsync(details);
//return result;
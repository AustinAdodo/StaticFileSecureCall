using Microsoft.AspNetCore.Mvc;
using System.IO;


namespace StaticFileSecureCall.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        [HttpGet("check")]
        public IActionResult Index()
        {
            string message = $"Api works fine and is ready to go! :)";
            return Ok(message);
        }

        //public async Task<IActionResult>SendMessage()
        //{
        //    Task<string> message = await Task.FromResult( "use the same api with secret key provided to you in the body of the api");
        //    return Ok(message);
        //}

        [HttpGet("reqCurrent")]
        public async IActionResult ReqCurrent()
        {
            //await SendMessage();
            return RedirectToAction("Index","Authorization");   
        }

        [HttpGet("{fileName}")]
        private IActionResult Download(string fileName)
        {
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


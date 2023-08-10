using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;

namespace StaticFileSecureCall.Controllers
{
    public class AuthorizationController : Controller
    {
        public static readonly ReadOnlyCollection<string> AuthEmails = new List<string> { "kdonaldresources@gmail.com", "abtesting911@gmail.com" }.AsReadOnly();

        public IActionResult Index()
        {
            return View();
        }
    }
}

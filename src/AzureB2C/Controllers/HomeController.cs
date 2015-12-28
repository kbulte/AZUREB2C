using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Linq;

namespace AzureB2C.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()

        {
           
            ViewBag.Log = Startup.Log;
            return View();
        }

        //[Authorize]
        public IActionResult UserAuthorized()
        {
            ViewBag.Claims = HttpContext.User.Claims;
            return View();
        }

        // These would only work if Graph API was working
        [Authorize(Roles = "Company Administrator,Global Administrators,Billing Administrators,Administrators,Service Administrators,User Administrators,Password Administrators")]
        public IActionResult AdministratorAuthorized()
        {
            return View();
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            ViewBag.Log = Startup.Log;
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}

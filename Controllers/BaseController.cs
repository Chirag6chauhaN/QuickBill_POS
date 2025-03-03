using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuickBill_POS.Controllers
{
    public class BaseController : Controller
    {
        protected string LoggedInUsername => User.Identity?.Name; // Get the logged-in username

        protected IActionResult EnsureUserAuthenticated()
        {
            if (string.IsNullOrEmpty(LoggedInUsername))
            {
                return RedirectToAction("Login", "Account");
            }
            return null; // User is authenticated
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuickBill_POS.Interface;

namespace QuickBill_POS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUser _user;

        public AccountController(IUser user)
        {
            _user = user;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] // ✅ Ensures request authenticity
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _user.ValidateUser(username, password);
            if (user != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username) // Store username in claims
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              new ClaimsPrincipal(claimsIdentity),
                                              authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid Username or Password";
            return View();
        }



        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

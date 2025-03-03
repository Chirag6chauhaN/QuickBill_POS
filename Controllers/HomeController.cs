using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBill_POS.Business_Data;
using QuickBill_POS.Implimentation;
using QuickBill_POS.Interface;
using QuickBill_POS.Models;
using System.Diagnostics;

namespace QuickBill_POS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IBillItem _billItem; // assume this provides bill data
        private readonly IBill _bill;
        private readonly IUser _user;

        public HomeController(ILogger<HomeController> logger, IBillItem billItem, IBill bill, IUser user)
        {
            _logger = logger;
            _billItem = billItem;
            _bill = bill;
            _user = user;
        }

        [Authorize]
        public IActionResult UserProfile()
        {
            var username = User.Identity.Name; // Get the logged-in username from claims

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _user.GetUserByUsername(username); // Fetch user by username

            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // If user not found, force login
            }

            return View(user); // Only logged-in user's data is shown
        }


        [Authorize]
        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime today = DateTime.Today;
            if (!dateFrom.HasValue) dateFrom = today;
            if (!dateTo.HasValue) dateTo = today;

            // Get logged-in user's ID
            var username = User.Identity.Name;
            var user = _user.GetUserByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = user.Id; // Get UserId
            DateTime todayStart = today.Date; // Start of the day
            DateTime todayEnd = today.Date.AddDays(1).AddTicks(-1); // End of the day
            var todaysBills = await _billItem.GetBillsByDateRangeAsync(todayStart, todayEnd, userId);
            
            // Get filtered bills for this user
            var filteredBills = await _billItem.GetBillsByDateRangeAsync(dateFrom.Value, dateTo.Value, userId);

            // Get overall bills for this user
            var overallBills = await _bill.GetAllBillsAsync(userId);

            var model = new HomeViewModel
            {
                TodaysBillCount = todaysBills.Count(),
                TodaysTotalIncome = todaysBills.Sum(b => b.GrandTotal),
                FilterBillCount = filteredBills.Count(),
                FilterTotalIncome = filteredBills.Sum(b => b.GrandTotal),
                OverallBillCount = overallBills.Count(),
                OverallTotalIncome = overallBills.Sum(b => b.GrandTotal),
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

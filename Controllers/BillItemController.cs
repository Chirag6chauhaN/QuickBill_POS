using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuickBill_POS.Business_Data;
using QuickBill_POS.Implimentation;
using QuickBill_POS.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuickBill_POS.Controllers
{
    public class BillItemController : Controller
    {
        private readonly IBillItem _billItem;
        private readonly IFoodItem _foodItem;
        private readonly IUser _user;
        private readonly ILogger<BillItemController> _logger;

        public BillItemController(IBillItem billItem, IFoodItem foodItem, ILogger<BillItemController> logger, IUser user)
        {
            _billItem = billItem;
            _foodItem = foodItem;
            _logger = logger;
            _user = user;
        }

        [Authorize]
        public async Task<IActionResult> BillItemList(string billId = null, string searchTerm = "", string sortOrder = "billId_asc", int page = 1, int pageSize = 10)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var username = User.Identity.Name;
           

            if (string.IsNullOrEmpty(username))
            {
                TempData["Error"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var user = _user.GetUserByUsername(username);

            if (user == null || user.Id <= 0)
            {
                TempData["Error"] = "Invalid user session. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine(string.IsNullOrEmpty(billId) ? "Fetching all bill items for the user" : $"Fetching bill items for Bill ID: {billId}");


            var pageSizeOptions = new[] { 10, 20, 50 }
            .Select(size => $"<option value=\"{size}\" {(size == pageSize ? "selected" : "")} >{size}</option>")
            .ToList();
            ViewBag.PageSizeOptions = string.Join("", pageSizeOptions);


            // Pass billId to fetch filtered data
            //var billItems = await _billItem.GetAllBillingItemsAsync(billId);

            //if (!string.IsNullOrEmpty(searchTerm))
            //{
            //    billItems = billItems.Where(b => b.FoodItemName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            //}

            //return View(billItems);

            var result = await GetFilteredSortedPagedData(user.Id, billId, searchTerm, sortOrder, page, pageSize);

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.BillId = billId;

            return View(result.Data);


        }


        // GET: Add/Edit Bill (for header + details)
        [Authorize]
        public async Task<IActionResult> AddEditBillItem(int id = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var username = User.Identity.Name;
            var user = _user.GetUserByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            BillModel model = new BillModel();

            if (id > 0)
            {
                // Load an existing bill detail (for simplicity, we assume one detail represents the header)
                var billDetail = await _billItem.GetBillingItemByIdAsync(id, user.Id);
                if (billDetail == null)
                    return NotFound();

                model.BillId = billDetail.BillId;
                model.BillDate = DateTime.Now; // or load from your database
                model.BillDetails.Add(billDetail);
            }
            else
            {
                // For a new bill, generate a new BillId and add one default detail row.
                model.BillId = await _billItem.GenerateNextBillNumberAsync();
                model.BillDate = DateTime.Now;
                model.BillDetails.Add(new BillDetailModel { BillId = model.BillId });
            }

            // Load food items as SelectListItem.
            var foodItems = await _foodItem.GetAllFoodItemsAsync(user.Id) ?? new List<FoodItemModel>();
            ViewBag.FoodItems = foodItems.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name
            }).ToList();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddEditBillItem(BillModel model)
        {
            Business_Data.User user = null; // Declare user outside try-catch

            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }

                var username = User.Identity.Name;
                user = _user.GetUserByUsername(username); // Assign user here

                if (user == null || user.Id <= 0)
                {
                    _logger.LogError("Invalid user ID for username: {Username}", username);
                    TempData["Error"] = "User ID is invalid. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                model.UserId = user.Id;
                _logger.LogInformation("UserId set in model: {UserId}", model.UserId);

                // Ensure BillDetails is not null before accessing it
                model.BillDetails ??= new List<BillDetailModel>();

                // Propagate BillId to BillDetails
                foreach (var detail in model.BillDetails)
                {
                    detail.BillId = model.BillId;
                    detail.UserId = model.UserId;
                }

                _logger.LogInformation("Before Model Validation - UserId: {UserId}, BillId: {BillId}, Details Count: {Count}",
                    model.UserId, model.BillId, model.BillDetails.Count);

                // Recalculate GrandTotal based on details
                model.GrandTotal = model.BillDetails.Sum(d => d.Total);

                if (Convert.ToInt32(model.UserId) == 0)
                {
                    _logger.LogError("❌ UserId is still 0 before validation!");
                }


                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState)
                    {
                        foreach (var err in error.Value.Errors)
                        {
                            _logger.LogError("Model Validation Error: Field {Field}, Error: {Error}", error.Key, err.ErrorMessage);
                        }
                    }

                    TempData["ModelErrors"] = string.Join("<br>", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return View(model);
                }

                
                // Save the bill
                await _billItem.FinalizeBillAsync(new BillModel
                {
                    BillId = model.BillId,
                    BillDate = DateTime.Now,
                    GrandTotal = model.GrandTotal,
                    UserId = model.UserId
                }, model.BillDetails);

                TempData["Success"] = "Bill saved successfully!";
                return RedirectToAction("BillItemList", new { billId = model.BillId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving the bill");
                TempData["Error"] = "An error occurred while saving the bill. Please try again later.";

                if (User.Identity.IsAuthenticated && user != null) // ✅ Now 'user' is accessible here
                {
                    var foodItems = await _foodItem.GetAllFoodItemsAsync(user.Id) ?? new List<FoodItemModel>();
                    ViewBag.FoodItems = foodItems.Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = f.Name
                    }).ToList();
                }

                return View(model);
            }
        }


        private async Task<PagedResultModel<BillDetailListModel>> GetFilteredSortedPagedData(
     int userId, string billId, string searchTerm, string sortOrder, int page, int pageSize)
        {
            if (userId <= 0)
            {
                return new PagedResultModel<BillDetailListModel> { Data = new List<BillDetailListModel>(), TotalRecords = 0, TotalPages = 0 };
            }
            // Fetch bill items for the given UserId and (optional) BillId
            var dataList = await _billItem.GetAllBillingItemsAsync(userId, billId);
            var query = dataList.AsQueryable();

            if (!User.Identity.IsAuthenticated)
            {
                return new PagedResultModel<BillDetailListModel> { Data = new List<BillDetailListModel>(), TotalRecords = 0, TotalPages = 0 };
            }

          
            // Searching
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.FoodItemName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting
            query = sortOrder switch
            {
                "billId_asc" => query.OrderBy(b => b.BillId),
                "billId_desc" => query.OrderByDescending(b => b.BillId),
                "foodItemName_asc" => query.OrderBy(b => b.FoodItemName),
                "foodItemName_desc" => query.OrderByDescending(b => b.FoodItemName),
                "price_asc" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                "quantity_asc" => query.OrderBy(b => b.Quantity),
                "quantity_desc" => query.OrderByDescending(b => b.Quantity),
                "weightType_asc" => query.OrderBy(b => b.WeightType),
                "weightType_desc" => query.OrderByDescending(b => b.WeightType),
                "total_asc" => query.OrderBy(b => b.Total),
                "total_desc" => query.OrderByDescending(b => b.Total),
                _ => query.OrderBy(b => b.BillId) // Default sorting
            };

            // Pagination
            int totalRecords = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultModel<BillDetailListModel>
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }

    }
}

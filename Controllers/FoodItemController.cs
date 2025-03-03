using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBill_POS.Business_Data;
using QuickBill_POS.Implimentation;
using QuickBill_POS.Interface;
using System.Diagnostics;
using System.Reflection;

namespace QuickBill_POS.Controllers
{
    public class FoodItemController : Controller
    {
        private readonly IFoodItem _foodItem;
        private readonly IUser _user;

        public FoodItemController(IFoodItem foodItem, IUser user)
        {
            _foodItem = foodItem;
            _user = user;
        }

        //[Authorize]
        //public async Task<IActionResult> FoodItemList()
        //{
        //    var username = User.Identity.Name;
        //    var user = _user.GetUserByUsername(username);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var foodItems = await _foodItem.GetAllFoodItemsAsync(user.Id);
        //    return View(foodItems);
        //}

        [Authorize]
        public async Task<IActionResult> FoodItemList(string searchTerm = "", string sortOrder = "name_asc", int page = 1, int pageSize = 10)
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

            if (string.IsNullOrEmpty(searchTerm))
            {
                Debug.WriteLine("Search term is NULL or EMPTY!"); // Debug log
            }
            else
            {
                Debug.WriteLine($"Search term received: {searchTerm}");
            }

            var pageSizeOptions = new[] { 10, 20, 50 }
            .Select(size => $"<option value=\"{size}\" {(size == pageSize ? "selected" : "")} >{size}</option>")
            .ToList();
            ViewBag.PageSizeOptions = string.Join("", pageSizeOptions);

            var result = await GetFilteredSortedPagedData(user.Id, searchTerm, sortOrder, page, pageSize);

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.TotalPages;
           

            return View(result.Data);


        }


        [Authorize]
        public async Task<IActionResult> AddEditFoodItem(int? id)
        {
            var username = User.Identity.Name;
            var user = _user.GetUserByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id.HasValue && id > 0)
            {
                var foodItem = await _foodItem.GetFoodItemByIdAsync(id.Value, user.Id);
                if (foodItem == null)
                {
                    return NotFound();
                }
                return View(foodItem);
            }
            return View(new FoodItemModel()); // New Entry
        }

        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEditFoodItem(FoodItemModel foodItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var username = User.Identity.Name;
                    var user = _user.GetUserByUsername(username);
                    if (user == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    if (foodItem.Id > 0) // Edit Mode
                    {
                        await _foodItem.EditFoodItemAsync(foodItem, user.Id);
                        TempData["Success"] = "Food item updated successfully!";
                    }
                    else // Add Mode
                    {
                        await _foodItem.AddFoodItemAsync(foodItem, user.Id);
                        TempData["Success"] = "Food item added successfully!";
                    }
                    return RedirectToAction(nameof(FoodItemList));
                }

                TempData["Error"] = "Failed to save food item. Please check inputs.";
                return View(foodItem);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return View(foodItem);
            }
        }

        private async Task<PagedResultModel<FoodItemModel>> GetFilteredSortedPagedData(
     int userId, string searchTerm, string sortOrder, int page, int pageSize)
        {
            if (userId <= 0)
            {
                return new PagedResultModel<FoodItemModel> { Data = new List<FoodItemModel>(), TotalRecords = 0, TotalPages = 0 };
            }

            // Fetch food items for the given UserId
            var dataList = await _foodItem.GetAllFoodItemsAsync(userId);
            var query = dataList.AsQueryable();

            if (!User.Identity.IsAuthenticated)
            {
                return new PagedResultModel<FoodItemModel> { Data = new List<FoodItemModel>(), TotalRecords = 0, TotalPages = 0 };
            }

            // Searching
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting
            query = sortOrder switch
            {
                //"id_asc" => query.OrderBy(f => f.Id),
                //"id_desc" => query.OrderByDescending(f => f.Id),
                "name_asc" => query.OrderBy(f => f.Name),
                "name_desc" => query.OrderByDescending(f => f.Name),
                _ => query.OrderBy(f => f.Name) // Default sorting
            };

            // Pagination
            int totalRecords = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResultModel<FoodItemModel>
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }

    }
}

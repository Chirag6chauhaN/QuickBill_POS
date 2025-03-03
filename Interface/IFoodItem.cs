using QuickBill_POS.Business_Data;

namespace QuickBill_POS.Interface
{
    public interface IFoodItem
    {
        Task <List<FoodItemModel>> GetAllFoodItemsAsync(int userId);
        Task AddFoodItemAsync(FoodItemModel foodItem, int userId);
        Task EditFoodItemAsync(FoodItemModel foodItem, int userId);
        Task DeleteFoodItemAsync(int Id);
        Task <FoodItemModel> GetFoodItemByIdAsync(int Id, int userId);
    }
}

using QuickBill_POS.Business_Data;

namespace QuickBill_POS.Interface
{
    public interface IBillItem
    {
        Task<IEnumerable<BillDetailListModel>> GetAllBillingItemsAsync(int userId, string billId = null);
        Task<BillDetailModel> GetBillingItemByIdAsync(int id, int userId);
        Task<int> AddOrUpdateBillingItemAsync(BillDetailModel billing, int userId);

        Task<int> DeleteBillingItemAsync(int id);
        Task<List<FoodItemModel>> GetAllFoodItemsAsync(int userId);

         Task<string> GenerateNextBillNumberAsync();

        Task<int> FinalizeBillAsync(BillModel billHeader, List<BillDetailModel> billItems);

        Task<IEnumerable<BillModel>> GetBillsByDateRangeAsync(DateTime from, DateTime to, int userId);
    }
}

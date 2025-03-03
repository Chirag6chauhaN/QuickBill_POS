using QuickBill_POS.Business_Data;

namespace QuickBill_POS.Interface
{
    public interface IBill
    {
        Task AddBill(decimal grandTotal);
        Task AddBillDetail(int billId, int foodItemId, int quantity, decimal weight, decimal totalPrice);
        Task<IEnumerable<BillModel>> GetAllBillsAsync(int userId);
        decimal GetGrandTotal(int billId);
    }
}

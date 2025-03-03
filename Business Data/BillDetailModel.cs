using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QuickBill_POS.Business_Data
{
    public class BillDetailModel
    {
        public int Id { get; set; }
        public string BillId { get; set; }
        public int FoodItemId { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string WeightType { get; set; }
        public decimal Total { get; set; }
        public int UserId { get; set; }

    }
}

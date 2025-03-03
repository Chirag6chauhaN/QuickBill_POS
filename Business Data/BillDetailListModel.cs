﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QuickBill_POS.Business_Data
{
    public class BillDetailListModel
    {
        public int Id { get; set; }
        public string BillId { get; set; }
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string WeightType { get; set; }
        public decimal Total { get; set; }
    }
}

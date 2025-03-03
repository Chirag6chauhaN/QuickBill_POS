namespace QuickBill_POS.Business_Data
{
    public class HomeViewModel
    {
        // Today's summary
        public int TodaysBillCount { get; set; }
        public decimal TodaysTotalIncome { get; set; }

        // Filtered (date range) summary
        public int FilterBillCount { get; set; }
        public decimal FilterTotalIncome { get; set; }

        // Overall (all-time) summary
        public int OverallBillCount { get; set; }
        public decimal OverallTotalIncome { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

    }
}

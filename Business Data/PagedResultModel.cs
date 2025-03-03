namespace QuickBill_POS.Business_Data
{
    public class PagedResultModel<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}

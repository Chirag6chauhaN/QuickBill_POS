using System.ComponentModel.DataAnnotations;

namespace QuickBill_POS.Business_Data
{
    public class BillModel
    {
        [Required(ErrorMessage = "BillId is required.")]
        public string BillId { get; set; }

        [Required(ErrorMessage = "Bill date is required.")]
        public DateTime BillDate { get; set; }

        [Required(ErrorMessage = "GrandTotal is required.")]
        public decimal GrandTotal { get; set; }

        [Required]
        public int UserId { get;  set; }

        [Required(ErrorMessage = "BillDetails cannot be empty.")]
        public List<BillDetailModel> BillDetails { get; set; } = new List<BillDetailModel>();
    }
}

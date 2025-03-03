namespace QuickBill_POS.Business_Data
{
    public class User
    {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; } // Store as a hashed password
       
    }
}

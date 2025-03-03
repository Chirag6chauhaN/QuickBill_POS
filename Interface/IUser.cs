using QuickBill_POS.Business_Data;

namespace QuickBill_POS.Interface
{
    public interface IUser
    {
        User ValidateUser(string username, string password);
        User GetUserByUsername(string username);
    }
}

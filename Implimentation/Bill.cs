using Microsoft.Data.SqlClient;
using QuickBill_POS.Business_Data;
using QuickBill_POS.Interface;
using System.Data;

namespace QuickBill_POS.Implimentation
{
    public class Bill : IBill
    {
        private readonly string _connectionString;

        public Bill(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Task AddBill(decimal grandTotal)
        {
            throw new NotImplementedException();
        }

        public Task AddBillDetail(int billId, int foodItemId, int quantity, decimal weight, decimal totalPrice)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BillModel>> GetAllBillsAsync(int userId)
        {
            var bills = new List<BillModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("GetAllBills", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId); // Pass UserId

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var bill = new BillModel
                            {
                                BillId = reader["BillId"].ToString(),
                                BillDate = reader["BillDate"] != DBNull.Value ? Convert.ToDateTime(reader["BillDate"]) : DateTime.MinValue,
                                GrandTotal = reader["GrandTotal"] != DBNull.Value ? Convert.ToDecimal(reader["GrandTotal"]) : 0
                            };

                            bills.Add(bill);
                        }
                    }
                }
            }

            return bills;
        }


        public decimal GetGrandTotal(int billId)
        {
            throw new NotImplementedException();
        }
    }
}

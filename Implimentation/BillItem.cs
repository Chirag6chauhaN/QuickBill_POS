using Microsoft.Data.SqlClient;
using QuickBill_POS.Business_Data;
using QuickBill_POS.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace QuickBill_POS.Implimentation
{
    public class BillItem : IBillItem
    {
        public string _connectionString;

        public BillItem(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> AddOrUpdateBillingItemAsync(BillDetailModel billing, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("AddUpdateBillItem", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", billing.Id);

                        // If BillId is empty, generate a new one; otherwise, use the existing one.
                        string billId = string.IsNullOrWhiteSpace(billing.BillId)
                                        ? await GenerateNextBillNumberAsync()
                                        : billing.BillId;
                        command.Parameters.AddWithValue("@BillId", billId);
                        command.Parameters.AddWithValue("@FoodItemId", billing.FoodItemId);
                        command.Parameters.AddWithValue("@Price", billing.Price);
                        command.Parameters.AddWithValue("@Quantity", billing.Quantity);
                        command.Parameters.AddWithValue("@WeightType", billing.WeightType);
                        command.Parameters.AddWithValue("@Total", billing.Total);
                        command.Parameters.AddWithValue("@UserId", userId); // Add UserId

                        var result = await command.ExecuteNonQueryAsync();

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<int> DeleteBillingItemAsync(int id)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<BillDetailListModel>> GetAllBillingItemsAsync(int userId, string billId = null)
        {
            var billDetailsList = new List<BillDetailListModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetBillItemList", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@UserId", userId);
                    //command.Parameters.AddWithValue("@BillId", string.IsNullOrEmpty(billId) ? (object)DBNull.Value : billId);

                    // Pass the parameter if provided; otherwise, pass DBNull.Value.
                    if (!string.IsNullOrEmpty(billId))
                    {
                        command.Parameters.AddWithValue("@BillId", billId);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@BillId", DBNull.Value);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            billDetailsList.Add(new BillDetailListModel
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                                BillId = reader.IsDBNull(reader.GetOrdinal("BillId")) ? null : reader.GetString(reader.GetOrdinal("BillId")),
                                FoodItemId = reader.IsDBNull(reader.GetOrdinal("FoodItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("FoodItemId")),
                                FoodItemName = reader.IsDBNull(reader.GetOrdinal("FoodItemName")) ? null : reader.GetString(reader.GetOrdinal("FoodItemName")),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Quantity")),
                                WeightType = reader.IsDBNull(reader.GetOrdinal("WeightType")) ? null : reader.GetString(reader.GetOrdinal("WeightType")),
                                Total = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Total"))
                            });
                        }
                    }
                }
            }

            return billDetailsList;
        }



        public async Task<BillDetailModel> GetBillingItemByIdAsync(int id, int userId)
        {
            BillDetailModel billDetail = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Ensure you have a stored procedure named "GetBillItemById" that accepts @Id
                using (var command = new SqlCommand("GetBillItemById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            billDetail = new BillDetailModel
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                                BillId = reader.IsDBNull(reader.GetOrdinal("BillId")) ? null : reader.GetString(reader.GetOrdinal("BillId")),
                                FoodItemId = reader.IsDBNull(reader.GetOrdinal("FoodItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("FoodItemId")),
                                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Price")),
                                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Quantity")),
                                WeightType = reader.IsDBNull(reader.GetOrdinal("WeightType")) ? null : reader.GetString(reader.GetOrdinal("WeightType")),
                                Total = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Total")),

                            };
                        }
                    }
                }
            }
            return billDetail;
        }


        public async Task<List<FoodItemModel>> GetAllFoodItemsAsync(int userId)
        {
            var foodItems = new List<FoodItemModel>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("GetFoodItems", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserId", userId); // Filter by UserId

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                foodItems.Add(new FoodItemModel
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                });
                            }
                        }
                    }
                }
                return foodItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GenerateNextBillNumberAsync()
        {
            string billId = "";
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("GetNextBillNumber", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    billId = (await command.ExecuteScalarAsync())?.ToString();
                }
            }
            return billId;
        }

        public async Task<int> FinalizeBillAsync(BillModel billHeader, List<BillDetailModel> billItems)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var billCmd = new SqlCommand("InsertBillHeader", connection, transaction))
                        {
                            billCmd.CommandType = CommandType.StoredProcedure;
                            billCmd.Parameters.AddWithValue("@BillId", billHeader.BillId);
                            billCmd.Parameters.AddWithValue("@BillDate", billHeader.BillDate);
                            billCmd.Parameters.AddWithValue("@GrandTotal", billHeader.GrandTotal);
                            billCmd.Parameters.AddWithValue("@UserId", billHeader.UserId);
                            await billCmd.ExecuteNonQueryAsync();
                        }

                        foreach (var item in billItems)
                        {
                            if (item.UserId == 0)
                            {
                                item.UserId = billHeader.UserId; // ✅ Assign UserId if missing
                            }

                            using (var itemCmd = new SqlCommand("AddUpdateBillItem", connection, transaction))
                            {
                                itemCmd.CommandType = CommandType.StoredProcedure;
                                itemCmd.Parameters.AddWithValue("@BillId", billHeader.BillId);
                                itemCmd.Parameters.AddWithValue("@FoodItemId", item.FoodItemId);
                                itemCmd.Parameters.AddWithValue("@Price", item.Price);
                                itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                itemCmd.Parameters.AddWithValue("@WeightType", item.WeightType);
                                itemCmd.Parameters.AddWithValue("@Total", item.Total);
                                itemCmd.Parameters.AddWithValue("@UserId", item.UserId); // Add UserId
                                await itemCmd.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                        return 1;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error finalizing bill: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public async Task<IEnumerable<BillModel>> GetBillsByDateRangeAsync(DateTime from, DateTime to, int userId)
        {
            var bills = new List<BillModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetBillsByDateRange", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    // Add parameters for the date range
                    command.Parameters.AddWithValue("@DateFrom", from);
                    command.Parameters.AddWithValue("@DateTo", to);
                    command.Parameters.AddWithValue("@UserId", userId); // Pass UserId

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var bill = new BillModel
                            {
                                // Adjust property names as needed. Example:
                                BillId = reader["BillId"].ToString(),
                                BillDate = reader["BillDate"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["BillDate"])
                                            : DateTime.MinValue,
                                GrandTotal = reader["Total"] != DBNull.Value
                                            ? Convert.ToDecimal(reader["Total"])
                                            : 0
                                // If you have other properties, fill them here.
                            };

                            bills.Add(bill);
                        }
                    }
                }
            }

            return bills;
        }

    }
}

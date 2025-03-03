using Microsoft.Data.SqlClient;
using QuickBill_POS.Business_Data;
using QuickBill_POS.Interface;
using System.Data;

namespace QuickBill_POS.Implimentation
{
    public class FoodItem : IFoodItem
    {
        public string _connectionString;

        
        public FoodItem(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task AddFoodItemAsync(FoodItemModel foodItem, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("AddOrUpdateFoodItem", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", foodItem.Id);
                        command.Parameters.AddWithValue("@Name", foodItem.Name);
                        command.Parameters.AddWithValue("@UserId", userId); // Add UserId

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
           
        }

        public Task DeleteFoodItemAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async Task EditFoodItemAsync(FoodItemModel foodItem, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("AddOrUpdateFoodItem", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Id", foodItem.Id);
                        command.Parameters.AddWithValue("@Name", foodItem.Name);
                        command.Parameters.AddWithValue("@UserId", userId); // Add UserId

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
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


        public async Task<FoodItemModel> GetFoodItemByIdAsync(int Id, int userId)
        {
            FoodItemModel foodItem = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("GetFoodItemById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Id", Id);
                        command.Parameters.AddWithValue("@UserId", userId); // Filter by UserId

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                foodItem = new FoodItemModel
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            return foodItem;
        }

    }
}

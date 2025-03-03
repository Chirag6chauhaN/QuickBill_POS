using Microsoft.Data.SqlClient;
using QuickBill_POS.Interface;
using QuickBill_POS.Business_Data; // Assuming User model is in Business_Data namespace
using Dapper;
using BCrypt.Net;

namespace QuickBill_POS.Implimentation
{
    public class User : IUser
    {
        private readonly string _connectionString;

        public User(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Business_Data.User GetUserByUsername(string username)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = "SELECT * FROM Users WHERE Username = @Username";
                var user = connection.QueryFirstOrDefault<Business_Data.User>(query, new { Username = username });

                return user; // Return user if found, otherwise null
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[Database Error] {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Unexpected Error] {ex.Message}");
                return null;
            }
        }


        public Business_Data.User ValidateUser(string username, string password)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // Fetch user by username
                var query = "SELECT * FROM Users WHERE Username = @Username";
                var user = connection.QueryFirstOrDefault<Business_Data.User>(query, new { Username = username });

                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return null;
                }

                // Check plain-text password
                if (user.Password != password)
                {
                    Console.WriteLine("Invalid password.");
                    return null;
                }

                return user; // Login successful
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[Database Error] {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Unexpected Error] {ex.Message}");
                return null;
            }
        }


    }
}

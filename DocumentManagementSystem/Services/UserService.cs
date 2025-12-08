using DocumentManagementSystem.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DocumentManagementSystem.Services
{
    public class UserService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IConfiguration _configuration;
        private UserModel _user = new();

        public class UserModel
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string PasswordHash { get; set; }
            public string Role { get; set; }
        }
        public UserService(DatabaseConnection dbConnection, IConfiguration configuration)
        {
            _dbConnection = dbConnection;
            _configuration = configuration;
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                var sql = "SELECT \"UserId\", \"Username\", \"PasswordHash\", \"Role\" FROM \"DocMS\".\"Users\"";
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>());
                if (results.Count > 0)
                {
                    var row = results[0];
                    _user = new UserModel
                    {
                        UserID = row["UserId"] != null ? Convert.ToInt32(row["UserId"]) : 0,
                        Username = row["Username"]?.ToString() ?? string.Empty,
                        PasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty,
                        Role = row["Role"]?.ToString() ?? string.Empty
                    };
                }

            }
            catch
            {
                _user = new UserModel();
                throw;
            }
        }

        public UserModel GetUser() => _user;

        public async Task<bool> CreateUserAsync(string username, string password, string role)
        {
            try
            {
                // Hash password before saving
                string passwordHash = HashPassword(password);

                var sql = "INSERT INTO \"DocMS\".\"Users\" (\"Username\", \"PasswordHash\", \"Role\") VALUES (@username, @passwordHash, @role)";
                var parameters = new[]
                {
                    new NpgsqlParameter("@username", username),
                    new NpgsqlParameter("@passwordHash", passwordHash),
                    new NpgsqlParameter("@role", role)
                };

                int rowsAffected = await _dbConnection.ExecuteNonQueryAsync(sql, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error creating user: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        public async Task<bool> Login(string username, string password)
        {
            try
            {
                // First get user by username
                var sql = "SELECT \"UserId\", \"Username\", \"PasswordHash\", \"Role\" FROM \"DocMS\".\"Users\" WHERE \"Username\" = @username";
                var parameters = new[]
                {
                    new NpgsqlParameter("@username", username)
                };
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>(), parameters);

                if (results.Count > 0)
                {
                    var row = results[0];
                    var storedPasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty;

                    // Verify password using BCrypt
                    if (VerifyPassword(password, storedPasswordHash))
                    {
                        _user = new UserModel
                        {
                            UserID = row["UserId"] != null ? Convert.ToInt32(row["UserId"]) : 0,
                            Username = row["Username"]?.ToString() ?? string.Empty,
                            PasswordHash = storedPasswordHash,
                            Role = row["Role"]?.ToString() ?? string.Empty
                        };
                        return true;
                    }
                }

                _user = new UserModel();
                return false;
            }
            catch
            {
                _user = new UserModel();
                return false;
            }
        }

        public async Task<UserModel?> GetCurrentUserAsync()
        {
            try
            {
                if (_user != null && !string.IsNullOrEmpty(_user.Username))
                {
                    return _user;
                }
                await LoadUsersAsync();
                return _user;
            }
            catch
            {
                return null;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        // Get all users
        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                var sql = "SELECT \"UserId\", \"Username\", \"PasswordHash\", \"Role\" FROM \"DocMS\".\"Users\" ORDER BY \"Username\"";
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>());
                
                var users = new List<UserModel>();
                foreach (var row in results)
                {
                    users.Add(new UserModel
                    {
                        UserID = row["UserId"] != null ? Convert.ToInt32(row["UserId"]) : 0,
                        Username = row["Username"]?.ToString() ?? string.Empty,
                        PasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty,
                        Role = row["Role"]?.ToString() ?? string.Empty
                    });
                }
                return users;
            }
            catch
            {
                return new List<UserModel>();
            }
        }

        // Get user by ID
        public async Task<UserModel?> GetUserByIdAsync(int userId)
        {
            try
            {
                var sql = "SELECT \"UserId\", \"Username\", \"PasswordHash\", \"Role\" FROM \"DocMS\".\"Users\" WHERE \"UserId\" = @userId";
                var parameters = new[]
                {
                    new NpgsqlParameter("@userId", userId)
                };
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>(), parameters);
                
                if (results.Count > 0)
                {
                    var row = results[0];
                    return new UserModel
                    {
                        UserID = row["UserId"] != null ? Convert.ToInt32(row["UserId"]) : 0,
                        Username = row["Username"]?.ToString() ?? string.Empty,
                        PasswordHash = row["PasswordHash"]?.ToString() ?? string.Empty,
                        Role = row["Role"]?.ToString() ?? string.Empty
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // Check if username already exists
        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            try
            {
                var sql = "SELECT COUNT(*) FROM \"DocMS\".\"Users\" WHERE \"Username\" = @username";
                if (excludeUserId.HasValue)
                {
                    sql += " AND \"UserId\" != @excludeUserId";
                }
                
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@username", username)
                };
                
                if (excludeUserId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@excludeUserId", excludeUserId.Value));
                }
                
                var result = await _dbConnection.ExecuteScalarAsync(sql, parameters.ToArray());
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Update existing user
        public async Task<bool> UpdateUserAsync(int userId, string username, string? password, string role)
        {
            try
            {
                int rowsAffected;
                // If password is provided, hash it and update
                if (!string.IsNullOrWhiteSpace(password))
                {
                    var sql = "UPDATE \"DocMS\".\"Users\" SET \"Username\" = @username, \"PasswordHash\" = @passwordHash, \"Role\" = @role WHERE \"UserId\" = @userId";
                    var passwordHash = HashPassword(password);
                    var parameters = new[]
                    {
                        new NpgsqlParameter("@username", username),
                        new NpgsqlParameter("@passwordHash", passwordHash),
                        new NpgsqlParameter("@role", role),
                        new NpgsqlParameter("@userId", userId)
                    };
                    rowsAffected = await _dbConnection.ExecuteNonQueryAsync(sql, parameters);
                }
                else
                {
                    // If no password provided, only update username and role
                    var sql = "UPDATE \"DocMS\".\"Users\" SET \"Username\" = @username, \"Role\" = @role WHERE \"UserId\" = @userId";
                    var parameters = new[]
                    {
                        new NpgsqlParameter("@username", username),
                        new NpgsqlParameter("@role", role),
                        new NpgsqlParameter("@userId", userId)
                    };
                    rowsAffected = await _dbConnection.ExecuteNonQueryAsync(sql, parameters);
                }
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error updating user: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var sql = "DELETE FROM \"DocMS\".\"Users\" WHERE \"UserId\" = @userId";
                var parameters = new[]
                {
                    new NpgsqlParameter("@userId", userId)
                };
                await _dbConnection.ExecuteNonQueryAsync(sql, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
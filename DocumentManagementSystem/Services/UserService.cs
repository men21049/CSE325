namespace DocumentManagementSystem.Services
{
    public class UserService
    {
        private readonly Dictionary<string, string> Users = new()
        {
            { "admin", "admin123" },
            { "user", "user123" }
        };

        public bool Login(string username, string password)
        {
            return Users.ContainsKey(username) && Users[username] == password;
        }
    }
}
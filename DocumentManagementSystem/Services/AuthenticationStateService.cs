using DocumentManagementSystem.Services;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagementSystem.Services
{
    public class AuthenticationStateService
    {

        private readonly ConcurrentDictionary<string, UserService.UserModel> _authenticatedUsers = new();
        

        private UserService.UserModel? _currentUser = null;
        private readonly IServiceProvider _serviceProvider;

        public AuthenticationStateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Helper method to get UserService from the service provider
        private UserService GetUserService()
        {
            // Create a scope to get a Scoped service
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<UserService>();
        }

        // Check if user is authenticated
        public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentUser.Username);

        // Get current user
        public UserService.UserModel? CurrentUser => _currentUser;

        // Get current username
        public string? CurrentUsername => _currentUser?.Username;

        // Get current user role
        public string? CurrentUserRole => _currentUser?.Role;

        // Login method
        public async Task<bool> LoginAsync(string username, string password)
        {
            var userService = GetUserService();
            bool success = await userService.Login(username, password);
            if (success)
            {

                var user = userService.GetUser();
                if (user != null && !string.IsNullOrEmpty(user.Username))
                {
       
                    _authenticatedUsers.AddOrUpdate(username, user, (key, oldValue) => user);
                    _currentUser = user;
                }
                else
                {
                    throw new Exception("Login succeeded but user is null or empty!");
                }
            }
            return success;
        }

        // Logout method
        public void Logout()
        {
            if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Username))
            {
                _authenticatedUsers.TryRemove(_currentUser.Username, out _);
            }
            _currentUser = null;
        }

        // Initialize user from stored state
        // This method restores the user from the dictionary when navigating between pages
        public async Task InitializeAsync()
        {
            
            if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Username))
            {
                return;
            }

            
            // This handles the case where we navigate between pages and a new instance is created
            if (_authenticatedUsers.Count > 0)
            {

                var storedUser = _authenticatedUsers.Values.FirstOrDefault();
                if (storedUser != null)
                {
                    _currentUser = storedUser;
                    return;
                }
            }

            
            var userService = GetUserService();
            var userFromService = userService.GetUser();
            if (userFromService != null && !string.IsNullOrEmpty(userFromService.Username))
            {
            
                _currentUser = userFromService;
                _authenticatedUsers.AddOrUpdate(userFromService.Username, userFromService, (key, oldValue) => userFromService);
            
                return;
            }

        }
    }
}
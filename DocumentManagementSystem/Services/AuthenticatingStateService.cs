using DocumentManagementSystem.Services;

namespace DocumentManagementSystem.Services
{
    public class AuthenticationStateService
    {
        private UserService.UserModel? _currentUser = null;
        private readonly UserService _userService;

        public AuthenticationStateService(UserService userService)
        {
            _userService = userService;
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
            bool success = await _userService.Login(username, password);
            if (success)
            {
                _currentUser = await _userService.GetCurrentUserAsync();
            }
            return success;
        }

        // Logout method
        public void Logout()
        {
            _currentUser = null;
        }

        // Initialize user from UserService (if already logged in)
        public async Task InitializeAsync()
        {
            if (_currentUser == null)
            {
                _currentUser = await _userService.GetCurrentUserAsync();
            }
        }
    }
}
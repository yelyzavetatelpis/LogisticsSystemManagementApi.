namespace LogisticsSystemManagementApi.DTOs
{
    // Data sent by the user when logging in
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Data sent by the user from registering
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; } = 4; // registering as Customer by default
    }

    // Data returned to the user after successful login or registration
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
namespace LogisticsSystemManagementApi.DTOs
{
    //what user sent when logging in
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    //  registration form information filled
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; } = 4; // defaults to customer
        public string LicenseNumber { get; set; }
    }

    // send back to the frontend after login is successful
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}



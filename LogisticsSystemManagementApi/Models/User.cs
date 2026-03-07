namespace LogisticsSystemManagementApi.Models
{
    public class User // Represents a user in the system
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public int RoleId { get; set; }
    }

    public class Role // Represents a user role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
namespace LogisticsSystemManagementApi.DTOs
{
    public class AccountsDto
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string? Password { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;



    }
    public class DriverOverviewDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public string AvailabilityStatus { get; set; }
    }
}



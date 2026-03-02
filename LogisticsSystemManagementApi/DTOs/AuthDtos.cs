namespace LogisticsSystemManagementApi.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; } = 4;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class CreateOrderDto
    {
        public string PickupStreet { get; set; }
        public string PickupCity { get; set; }
        public string PickupPostalCode { get; set; }

        public string DeliveryStreet { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryPostalCode { get; set; }

        public decimal PackageWeight { get; set; }
        public string? OrderDescription { get; set; }
        public DateTime PickupDate { get; set; }

        public int OrderStatusId { get; set; }
    }
}
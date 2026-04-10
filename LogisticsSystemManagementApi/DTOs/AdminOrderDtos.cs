namespace LogisticsSystemManagementApi.DTOs
{
    public class AdminOrderResponseDto
    {
        public int OrderId { get; set; }
        public string Email { get; set; }
        public int CustomerId { get; set; }
        public string PickupCity { get; set; } = string.Empty;


        public string PickupStreet { get; set; } = string.Empty;
        public string PickupPostalCode { get; set; } = string.Empty;
        public string DeliveryStreet { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public decimal PackageWeight { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public DateTime PickupDate { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    public class AdminOrderFilterDto
    {
        public string? Email { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}






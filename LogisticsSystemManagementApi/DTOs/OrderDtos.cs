namespace LogisticsSystemManagementApi.DTOs
{
    // Data sent by customer when creating a new order
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
        public decimal Price { get; set; }
    }


    // Order in a list, used in dashboard and order history
    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string PickupCity { get; set; }
        public string DeliveryCity { get; set; }
        public decimal PackageWeight { get; set; }
        public string StatusName { get; set; }
        public DateTime PickupDate { get; set; }
    }


    // Full order details shown on the My Orders page
    public class MyOrdersDto
    {
        public int OrderId { get; set; }
        public string PickupStreet { get; set; }
        public string PickupCity { get; set; }
        public string PickupPostalCode { get; set; }
        public string DeliveryStreet { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryPostalCode { get; set; }
        public decimal PackageWeight { get; set; }
        public string? OrderDescription { get; set; }
        public DateTime PickupDate { get; set; }
        public string StatusName { get; set; }
        public string AdditionalNotes { get; set; }


        public DateTime CreatedAt { get; set; }
    }


    // Message sent by dispatcher when rejecting an order
    public class RejectOrderDto
    {
        public string Reason { get; set; }
    }
    public class UpdateShipmentStatusResult
    {
        public int ShipmentId { get; set; }
        public int ShipmentStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int? TripId { get; set; }
        public string? TripStatusName { get; set; }
    }


    public class UpdateShipmentStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}








namespace LogisticsSystemManagementApi.Models
{
    //  Order placed by a customer
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }


        // Pickup location
        public string PickupStreet { get; set; }
        public string PickupCity { get; set; }
        public string PickupPostalCode { get; set; }


        // Delivery location
        public string DeliveryStreet { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryPostalCode { get; set; }


        //Additional details about the order
        public decimal PackageWeight { get; set; }
        public string? OrderDescription { get; set; }
        public DateTime PickupDate { get; set; }


        public int OrderStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Price { get; set; }


    }
}








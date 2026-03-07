namespace LogisticsSystemManagementApi.Models
{
    // Shipment created from an order by a dispatcher
    public class Shipment
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; } // links to the order the shipment is created from
        public int ShipmentStatusId { get; set; } // links to the ShipmentStatuses table
        public DateTime CreatedAt { get; set; } // set when the shipment is created
    }
}
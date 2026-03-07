namespace LogisticsSystemManagementApi.Models
{
    // Status of a shipment (e.g. Pending, In Transit, Delivered)
    public class ShipmentStatus
    {
        public int ShipmentStatusId { get; set; }
        public string StatusName { get; set; }
    }
}
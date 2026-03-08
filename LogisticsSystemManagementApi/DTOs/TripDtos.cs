public class CreateTripDto
{
    public int DriverId { get; set; }

    public int VehicleId { get; set; }

    public List<int> ShipmentIds { get; set; }
}
public class TripDto
{
    public int TripId { get; set; }

    public int DriverId { get; set; }

    public int VehicleId { get; set; }

    public string StatusName { get; set; }

    public DateTime PlannedDeparture { get; set; }
}
public class TripShipmentDto
{
    public int ShipmentId { get; set; }

    public string StatusName { get; set; }

    public DateTime CreatedAt { get; set; }


    public int OrderId { get; set; }

    public string PickupCity { get; set; }

    public string DeliveryCity { get; set; }

    public decimal PackageWeight { get; set; }
}
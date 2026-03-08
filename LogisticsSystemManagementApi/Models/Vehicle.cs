namespace LogisticsSystemManagementApi.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }

        public string RegistrationNumber { get; set; }

        public decimal Capacity { get; set; }

        public int VehicleAvailabilityStatusId { get; set; }
    }
}
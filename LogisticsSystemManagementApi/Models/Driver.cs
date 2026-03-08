namespace LogisticsSystemManagementApi.Models
{
    public class Driver
    {
        public int DriverId { get; set; }

        public int UserId { get; set; }

        public string LicenseNumber { get; set; }

        public int DriverAvailabilityStatusId { get; set; }
    }
}
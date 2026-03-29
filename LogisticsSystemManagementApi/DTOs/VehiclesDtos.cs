namespace LogisticsSystemManagementApi.DTOs
{
    public class CreateVehicleDto
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleAvailabilityStatusId { get; set; }
    }

    public class VehicleResponseDto
    {
        public int VehicleId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string VehicleModel { get; set; } = string.Empty;
        public int VehicleAvailabilityStatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    public class UpdateVehicleStatusDto
    {
        public int VehicleId { get; set; }
        public int VehicleAvailabilityStatusId { get; set; }
    }
}



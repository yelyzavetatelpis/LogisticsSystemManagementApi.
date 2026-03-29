namespace LogisticsSystemManagementApi.DTOs
{
    public class DriverAvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public DriverAvailabilityDaysDto AvailableDays { get; set; } = new();
        public List<string> SpecificDates { get; set; } = new();
    }

    public class DriverAvailabilityDaysDto
    {
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }


    public class SaveDriverAvailabilityRequest
    {
        public bool IsAvailable { get; set; }
        public DriverAvailabilityDaysDto AvailableDays { get; set; } = new();
        public List<string> SpecificDates { get; set; } = new();
    }
}




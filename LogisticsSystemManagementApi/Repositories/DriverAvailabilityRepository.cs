using System.Data;
using System.Text.Json;
using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;

public class DriverAvailabilityRepository
{
    private readonly DbContext _context;
    private const int AvailableStatusId = 1;
    private const int NotAvailableStatusId = 2;
    private static readonly string[] DayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    public DriverAvailabilityRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<DriverAvailabilityDto?> GetByDriverIdAsync(int driverId)
    {
        var query = @"
            SELECT 
                da.DriverAvailabilityStatusId,
                da.AvailableDays,
                da.SpecificDates
            FROM DriverAvailability da
            WHERE da.DriverId = @DriverId";

        using (var connection = _context.CreateConnection())
        {
            var row = await connection.QueryFirstOrDefaultAsync<DriverAvailabilityRow>(query, new { DriverId = driverId });
            if (row == null)
                return GetDefaultAvailability();

            return MapToDto(row);
        }
    }

    public async Task<DriverAvailabilityDto> UpsertAsync(int driverId, SaveDriverAvailabilityRequest request)
    {
        var statusId = request.IsAvailable ? AvailableStatusId : NotAvailableStatusId;
        var availableDaysStr = SerializeAvailableDays(request.AvailableDays);
        var specificDatesJson = JsonSerializer.Serialize(request.SpecificDates ?? new List<string>());

        var upsertQuery = @"
            MERGE dbo.DriverAvailability AS target
            USING (SELECT @DriverId AS DriverId) AS source
            ON target.DriverId = source.DriverId
            WHEN MATCHED THEN
                UPDATE SET
                    DriverAvailabilityStatusId = @StatusId,
                    AvailableDays = @AvailableDays,
                    SpecificDates = @SpecificDates,
                    UpdatedAt = GETUTCDATE()
            WHEN NOT MATCHED BY TARGET THEN
                INSERT (DriverId, DriverAvailabilityStatusId, AvailableDays, SpecificDates, UpdatedAt)
                VALUES (@DriverId, @StatusId, @AvailableDays, @SpecificDates, GETUTCDATE());";

        using (var connection = _context.CreateConnection())
        {
            await connection.ExecuteAsync(upsertQuery, new
            {
                DriverId = driverId,
                StatusId = statusId,
                AvailableDays = availableDaysStr,
                SpecificDates = specificDatesJson
            });
        }

        return new DriverAvailabilityDto
        {
            IsAvailable = request.IsAvailable,
            AvailableDays = request.AvailableDays ?? new DriverAvailabilityDaysDto(),
            SpecificDates = request.SpecificDates ?? new List<string>()
        };
    }

    private static DriverAvailabilityDto GetDefaultAvailability()
    {
        return new DriverAvailabilityDto
        {
            IsAvailable = true,
            AvailableDays = new DriverAvailabilityDaysDto(),
            SpecificDates = new List<string>()
        };
    }

    private static string SerializeAvailableDays(DriverAvailabilityDaysDto? days)
    {
        if (days == null) return string.Empty;
        var selected = DayNames.Where(d =>
            d.ToLowerInvariant() switch
            {
                "monday" => days.Monday,
                "tuesday" => days.Tuesday,
                "wednesday" => days.Wednesday,
                "thursday" => days.Thursday,
                "friday" => days.Friday,
                "saturday" => days.Saturday,
                "sunday" => days.Sunday,
                _ => false
            }).ToList();
        return string.Join(",", selected);
    }

    private static DriverAvailabilityDto MapToDto(DriverAvailabilityRow row)
    {
        var days = ParseAvailableDays(row.AvailableDays);
        var specificDates = ParseSpecificDates(row.SpecificDates);
        return new DriverAvailabilityDto
        {
            IsAvailable = row.DriverAvailabilityStatusId == AvailableStatusId,
            AvailableDays = days,
            SpecificDates = specificDates
        };
    }

    private static DriverAvailabilityDaysDto ParseAvailableDays(string? availableDaysStr)
    {
        var dto = new DriverAvailabilityDaysDto();
        if (string.IsNullOrWhiteSpace(availableDaysStr)) return dto;
        var parts = availableDaysStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var day in parts)
        {
            switch (day.ToLowerInvariant())
            {
                case "monday": dto.Monday = true; break;
                case "tuesday": dto.Tuesday = true; break;
                case "wednesday": dto.Wednesday = true; break;
                case "thursday": dto.Thursday = true; break;
                case "friday": dto.Friday = true; break;
                case "saturday": dto.Saturday = true; break;
                case "sunday": dto.Sunday = true; break;
            }
        }
        return dto;
    }

    private static List<string> ParseSpecificDates(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(json);
            return list ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private class DriverAvailabilityRow
    {
        public int DriverAvailabilityStatusId { get; set; }
        public string? AvailableDays { get; set; }
        public string? SpecificDates { get; set; }
    }
}



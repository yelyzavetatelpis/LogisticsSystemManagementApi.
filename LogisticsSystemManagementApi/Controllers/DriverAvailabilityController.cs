using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LogisticsSystemManagementApi.DTOs;

namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/driveravailability")]
    [Authorize]
    public class DriverAvailabilityController : ControllerBase
    {
        private readonly DriverAvailabilityRepository _repository;

        public DriverAvailabilityController(DriverAvailabilityRepository repository)
        {
            _repository = repository;
        }


        [HttpGet]
        public async Task<IActionResult> GetMyAvailability()
        {
            var driverId = GetCurrentDriverId();
            if (driverId == null)
                return Unauthorized(new { message = "Driver identifier not found." });

            var availability = await _repository.GetByDriverIdAsync(driverId.Value);
            return Ok(availability);
        }

        [HttpPut]
        public async Task<IActionResult> SaveMyAvailability([FromBody] SaveDriverAvailabilityRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required." });

            var driverId = GetCurrentDriverId();
            if (driverId == null)
                return Unauthorized(new { message = "Driver identifier not found." });

            var result = await _repository.UpsertAsync(driverId.Value, request);
            return Ok(result);
        }


        private int? GetCurrentDriverId()
        {

            var claim =
                User.FindFirst("driverId") ??
                User.FindFirst("userId") ??
                User.FindFirst("sub") ??
                User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;
            if (!int.TryParse(claim.Value, out var id))
                return null;
            return id;
        }
    }
}



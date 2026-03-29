using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleRepository _repository;

        public VehiclesController(IVehicleRepository repository)
        {
            _repository = repository;
        }

        // Get all vehicles
        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var vehicles = await _repository.GetVehiclesAsync();
            return Ok(vehicles);
        }

        //  Create vehicle
        [HttpPost]
        public async Task<IActionResult> CreateVehicle(CreateVehicleDto dto)
        {
            // Check duplicate registration
            var exists = await _repository.IsRegistrationExists(dto.RegistrationNumber);
            if (exists)
                return BadRequest("Registration number already exists");

            var vehicleId = await _repository.CreateVehicleAsync(dto);

            return Ok(new { message = "Vehicle created successfully", vehicleId });
        }

        //Update vehicle
        [HttpPut("status")]
        public async Task<IActionResult> UpdateVehicleStatus(UpdateVehicleStatusDto dto)
        {
            var updated = await _repository.UpdateVehicleStatusAsync(dto.VehicleId, dto.VehicleAvailabilityStatusId);

            if (!updated)
                return NotFound("Vehicle not found");

            return Ok(new { message = "Vehicle status updated successfully" });
        }
    }
}









using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripRepository _repository;


        public TripsController(ITripRepository repository)
        {
            _repository = repository;
        }


        // drivers see only their own trips, dispatchers and admins see all
        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (userRole == "Driver" && userIdClaim != null)
            {
                int userId = int.Parse(userIdClaim);
                var driverTrips = await _repository.GetTripsByDriverAsync(userId);
                return Ok(driverTrips);
            }


            var trips = await _repository.GetTripsAsync();
            return Ok(trips);
        }


        // get all shipments assigned to a specific trip
        [HttpGet("{tripId}/shipments")]
        public async Task<IActionResult> GetTripShipments(int tripId)
        {
            var shipments = await _repository.GetTripShipments(tripId);
            return Ok(shipments);
        }


        // start a planned trip
        [HttpPatch("{tripId}/start")]
        public async Task<IActionResult> StartTrip(int tripId)
        {
            var trip = await _repository.StartTripAsync(tripId);
            if (trip == null)
                return NotFound(new { message = "Trip not found." });
            return Ok(trip);
        }


        // get all currently available drivers
        [HttpGet("availableDrivers")]
        public async Task<IActionResult> GetAvailableDrivers()
        {
            var drivers = await _repository.GetAvailableDrivers();
            return Ok(drivers);
        }


        // get all currently available vehicles
        [HttpGet("availableVehicles")]
        public async Task<IActionResult> GetAvailableVehicles()
        {
            var vehicles = await _repository.GetAvailableVehicles();
            return Ok(vehicles);
        }


        // get drivers available on a specific date
        [HttpGet("availableDriversByDate")]
        public async Task<IActionResult> GetAvailableDriversByDate(DateTime tripDate)
        {
            var drivers = await _repository.GetAvailableDriversByDate(tripDate);
            return Ok(drivers);
        }


        // create a new trip and assign shipments, driver and vehicle
        [HttpPost("createTrip")]
        public async Task<IActionResult> CreateTrip(CreateTripDto dto)
        {
            var tripId = await _repository.CreateTripAsync(dto);
            return Ok(new { tripId });
        }


        // update a shipment status and sync the order status
        [HttpPatch("shipments/{shipmentId}/status")]
        public async Task<IActionResult> UpdateShipmentStatus(int shipmentId, [FromBody] UpdateShipmentStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { message = "Status is required." });


            var result = await _repository.UpdateShipmentStatusAsync(shipmentId, request.Status.Trim());
            if (result == null)
                return NotFound(new { message = "Shipment not found or invalid status." });


            return Ok(result);
        }
    }
}




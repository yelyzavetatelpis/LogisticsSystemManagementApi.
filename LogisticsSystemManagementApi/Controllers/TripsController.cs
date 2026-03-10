using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/trips")]
    public class TripsController : ControllerBase
    {
        private readonly TripRepository _repository;

        public TripsController(TripRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _repository.GetTripsAsync();

            return Ok(trips);
        }

        [HttpGet("{tripId}/shipments")]
        public async Task<IActionResult> GetTripShipments(int tripId)
        {
            var shipments = await _repository.GetTripShipments(tripId);

            return Ok(shipments);
        }

        [HttpPost("{tripId}/start")]
        public async Task<IActionResult> StartTrip(int tripId)
        {
            var trip = await _repository.StartTripAsync(tripId);

            if (trip == null)
                return NotFound(new { message = "Trip not found or cannot be started (already in progress, completed, or cancelled)." });

            return Ok(trip);
        }
    }
}
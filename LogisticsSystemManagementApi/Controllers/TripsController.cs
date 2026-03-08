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
    }
}
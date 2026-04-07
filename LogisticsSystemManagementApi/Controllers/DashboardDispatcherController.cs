using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Dispatcher")]
    public class DashboardDispatcherController : ControllerBase
    {
        private readonly IDispatcherRepository _repository;


        public DashboardDispatcherController(IDispatcherRepository repository)
        {
            _repository = repository;
        }


        // get dispatcher dashboard information
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _repository.GetDashboardData();
            return Ok(data);
        }

        // get trips 
        [HttpGet("trips")]
        public async Task<IActionResult> GetTripsByDate(DateTime fromDate, DateTime toDate)
        {
            var data = await _repository.GetTripsByDate(fromDate, toDate);
            return Ok(data);
        }
    }
}




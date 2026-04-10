using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _repo;


        public DashboardController(IDashboardRepository repo)
        {
            _repo = repo;
        }


        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var data = await _repo.GetDashboardMetricsAsync();
            return Ok(data);
        }
    }
}






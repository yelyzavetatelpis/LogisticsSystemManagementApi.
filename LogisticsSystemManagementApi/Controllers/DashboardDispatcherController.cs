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
        private readonly IOrderRepository _repository;

        public DashboardDispatcherController(IOrderRepository repository)
        {
            _repository = repository;
        }

        //  dashboard summary data for dispatcher
        [HttpGet("dispatcher")]
        public async Task<IActionResult> GetDispatcherDashboard()
        {
            var orders = await _repository.GetPendingOrdersAsync();
            return Ok(orders);
        }
    }
}
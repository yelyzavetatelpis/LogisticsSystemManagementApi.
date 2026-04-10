using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderRepository _repository;


        public AdminOrdersController(IAdminOrderRepository repository)
        {
            _repository = repository;
        }


        // get all orders in the system
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _repository.GetAllOrdersAsync();
            return Ok(orders);
        }


        // get orders filtered by email and date 
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredOrders([FromBody] AdminOrderFilterDto filter)
        {
            var result = await _repository.GetFilteredOrdersAsync(filter);
            return Ok(result);
        }
    }
}




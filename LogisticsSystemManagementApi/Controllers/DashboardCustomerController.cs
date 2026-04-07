using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class DashboardCustomerController : ControllerBase
    {
        private readonly ICustomerDashboardRepository _repository;


        public DashboardCustomerController(ICustomerDashboardRepository repository)
        {
            _repository = repository;
        }


        // dashboard data for the customer
        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            // get user id from the token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();


            int userId = int.Parse(userIdClaim);
            int customerId = await _repository.GetCustomerIdByUserIdAsync(userId);
            var dashboardData = await _repository.GetCustomerDashboardDataAsync(customerId);
            return Ok(dashboardData);
        }
    }
}




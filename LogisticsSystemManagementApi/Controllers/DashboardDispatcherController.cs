using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogisticsSystemManagementApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Dispatcher")]
    public class DashboardDispatcherController : ControllerBase
    {
        private readonly DispatcherRepository _repository;

        public DashboardDispatcherController(DispatcherRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _repository.GetDashboardData();
            return Ok(data);
        }
    }
}
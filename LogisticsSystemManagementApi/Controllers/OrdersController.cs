using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using Dapper;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository _repository;

        public OrdersController(OrderRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("createOrder")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);


            int customerId;

           
                var query = "SELECT CustomerId FROM Customers WHERE UserId = @UserId";
            customerId = await _repository.GetCustomerIdByUserId(userId);
            


            var order = new Order
            {
                CustomerId = customerId,
                PickupStreet = dto.PickupStreet,
                PickupCity = dto.PickupCity,
                PickupPostalCode = dto.PickupPostalCode,
                DeliveryStreet = dto.DeliveryStreet,
                DeliveryCity = dto.DeliveryCity,
                DeliveryPostalCode = dto.DeliveryPostalCode,
                PackageWeight = dto.PackageWeight,
                OrderDescription = dto.OrderDescription,
                PickupDate = dto.PickupDate,
                OrderStatusId = 7,
                CreatedAt = DateTime.Now
            };

            await _repository.CreateOrderAsync(order);

            return Ok(new { message = "Order created successfully" });
        }
        
        
    }
}
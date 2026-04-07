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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerDashboardRepository _dashboardRepository;


        public OrdersController(IOrderRepository orderRepository, ICustomerDashboardRepository dashboardRepository)
        {
            _orderRepository = orderRepository;
            _dashboardRepository = dashboardRepository;
        }


        // create a new order for customer
        [HttpPost("createOrder")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });


            int userId = int.Parse(userIdClaim);
            int customerId = await _dashboardRepository.GetCustomerIdByUserIdAsync(userId);


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
                CreatedAt = DateTime.Now,
                Price = dto.Price
            };


            int orderId = await _orderRepository.CreateOrderAsync(order);
            return Ok(new { orderId, message = "Order created successfully" });
        }


        // get all orders placed by the customer
        [HttpGet("myOrders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });


            int userId = int.Parse(userIdClaim);
            int customerId = await _dashboardRepository.GetCustomerIdByUserIdAsync(userId);
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }
    }
}




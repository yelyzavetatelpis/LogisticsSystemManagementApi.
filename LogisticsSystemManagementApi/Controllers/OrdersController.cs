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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _repository;

        public OrdersController(IOrderRepository repository)
        {
            _repository = repository;
        }

        // Creates a new order for a customer
        [HttpPost("createOrder")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);
            int customerId = await _repository.GetCustomerIdByUserIdAsync(userId);

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
                OrderStatusId = 7, // Pending 
                CreatedAt = DateTime.Now
            };

            await _repository.CreateOrderAsync(order);
            return Ok(new { message = "Order created successfully" });
        }

        // all orders made by the customer
        [HttpGet("myOrders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token" });

            int userId = int.Parse(userIdClaim);
            int customerId = await _repository.GetCustomerIdByUserIdAsync(userId);
            var orders = await _repository.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }

        // --- Dispatcher-related functionality ---

        // all orders waiting to be reviewed
        [HttpGet("pending")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await _repository.GetPendingOrdersAsync();
            return Ok(orders);
        }

        // accepting an order and createing a shipment 
        [HttpPost("{id}/accept")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            await _repository.AcceptOrderAsync(id);
            return Ok(new { message = "Order accepted successfully" });
        }

        // rejecting an order with a reason
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> RejectOrder(int id, RejectOrderDto dto)
        {
            await _repository.RejectOrderAsync(id, dto.Reason);
            return Ok(new { message = "Order rejected successfully" });
        }

        // all shipments ready to be assigned to trips
        [HttpGet("shipments")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> GetShipments()
        {
            var shipments = await _repository.GetShipmentsAsync();
            return Ok(shipments);
        }
    }
}
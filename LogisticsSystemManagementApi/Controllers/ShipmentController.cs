using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LogisticsSystemManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentRepository _repository;

        public ShipmentController(IShipmentRepository repository)
        {
            _repository = repository;
        }

        // get all orders waiting for dispatcher review
        [HttpGet("pending")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await _repository.GetPendingOrdersAsync();
            return Ok(orders);
        }

        // accept an order and create a shipment for it
        [HttpPost("{id}/accept")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            await _repository.AcceptOrderAsync(id);
            return Ok(new { message = "Order accepted successfully" });
        }

        // reject an order with a reason
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> RejectOrder(int id, RejectOrderDto dto)
        {
            await _repository.RejectOrderAsync(id, dto.Reason);
            return Ok(new { message = "Order rejected successfully" });
        }

        // get all shipments ready to be assigned to a trip
        [HttpGet("ready")]
        [Authorize(Roles = "Dispatcher")]
        public async Task<IActionResult> GetShipments()
        {
            var shipments = await _repository.GetShipmentsAsync();
            return Ok(shipments);
        }
    }
}




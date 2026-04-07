using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.Models;


namespace LogisticsSystemManagementApi.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly DbContext _context;


        // order status id for pending dispatcher review
        private const int OrderStatusPending = 7;


        // shipment status id for ready-to-assign shipments
        private const int ShipmentStatusPending = 5;


        public ShipmentRepository(DbContext context)
        {
            _context = context;
        }

        // accept order and create a shipment
        public async Task AcceptOrderAsync(int orderId)
        {
            using var connection = _context.CreateConnection();


            await connection.ExecuteAsync(
                "UPDATE Orders SET OrderStatusId = 8 WHERE OrderId = @OrderId",
                new { OrderId = orderId });


            var shipmentExists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Shipments WHERE OrderId = @OrderId",
                new { OrderId = orderId });


            if (shipmentExists == 0)
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO Shipments (OrderId, ShipmentStatusId, CreatedAt)
                      VALUES (@OrderId, @ShipmentStatusId, @CreatedAt)",
                    new { OrderId = orderId, ShipmentStatusId = ShipmentStatusPending, CreatedAt = DateTime.UtcNow });
            }
        }

        // reject an order and save the reason in additional notes
        public async Task RejectOrderAsync(int orderId, string reason)
        {
            var sql = @"UPDATE Orders
                        SET OrderStatusId = 13, AdditionalNotes = @AdditionalNotes
                        WHERE OrderId = @OrderId";


            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { AdditionalNotes = reason, OrderId = orderId });
        }


        // get all orders waiting for dispatcher review
        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            var sql = "SELECT * FROM Orders WHERE OrderStatusId = @Status";
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<Order>(sql, new { Status = OrderStatusPending });
            return result.ToList();
        }

        // get all shipments ready for trip assignment (status = Pending, not yet assigned)
        public async Task<List<Shipment>> GetShipmentsAsync()
        {
            var query = @"
                SELECT s.*, o.*, ss.*
                FROM Shipments s
                INNER JOIN Orders o ON s.OrderId = o.OrderId
                INNER JOIN ShipmentStatus ss ON ss.ShipmentStatusId = s.ShipmentStatusId
                WHERE s.ShipmentStatusId = @ShipmentStatusId";


            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<Shipment, Order, ShipmentStatus, Shipment>(
                query,
                (shipment, order, status) =>
                {
                    shipment.Order = order;
                    shipment.ShipmentStatus = status;
                    return shipment;
                },
                new { ShipmentStatusId = ShipmentStatusPending },
                splitOn: "OrderId,ShipmentStatusId");


            return result.ToList();
        }
    }
}




using Ecommerce.Model;
using Ecommerce.OrderService.Infrastructure;
using MassTransit;
using Polly.Retry;
using Polly;

namespace Ecommerce.OrderService.Logic
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public OrderService(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;

            // Retry policy setup: 3 retries with 2-second delays between retries
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} due to error: {exception.Message}. Retrying in {timeSpan.TotalSeconds}s.");
                    });
        }

        // Create Order
        public async Task<OrderModel> CreateOrderAsync(OrderModel order)
        {
            try
            {
                await _orderRepository.AddOrderAsync(order);
                _logger.LogInformation("Order {OrderId} created successfully.", order.Id);

                // Publish event for other services (e.g., ProductService)
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var orderCreatedEvent = new OrderCreatedEvent
                    {
                        OrderId = order.Id,
                        ProductId = order.ProductId,
                        Quantity = order.Quantity
                    };
                    await _publishEndpoint.Publish(orderCreatedEvent);
                    _logger.LogInformation("Order {OrderId} event published.", order.Id);
                });

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order {OrderId}.", order.Id);
                throw;
            }
        }

        // Get Order by ID
        public async Task<OrderModel> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                throw new KeyNotFoundException("Order not found.");
            }
            return order;
        }

        // Get All Orders
        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        // Update Order
        public async Task UpdateOrderAsync(OrderModel order)
        {
            try
            {
                await _orderRepository.UpdateOrderAsync(order);
                _logger.LogInformation("Order {OrderId} updated successfully.", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order {OrderId}.", order.Id);
                throw;
            }
        }

        // Delete Order
        public async Task DeleteOrderAsync(int orderId)
        {
            try
            {
                await _orderRepository.DeleteOrderAsync(orderId);
                _logger.LogInformation("Order {OrderId} deleted successfully.", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order {OrderId}.", orderId);
                throw;
            }
        }
    }


}

using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Ecommerce.OrderService.Utils
{
    public class OrderConsumer : IConsumer<OrderModel>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderConsumer> _logger;

        public OrderConsumer(IServiceScopeFactory serviceScopeFactory, ILogger<OrderConsumer> logger)
        {
            _dbContext = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<OrderDbContext>();
            _publishEndpoint = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IPublishEndpoint>();
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderModel> context)
        {
            try
            {
                OrderModel order = context.Message;

                // Save the order to the database
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                // Publish an event to notify other services (ProductService in this case)
                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    ProductId = order.ProductId,
                    Quantity = order.Quantity
                };

                await _publishEndpoint.Publish(orderCreatedEvent);

                _logger.LogInformation($"Order created and event published for Order ID: {order.Id}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while consuming OrderModel.");
                throw;
            }
        }
    }
}

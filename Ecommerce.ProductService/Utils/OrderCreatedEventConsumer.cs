using Ecommerce.Model;
using Ecommerce.ProductService.Infrastructure;
using Ecommerce.ProductService.Utils;
using MassTransit;

namespace Ecommerce.OrderService.Utils
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly IProductRepository _productRepository;

        public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var eventData = context.Message;
            _logger.LogInformation($"Event received: Order {eventData.OrderId} created for Product {eventData.ProductId}, Quantity: {eventData.Quantity}");

            try
            {
                // Get the product from the database
                var product = await _productRepository.GetProductByIdAsync(eventData.ProductId);

                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {eventData.ProductId} not found.");
                    throw new ProductNotFoundException($"Product with ID {eventData.ProductId} not found.");
                }

                // Check if there is enough stock
                if (product.Quantity < eventData.Quantity)
                {
                    _logger.LogWarning($"Not enough stock for Product {eventData.ProductId}. Requested: {eventData.Quantity}, Available: {product.Quantity}");
                    throw new InsufficientStockException($"Not enough stock for Product {eventData.ProductId}. Requested: {eventData.Quantity}, Available: {product.Quantity}");
                }

                // Deduct the quantity from the product
                product.Quantity -= eventData.Quantity;

                // Update the product quantity in the database
                await _productRepository.UpdateProductAsync(product);

                _logger.LogInformation($"Product {eventData.ProductId} quantity updated. New quantity: {product.Quantity}");
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogError(ex, "Product not found while processing the OrderCreatedEvent.");
                throw;  // MassTransit will handle retries or dead-lettering
            }
            catch (InsufficientStockException ex)
            {
                _logger.LogError(ex, "Insufficient stock while processing the OrderCreatedEvent.");
                throw;  // MassTransit will handle retries or dead-lettering
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the OrderCreatedEvent.");
                throw;  // MassTransit will handle retries or dead-lettering
            }
        }
        
}
}

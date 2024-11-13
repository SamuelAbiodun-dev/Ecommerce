using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using Ecommerce.ProductService.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Threading.Tasks;

namespace Ecommerce.ProductService.Logic
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProductService> _logger;
        private readonly ProductDbContext _productDbContext;
        private readonly AsyncPolicy _circuitBreakerPolicy;

        public ProductService(IProductRepository productRepository, IPublishEndpoint publishEndpoint, ILogger<ProductService> logger, ProductDbContext productDbContext)
        {
            _productRepository = productRepository;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _productDbContext = productDbContext;

            // Define the circuit breaker policy
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    3, // Number of allowed failures
                    TimeSpan.FromSeconds(10), // Duration of the open state
                    onBreak: (exception, timespan) => _logger.LogWarning($"Circuit broken: {exception.Message}"),
                    onReset: () => _logger.LogInformation("Circuit reset.")
                );
        }

        public async Task<ProductModel> CreateProductAsync(ProductModel product)
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await _productRepository.AddProductAsync(product);
                _logger.LogInformation($"Product {product.Name} created successfully.");
            });

            return product;
        }

        public async Task<ProductModel> GetProductByIdAsync(int productId)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                return await _productRepository.GetProductByIdAsync(productId);
            });
        }

        public async Task<IEnumerable<ProductModel>> GetAllProductsAsync()
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                return await _productRepository.GetAllProductsAsync();
            });
        }

        public async Task UpdateProductAsync(ProductModel product)
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await _productRepository.UpdateProductAsync(product);
                _logger.LogInformation($"Product {product.Name} updated successfully.");
            });
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await _productRepository.DeleteProductAsync(productId);
                _logger.LogInformation($"Product with ID {productId} deleted successfully.");
            });
        }

        // This method will handle the event from OrderService
        public async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent)
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var product = await _productRepository.GetProductByIdAsync(orderCreatedEvent.ProductId);
                if (product != null && product.Quantity >= orderCreatedEvent.Quantity)
                {
                    // Begin a new transaction to ensure atomicity of the operation
                    using var transaction = await _productDbContext.Database.BeginTransactionAsync();
                    try
                    {
                        // Deduct the quantity from the product
                        product.Quantity -= orderCreatedEvent.Quantity;

                        // Update product in the database
                        await _productRepository.UpdateProductAsync(product);

                        // Publish an event after updating the product
                        await _publishEndpoint.Publish(new OrderProcessedEvent(
                            orderCreatedEvent.ProductId,
                            orderCreatedEvent.Quantity,
                            orderCreatedEvent.OrderId
                        ));

                        // Commit the transaction
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Updated product quantity for Product {product.Name}. New Quantity: {product.Quantity}");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of an error
                        await transaction.RollbackAsync();
                        _logger.LogError($"Error processing order for Product {product.Name}: {ex.Message}");
                    }
                }
                else
                {
                    // Log a warning if there isn't enough stock for the order
                    _logger.LogWarning($"Not enough stock for Product {orderCreatedEvent.ProductId}. Order cannot be processed.");
                }
            });
        }
    }
}

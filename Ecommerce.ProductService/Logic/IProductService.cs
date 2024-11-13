using Ecommerce.Model;

namespace Ecommerce.ProductService.Logic
{
    public interface IProductService
    {
        Task<ProductModel> CreateProductAsync(ProductModel product);

        Task<ProductModel> GetProductByIdAsync(int productId);

        Task<IEnumerable<ProductModel>> GetAllProductsAsync();

        Task UpdateProductAsync(ProductModel product);

        Task DeleteProductAsync(int productId);

        // Method to handle order creation and update the product quantity
        Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent);
    }
}

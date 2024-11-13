using Ecommerce.Model;

namespace Ecommerce.ProductService.Infrastructure
{
    public interface IProductRepository
    {
        Task<ProductModel> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductModel>> GetAllProductsAsync();
        Task AddProductAsync(ProductModel product);
        Task UpdateProductAsync(ProductModel product);
        Task DeleteProductAsync(int productId);
    }
}

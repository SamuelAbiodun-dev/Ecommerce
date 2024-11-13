using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.ProductService.Infrastructure
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;
        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<ProductModel> GetProductByIdAsync(int productId)
        {
            return await _context.Set<ProductModel>().FindAsync(productId);
        }

        public async Task<IEnumerable<ProductModel>> GetAllProductsAsync()
        {
            return await _context.Set<ProductModel>().ToListAsync();
        }

        public async Task AddProductAsync(ProductModel product)
        {
            await _context.Set<ProductModel>().AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(ProductModel product)
        {
            _context.Set<ProductModel>().Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                _context.Set<ProductModel>().Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}

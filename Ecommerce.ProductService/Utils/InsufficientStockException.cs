namespace Ecommerce.ProductService.Utils
{
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message) : base(message) { }
    }

}

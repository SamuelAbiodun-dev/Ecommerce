namespace Ecommerce.ProductService.Utils
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string message) : base(message) { }
    }


}

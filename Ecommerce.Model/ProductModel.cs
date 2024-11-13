namespace Ecommerce.Model
{
    public class ProductModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; } // New field for tracking when the product was created

        public DateTime DateUpdated { get; set; } // New field for tracking the last update
    }
}

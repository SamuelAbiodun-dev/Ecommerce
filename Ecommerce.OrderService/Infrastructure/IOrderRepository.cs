using Ecommerce.Model;

namespace Ecommerce.OrderService.Infrastructure
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(OrderModel order);
        Task<OrderModel> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync();
        Task UpdateOrderAsync(OrderModel order);
        Task DeleteOrderAsync(int orderId);
    }
}

using Ecommerce.Model;

namespace Ecommerce.OrderService.Logic
{
    public interface IOrderService
    {
        Task<OrderModel> CreateOrderAsync(OrderModel order);
        Task<OrderModel> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync();
        Task UpdateOrderAsync(OrderModel order);
        Task DeleteOrderAsync(int orderId);
    }
}

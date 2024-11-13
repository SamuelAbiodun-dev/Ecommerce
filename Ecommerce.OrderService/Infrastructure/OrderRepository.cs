using Ecommerce.Model;

namespace Ecommerce.OrderService.Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        private readonly List<OrderModel> _orders = new List<OrderModel>();

        public async Task AddOrderAsync(OrderModel order)
        {
            _orders.Add(order);
            await Task.CompletedTask;
        }

        public async Task<OrderModel> GetOrderByIdAsync(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            return await Task.FromResult(order);
        }

        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
        {
            return await Task.FromResult(_orders);
        }

        public async Task UpdateOrderAsync(OrderModel order)
        {
            var existingOrder = _orders.FirstOrDefault(o => o.Id == order.Id);
            if (existingOrder != null)
            {
                existingOrder.CustomerName = order.CustomerName;
                existingOrder.ProductId = order.ProductId;
                existingOrder.Quantity = order.Quantity;
                existingOrder.OrderDate = order.OrderDate;
                await Task.CompletedTask;
            }
            else
            {
                throw new KeyNotFoundException("Order not found.");
            }
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                _orders.Remove(order);
                await Task.CompletedTask;
            }
            else
            {
                throw new KeyNotFoundException("Order not found.");
            }
        }
    }
    }

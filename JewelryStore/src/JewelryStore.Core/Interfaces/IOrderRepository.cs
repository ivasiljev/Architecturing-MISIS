using JewelryStore.Core.Entities;

namespace JewelryStore.Core.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task DeleteAsync(int id);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);
}
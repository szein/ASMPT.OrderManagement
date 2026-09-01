using core.Models;
public interface IOrderService
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> CreateAsync(string name, string description, DateTime orderDate);
    Task<Order?> UpdateAsync(int id, string name, string description, DateTime orderDate);
    Task<Order?> UpdateBoardAssignmentsAsync(int orderId, IEnumerable<int> boardIds);
    Task<bool> DeleteAsync(int id);
}

using core.Models;
public interface IOrderService
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(string name, string description, DateTime orderDate);
    Task<Order?> UpdateAsync(Guid id, string name, string description, DateTime orderDate);
    Task<Order?> UpdateBoardAssignmentsAsync(Guid orderId, IEnumerable<int> boardIds);
    Task<bool> DeleteAsync(Guid id);
}

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<Order?> UpdateBoardAssignmentsAsync(int orderId, IEnumerable<int> boardIds);
    Task<bool> DeleteAsync(int id);
}

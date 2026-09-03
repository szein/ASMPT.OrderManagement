public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<List<Board>> GetBoardsAsync(Guid orderId);
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<Order?> UpdateBoardAssignmentsAsync(Guid orderId, IEnumerable<int> boardIds);
    Task<Order?> UpdateComponentAssignmentsAsync(Guid orderId, IEnumerable<OrderComponentAssignment> assignments);
    Task<Order?> SaveAsync(Guid orderId);
    Task<bool> DeleteAsync(Guid id);
}

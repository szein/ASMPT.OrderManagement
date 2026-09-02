public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> AddAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<Order?> UpdateBoardAssignmentsAsync(Guid orderId, IEnumerable<int> boardIds);
    Task<bool> DeleteAsync(Guid id);
}

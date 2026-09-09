public interface IOrderRepository
{
    /// <summary>
    /// Get All Orders
    /// </summary>
    /// <returns></returns>
    Task<List<Order>> GetAllAsync();
    /// <summary>
    /// Get Boards in an Order
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    Task<List<Board>> GetBoardsAsync(Guid orderId);
    /// <summary>
    /// Get all components for the Board in the Order.
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="boardId"></param>
    /// <returns>Components assigned to the board within the order</returns>
    Task<List<OrderBoardComponent>> GetComponentsAsync(Guid orderId, int boardId);
    /// <summary>
    /// Get Order by its Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Order?> GetByIdAsync(Guid id);
    /// <summary>
    /// Get Order data to be exported (as json)
    /// </summary>
    /// <param name="id">Order id</param>
    /// <returns></returns>
    Task<OrderExportResponse?> GetExportDataAsync(Guid id);
    /// <summary>
    /// Add Order to the database
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    Task<Order> AddAsync(Order order);
    /// <summary>
    /// Update an Order 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    Task<Order> UpdateAsync(Order order);
    /// <summary>
    /// Delete an Order by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Guid id);
}

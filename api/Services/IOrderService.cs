using core.Models;
public interface IOrderService
{
    /// <summary>
    /// Get all Orders
    /// </summary>
    /// <returns>List of Orders</returns>
    Task<List<Order>> GetAllAsync();
    /// <summary>
    /// Get Boards in an Order
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns>List of Boards</returns>
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
    /// Get order realted data from repository 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<OrderExportResponse?> GetExportDataAsync(Guid id);
    /// <summary>
    /// Create new Order with its Boards and Components
    /// </summary>
    /// <param name="request">A DTO that holds the order, boards and componants data</param>
    /// <returns></returns>
    Task<Order> CreateAsync(CreateOrderRequest request);
    /// <summary>
    /// Update the order data (name, description and order data)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="orderDate"></param>
    /// <returns></returns>
    Task<Order?> UpdateAsync(Guid id, string name, string description, DateTime orderDate);
    /// <summary>
    /// Delete Order by its id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Guid id);
}

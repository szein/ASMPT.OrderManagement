using core.Models;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all orders.");
        return await _orderRepository.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Service request received to fetch order {OrderId}.", id);
        return await _orderRepository.GetByIdAsync(id);
    }

    public async Task<Order> CreateAsync(string name, string description, DateTime orderDate)
    {
        _logger.LogInformation("Creating a new order with name {OrderName} and date {OrderDate}.", name, orderDate);

        ValidateOrder(name, description, orderDate);

        var order = new Order
        {
            Name = name.Trim(),
            Description = description.Trim(),
            OrderDate = orderDate
        };

        var createdOrder = await _orderRepository.AddAsync(order);
        _logger.LogInformation("Order creation completed for order {OrderId}.", createdOrder.Id);
        return createdOrder;
    }

    public async Task<Order?> UpdateAsync(int id, string name, string description, DateTime orderDate)
    {
        _logger.LogInformation("Updating order {OrderId}.", id);

        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder is null)
        {
            _logger.LogWarning("Update requested for missing order {OrderId}.", id);
            return null;
        }

        ValidateOrder(name, description, orderDate);

        existingOrder.Name = name.Trim();
        existingOrder.Description = description.Trim();
        existingOrder.OrderDate = orderDate;

        var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
        _logger.LogInformation("Order {OrderId} was successfully updated.", updatedOrder.Id);
        return updatedOrder;
    }

    public async Task<Order?> UpdateBoardAssignmentsAsync(int orderId, IEnumerable<int> boardIds)
    {
        _logger.LogInformation("Updating board assignments for order {OrderId}.", orderId);

        if (boardIds is null)
        {
            throw new ArgumentException("Board IDs are required.", nameof(boardIds));
        }

        var normalizedBoardIds = boardIds.ToList();
        if (normalizedBoardIds.Any(boardId => boardId <= 0))
        {
            throw new ArgumentException("Board IDs must be greater than zero.", nameof(boardIds));
        }

        var existingOrder = await _orderRepository.GetByIdAsync(orderId);
        if (existingOrder is null)
        {
            _logger.LogWarning("Board assignment update requested for missing order {OrderId}.", orderId);
            return null;
        }

        return await _orderRepository.UpdateBoardAssignmentsAsync(orderId, normalizedBoardIds);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Delete request received for order {OrderId}.", id);
        var deleted = await _orderRepository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete operation failed because order {OrderId} was not found.", id);
        }

        return deleted;
    }

    private static void ValidateOrder(string name, string description, DateTime orderDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Order name is required.", nameof(name));
        }

        if (description is null)
        {
            throw new ArgumentException("Order description is required.", nameof(description));
        }

        if (orderDate == default)
        {
            throw new ArgumentException("Order date is required.", nameof(orderDate));
        }
    }
}

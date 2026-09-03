using core.Models;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStatusService _orderStatusService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger, IOrderStatusService? orderStatusService = null)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _orderStatusService = orderStatusService ?? new OrderStatusService();
    }

    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all orders.");
        return await _orderRepository.GetAllAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
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
            OrderDate = orderDate,
            Status = _orderStatusService.GetInitialStatus()
        };

        var createdOrder = await _orderRepository.AddAsync(order);
        _logger.LogInformation("Order creation completed for order {OrderId}.", createdOrder.Id);
        return createdOrder;
    }

    public async Task<Order?> UpdateAsync(Guid id, string name, string description, DateTime orderDate)
    {
        _logger.LogInformation("Updating order {OrderId}.", id);

        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder is null)
        {
            _logger.LogWarning("Update requested for missing order {OrderId}.", id);
            return null;
        }

        _orderStatusService.EnsureCanEdit(existingOrder);

        ValidateOrder(name, description, orderDate);

        existingOrder.Name = name.Trim();
        existingOrder.Description = description.Trim();
        existingOrder.OrderDate = orderDate;

        var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
        _logger.LogInformation("Order {OrderId} was successfully updated.", updatedOrder.Id);
        return updatedOrder;
    }

    public async Task<Order?> UpdateBoardAssignmentsAsync(Guid orderId, IEnumerable<int> boardIds)
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

        _orderStatusService.EnsureCanEdit(existingOrder);

        return await _orderRepository.UpdateBoardAssignmentsAsync(orderId, normalizedBoardIds);
    }

    public async Task<Order?> SaveAsync(Guid orderId)
    {
        _logger.LogInformation("Saving order {OrderId}.", orderId);
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
        {
            return null;
        }

        _orderStatusService.EnsureCanSave(order);
        return await _orderRepository.SaveAsync(orderId);
    }

    public async Task<Order?> UpdateComponentAssignmentsAsync(Guid orderId, IEnumerable<OrderComponentAssignment> assignments)
    {
        if (assignments is null)
        {
            throw new ArgumentException("Component assignments are required.", nameof(assignments));
        }

        var existingOrder = await _orderRepository.GetByIdAsync(orderId);
        if (existingOrder is null)
        {
            return null;
        }

        _orderStatusService.EnsureCanEdit(existingOrder);
        var normalizedAssignments = assignments.ToList();
        if (normalizedAssignments.Any(a => a.ComponentId <= 0 || a.Quantity <= 0))
        {
            throw new ArgumentException("Each component assignment must include a valid component ID and quantity greater than zero.", nameof(assignments));
        }

        return await _orderRepository.UpdateComponentAssignmentsAsync(orderId, normalizedAssignments);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Delete request received for order {OrderId}.", id);
        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return false;
        }

        _orderStatusService.EnsureCanDelete(order);
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

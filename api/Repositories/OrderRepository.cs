using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all orders from the database.");

        return await _context.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching order with Id {OrderId}.", id);

        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        _logger.LogInformation("Adding new order named {OrderName} for date {OrderDate}.", order.Name, order.OrderDate);

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was created successfully.", order.Id);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _logger.LogInformation("Updating order {OrderId}.", order.Id);

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was updated successfully.", order.Id);
        return order;
    }

    public async Task<Order?> UpdateBoardAssignmentsAsync(Guid orderId, IEnumerable<int> boardIds)
    {
        _logger.LogInformation("Updating board assignments for order {OrderId}.", orderId);

        if (boardIds is null)
        {
            throw new ArgumentException("Board IDs are required.", nameof(boardIds));
        }

        var requestedBoardIds = boardIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (requestedBoardIds.Count != boardIds.Count())
        {
            throw new ArgumentException("Duplicate or invalid board IDs were supplied.", nameof(boardIds));
        }

        if (requestedBoardIds.Count == 0)
        {
            var currentAssignments = await _context.OrderBoards
                .Where(ob => ob.OrderId == orderId)
                .ToListAsync();

            if (currentAssignments.Count > 0)
            {
                _context.OrderBoards.RemoveRange(currentAssignments);
                await _context.SaveChangesAsync();
            }

            return await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        var orderExists = await _context.Orders.AnyAsync(o => o.Id == orderId);
        if (!orderExists)
        {
            _logger.LogWarning("Board assignment update requested for missing order {OrderId}.", orderId);
            return null;
        }

        var validBoardIds = await _context.Boards
            .Where(b => requestedBoardIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync();

        if (validBoardIds.Count != requestedBoardIds.Count)
        {
            throw new ArgumentException("One or more board IDs do not exist.", nameof(boardIds));
        }

        var existingAssignments = await _context.OrderBoards
            .Where(ob => ob.OrderId == orderId)
            .ToListAsync();

        var currentBoardIds = existingAssignments.Select(ob => ob.BoardId).ToHashSet();
        var targetBoardIds = requestedBoardIds.ToHashSet();

        var assignmentsToRemove = existingAssignments
            .Where(ob => !targetBoardIds.Contains(ob.BoardId))
            .ToList();

        if (assignmentsToRemove.Count > 0)
        {
            _context.OrderBoards.RemoveRange(assignmentsToRemove);
        }

        var assignmentsToAdd = targetBoardIds
            .Where(boardId => !currentBoardIds.Contains(boardId))
            .Select(boardId => new OrderBoard
            {
                OrderId = orderId,
                BoardId = boardId
            })
            .ToList();

        if (assignmentsToAdd.Count > 0)
        {
            await _context.OrderBoards.AddRangeAsync(assignmentsToAdd);
        }

        await _context.SaveChangesAsync();

        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> SaveAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderBoards)
            .Include(o => o.OrderComponents)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
        {
            return null;
        }

        if (order.OrderBoards.Count == 0)
        {
            throw new ArgumentException("At least one board is required to save an order.");
        }

        if (order.OrderComponents.Count == 0)
        {
            throw new ArgumentException("At least one component with a quantity greater than zero is required to save an order.");
        }

        if (order.OrderComponents.Any(assignment => assignment.Quantity <= 0))
        {
            throw new ArgumentException("Component quantities must be greater than zero.");
        }

        var componentIds = order.OrderComponents.Select(assignment => assignment.ComponentId).ToList();
        var components = await _context.Components
            .Where(component => componentIds.Contains(component.Id))
            .OrderBy(component => component.Id)
            .ToListAsync();

        if (components.Count != order.OrderComponents.Select(assignment => assignment.ComponentId).Distinct().Count())
        {
            throw new ArgumentException("One or more components do not exist.");
        }

        foreach (var assignment in order.OrderComponents)
        {
            var component = components.Single(component => component.Id == assignment.ComponentId);
            if (component.Quantity < assignment.Quantity)
            {
                throw new ArgumentException($"Insufficient quantity for component {assignment.ComponentId}.");
            }
        }

        foreach (var assignment in order.OrderComponents)
        {
            var component = components.Single(component => component.Id == assignment.ComponentId);
            component.Quantity -= assignment.Quantity;
        }

        order.Status = OrderStatus.Created;
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> UpdateComponentAssignmentsAsync(Guid orderId, IEnumerable<OrderComponentAssignment> assignments)
    {
        var requestedAssignments = assignments.ToList();
        if (requestedAssignments.GroupBy(assignment => assignment.ComponentId).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Duplicate component IDs were supplied.", nameof(assignments));
        }

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return null;
        }

        var componentIds = requestedAssignments.Select(assignment => assignment.ComponentId).ToList();
        if (await _context.Components.CountAsync(component => componentIds.Contains(component.Id)) != componentIds.Count)
        {
            throw new ArgumentException("One or more components do not exist.", nameof(assignments));
        }

        var existingAssignments = await _context.OrderComponents
            .Where(assignment => assignment.OrderId == orderId)
            .ToListAsync();
        _context.OrderComponents.RemoveRange(existingAssignments);
        await _context.OrderComponents.AddRangeAsync(requestedAssignments.Select(assignment => new OrderComponent
        {
            OrderId = orderId,
            ComponentId = assignment.ComponentId,
            Quantity = assignment.Quantity
        }));
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Attempting to delete order {OrderId}.", id);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            _logger.LogWarning("Delete requested for order {OrderId}, but it was not found.", id);
            return false;
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be deleted.");
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was deleted successfully.", id);
        return true;
    }
}

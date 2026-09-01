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

    public async Task<Order?> GetByIdAsync(int id)
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

    public async Task<Order?> UpdateBoardAssignmentsAsync(int orderId, IEnumerable<int> boardIds)
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

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete order {OrderId}.", id);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
        {
            _logger.LogWarning("Delete requested for order {OrderId}, but it was not found.", id);
            return false;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} was deleted successfully.", id);
        return true;
    }
}

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

    //TODO: Check if it needs improvment
    public async Task<List<Order>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all orders from the database.");

        return await _context.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    public async Task<List<Board>> GetBoardsAsync(Guid orderId)
    {
        return await _context.OrderBoards
            .Where(orderBoard => orderBoard.OrderId == orderId)
            .Select(orderBoard => orderBoard.Board)
            .AsNoTracking()
            .OrderBy(board => board.Id)
            .ToListAsync();
    }

    public async Task<List<OrderBoardComponent>> GetComponentsAsync(Guid orderId, int boardId)
    {
        return await _context.OrderBoardComponents
            .Where(orderBoardComponent =>
                orderBoardComponent.OrderBoard.OrderId == orderId &&
                orderBoardComponent.OrderBoard.BoardId == boardId)
            .Include(orderBoardComponent => orderBoardComponent.Component)
            .ThenInclude(component => component.ComponentType)
            .AsNoTracking()
            .OrderBy(orderBoardComponent => orderBoardComponent.ComponentId)
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

        var componentsInRequest = order.OrderBoards
            .SelectMany(b => b.OrderBoardComponents)
            .GroupBy(c => c.ComponentId)
            .Select(g => new 
            {
                ComponentId = g.Key,
                TotalQuantity = g.Sum(c => c.Quantity)
            })
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync();
        foreach (var componentInRequest in componentsInRequest)
        {
            _logger.LogDebug("Component {componentId} with Total Quantity {Quantity} in request",componentInRequest.ComponentId, componentInRequest.TotalQuantity);
            var componentInDb = _context.Components.Single(c=> c.Id == componentInRequest.ComponentId);
            _logger.LogDebug("Component {componentId} has Quantity {Quantity} in Database",componentInRequest.ComponentId, componentInDb.Quantity);
            
            if(componentInDb.Quantity < componentInRequest.TotalQuantity) throw new ArgumentOutOfRangeException("Quantity in Database is not Sufficent!");
            componentInDb.Quantity -= componentInRequest.TotalQuantity;
        }
        order.Status = OrderStatus.Created;
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

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

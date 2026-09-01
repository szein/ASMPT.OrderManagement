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

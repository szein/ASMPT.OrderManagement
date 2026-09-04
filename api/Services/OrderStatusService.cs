using core.Models;

public class OrderStatusService : IOrderStatusService
{
    public OrderStatus GetInitialStatus() => OrderStatus.Pending;

    public void EnsureCanEdit(Order order)
    {
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be edited.");
        }
    }

    public void EnsureCanUpdate(Order order)
    {
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be saved.");
        }
    }

    public void EnsureCanDelete(Order order)
    {
        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Only pending orders can be deleted.");
        }
    }
}
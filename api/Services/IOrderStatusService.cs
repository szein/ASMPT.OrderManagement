using core.Models;

public interface IOrderStatusService
{
    OrderStatus GetInitialStatus();
    void EnsureCanEdit(Order order);
    void EnsureCanSave(Order order);
    void EnsureCanDelete(Order order);
}
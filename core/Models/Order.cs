namespace core.Models;

public class Order : Auditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
}


public class OrderBoard
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
}

public enum OrderStatus
{
    Created = 0,
    Pending = 10,
    InProgress = 20,
    Completed = 30,
    Cancelled = 40
} 

namespace core.Models;

public class Order : Auditable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
    public ICollection<OrderComponent> OrderComponents { get; set; } = new List<OrderComponent>();
}

public class OrderComponent
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;

    public int Quantity { get; set; }
}

public record OrderComponentAssignment(int ComponentId, int Quantity);

public class OrderBoard
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
}


namespace core.Models;

public class Board : Auditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Length { get; set; }
    public double Width { get; set; }

    public ICollection<OrderBoard> OrderBoards { get; set; } = new List<OrderBoard>();
    public ICollection<BoardComponent> BoardComponents { get; set; } = new List<BoardComponent>();
}


public class BoardComponent : Auditable
{
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;

    public int Quantity { get; set; }
}

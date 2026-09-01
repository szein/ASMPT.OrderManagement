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

    //normaly models should be dumb, but this is a business rule that makes sense to be here
    public void AddComponent(Component component, int quantity)
    {
        if (component == null)
        {
            //TODO: find out if here should throw exception or do somthing else.
            throw new ArgumentNullException(nameof(component), "Component cannot be null.");
        }

        if (quantity <= 0)
        {
            //TODO: find out if here should throw exception or do somthing else.
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var boardComponent = new BoardComponent
        {
            Board = this,
            Component = component,
            BoardComponentQuantity = quantity
        };

        BoardComponents.Add(boardComponent);
    }
}


public class BoardComponent : Auditable
{
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;

    //TODO: Consider checking if Quantity here is available in the Component before adding to BoardComponent
    public int BoardComponentQuantity { get; set; }
}

public record BoardComponentAssignment(int ComponentId, int Quantity);

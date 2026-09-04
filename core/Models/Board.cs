using System.Text.Json.Serialization;

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
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    [JsonIgnore]
    public Order Order { get; set; } = null!;
    public int BoardId { get; set; }
    [JsonIgnore]
    public Board Board { get; set; } = null!;
    public int ComponentId { get; set; }
    [JsonIgnore]
    public Component Component { get; set; } = null!;
    public int BoardComponentQuantity { get; set; }
}

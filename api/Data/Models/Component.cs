public class Component:Auditable
{
    public int Id { get; set; }
    public int ComponentTypeId { get; set; }

    public ComponentType ComponentType { get; set; } = null!;
    public ICollection<BoardComponent> BoardComponents { get; set; } = new List<BoardComponent>();
}
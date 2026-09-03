namespace core.Models;

public class Component : Auditable
{
    private ComponentStatus status = ComponentStatus.Available;

    public int Id { get; set; }
    public int ComponentTypeId { get; set; }
    public int Quantity { get; set; } //TODO: convert to uint since quantity cannot be negative.

    public ComponentStatus Status
    {
        get => status == ComponentStatus.Abandoned
            ? ComponentStatus.Abandoned
            : Quantity > 0 ? ComponentStatus.Available : ComponentStatus.OutOfStock;
        set => status = value;
    }
    public ComponentType ComponentType { get; set; } = null!;
    public ICollection<BoardComponent> BoardComponents { get; set; } = new List<BoardComponent>();

    public void AddComponentType(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            //TODO: find out if here should throw exception or do somthing else.
            throw new ArgumentException("ComponentType name cannot be null or whitespace.", nameof(name));
        }

        var componentType = new ComponentType
        {
            Name = name,
            Description = description
        };

        ComponentType = componentType;
        ComponentTypeId = componentType.Id;
    }
    
}

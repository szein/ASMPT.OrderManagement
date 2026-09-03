using Microsoft.EntityFrameworkCore;

public class ComponentRepository : IComponentRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ComponentRepository> _logger;

    public ComponentRepository(AppDbContext context, ILogger<ComponentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Component>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all components from the database.");

        return await _context.Components
            .Include(c => c.ComponentType)
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Component?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching component with Id {ComponentId}.", id);

        return await _context.Components
            .Include(c => c.ComponentType)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Component> AddAsync(Component component)
    {
        _logger.LogInformation("Adding new component with type {ComponentTypeId} and quantity {Quantity}.", component.ComponentTypeId, component.Quantity);

        await _context.Components.AddAsync(component);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Component {ComponentId} was created successfully.", component.Id);
        return component;
    }

    public async Task<Component> UpdateAsync(Component component)
    {
        _logger.LogInformation("Updating component {ComponentId}.", component.Id);

        _context.Components.Update(component);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Component {ComponentId} was updated successfully.", component.Id);
        return component;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete component {ComponentId}.", id);

        var component = await _context.Components.FirstOrDefaultAsync(c => c.Id == id);
        if (component is null)
        {
            _logger.LogWarning("Delete requested for component {ComponentId}, but it was not found.", id);
            return false;
        }

        var isInOrder = await _context.BoardComponents
            .Where(bc => bc.ComponentId == id)
            .Join(_context.OrderBoards, bc => bc.BoardId, ob => ob.BoardId, (bc, ob) => ob)
            .AnyAsync();

        if (isInOrder)
        {
            component.Status = ComponentStatus.Abandoned;
            component.Quantity = 0;
            await _context.SaveChangesAsync();
            return true;
        }

        var componentTypeId = component.ComponentTypeId;
        var assignments = await _context.BoardComponents
            .Where(bc => bc.ComponentId == id)
            .ToListAsync();
        _context.BoardComponents.RemoveRange(assignments);
        _context.Components.Remove(component);

        var hasOtherComponents = await _context.Components
            .AnyAsync(c => c.ComponentTypeId == componentTypeId && c.Id != id);
        if (!hasOtherComponents)
        {
            var componentType = await _context.ComponentTypes.FirstOrDefaultAsync(ct => ct.Id == componentTypeId);
            if (componentType is not null)
            {
                _context.ComponentTypes.Remove(componentType);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Component {ComponentId} was deleted successfully.", id);
        return true;
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        var componentType = await _context.ComponentTypes.FirstOrDefaultAsync(ct => ct.Id == id);
        if (componentType is null)
        {
            return false;
        }

        var components = await _context.Components
            .Where(c => c.ComponentTypeId == id)
            .ToListAsync();
        var componentIds = components.Select(c => c.Id).ToList();
        var isInOrder = await _context.BoardComponents
            .Where(bc => componentIds.Contains(bc.ComponentId))
            .Join(_context.OrderBoards, bc => bc.BoardId, ob => ob.BoardId, (bc, ob) => ob)
            .AnyAsync();

        if (isInOrder)
        {
            foreach (var component in components)
            {
                component.Status = ComponentStatus.Abandoned;
                component.Quantity = 0;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        var assignments = await _context.BoardComponents
            .Where(bc => componentIds.Contains(bc.ComponentId))
            .ToListAsync();
        _context.BoardComponents.RemoveRange(assignments);
        _context.Components.RemoveRange(components);
        _context.ComponentTypes.Remove(componentType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetAvailableQuantityAsync(int componentTypeId)
    {
        _logger.LogInformation("Fetching available quantity for component type {ComponentTypeId}.", componentTypeId);

        var availableQuantity = await _context.Components
            .Where(c => c.ComponentTypeId == componentTypeId)
            .SumAsync(c => c.Quantity);//TODO: improve this function

        _logger.LogInformation("Available quantity for component type {ComponentTypeId} is {AvailableQuantity}.", componentTypeId, availableQuantity);
        return availableQuantity;
    }
}

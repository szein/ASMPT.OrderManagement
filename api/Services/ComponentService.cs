using core.Models;

public class ComponentService : IComponentService
{
    private readonly IComponentRepository _componentRepository;
    private readonly IComponentTypeRepository _componentTypeRepository;
    private readonly ILogger<ComponentService> _logger;

    public ComponentService(
        IComponentRepository componentRepository,
        IComponentTypeRepository componentTypeRepository,
        ILogger<ComponentService> logger)
    {
        _componentRepository = componentRepository;
        _componentTypeRepository = componentTypeRepository;
        _logger = logger;
    }

    public async Task<List<Component>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all components.");
        return await _componentRepository.GetAllAsync();
    }

    public async Task<Component?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Service request received to fetch component {ComponentId}.", id);
        return await _componentRepository.GetByIdAsync(id);
    }

    public async Task<Component> CreateAsync(int componentTypeId, int quantity)
    {
        _logger.LogInformation("Creating a new component for type {ComponentTypeName}.", componentTypeId);

        ValidateComponent(componentTypeId, quantity);
        var componentType = await _componentTypeRepository.GetByIdAsync(componentTypeId);
        if (componentType is null)
        {
            throw new ArgumentException("Component type was not found.", nameof(componentTypeId));
        }

        var component = new Component
        {
            ComponentTypeId = componentTypeId,
            Quantity = quantity,
            ComponentType = componentType
        };

        var createdComponent = await _componentRepository.AddAsync(component);
        _logger.LogInformation("Component creation completed for component {ComponentId}.", createdComponent.Id);
        return createdComponent;
    }

    public async Task<Component> CreateAsync(ComponentType componentType, int quantity)
    {
        _logger.LogInformation("Creating a new component for type {ComponentTypeName}.", componentType?.Name);

        ValidateComponent(componentType, quantity);

        if (componentType.Id <= 0)
        {
            throw new ArgumentException("Component type was not found.", nameof(componentType));
        }

        var component = new Component
        {
            ComponentTypeId = componentType.Id,
            Quantity = quantity,
            ComponentType = componentType
        };

        var createdComponent = await _componentRepository.AddAsync(component);
        _logger.LogInformation("Component creation completed for component {ComponentId}.", createdComponent.Id);
        return createdComponent;
    }

    public async Task<Component?> UpdateAsync(int id, int componentTypeId, int quantity)
    {
        _logger.LogInformation("Updating component {ComponentId}.", id);

        var existingComponent = await _componentRepository.GetByIdAsync(id);
        if (existingComponent is null)
        {
            _logger.LogWarning("Update requested for missing component {ComponentId}.", id);
            return null;
        }

        ValidateComponent(componentTypeId, quantity);

        var componentType = await _componentTypeRepository.GetByIdAsync(componentTypeId);
        if (componentType is null)
        {
            throw new ArgumentException("Component type was not found.", nameof(componentTypeId));
        }

        existingComponent.ComponentTypeId = componentTypeId;
        existingComponent.Quantity = quantity;
        existingComponent.ComponentType = componentType;

        var updatedComponent = await _componentRepository.UpdateAsync(existingComponent);
        _logger.LogInformation("Component {ComponentId} was successfully updated.", updatedComponent.Id);
        return updatedComponent;
    }

    public async Task<int> GetAvailableQuantityAsync(int? componentTypeId, ComponentType? componentType)
    {
        _logger.LogInformation("Fetching available quantity for component type {ComponentTypeId}.", componentTypeId ?? componentType?.Id);

        if (componentTypeId is null && componentType is null)
        {
            throw new ArgumentException("Either component type ID or component type must be provided.");
        }

        if (componentTypeId is not null && componentType is not null && componentTypeId != componentType.Id)
        {
            throw new ArgumentException("Component type ID and component type do not match.");
        }

        int resolvedComponentTypeId = componentTypeId ?? componentType!.Id;
        

        var availableQuantity = await _componentRepository.GetAvailableQuantityAsync(resolvedComponentTypeId);
        _logger.LogInformation("Available quantity for component type {ComponentTypeId} is {AvailableQuantity}.", resolvedComponentTypeId, availableQuantity);
        return availableQuantity;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Delete request received for component {ComponentId}.", id);
        var deleted = await _componentRepository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete operation failed because component {ComponentId} was not found.", id);
        }

        return deleted;
    }

    private static void ValidateComponent(ComponentType componentType, int quantity)
    {
        if (componentType is null)
        {
            throw new ArgumentException("Component type is required.", nameof(componentType));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Component quantity must be greater than zero.", nameof(quantity));
        }
    }

    private static void ValidateComponent(int componentTypeId, int quantity)
    {
        if (componentTypeId <= 0)
        {
            throw new ArgumentException("Component type is required.", nameof(componentTypeId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Component quantity must be greater than zero.", nameof(quantity));
        }
    }
}

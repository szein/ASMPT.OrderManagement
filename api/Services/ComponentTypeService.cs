using core.Models;
public class ComponentTypeService : IComponentTypeService
{
    private readonly IComponentTypeRepository _componentTypeRepository;
    private readonly ILogger<ComponentTypeService> _logger;

    public ComponentTypeService(IComponentTypeRepository componentTypeRepository, ILogger<ComponentTypeService> logger)
    {
        _componentTypeRepository = componentTypeRepository;
        _logger = logger;
    }

    public async Task<List<ComponentType>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all component types.");
        return await _componentTypeRepository.GetAllAsync();
    }

    public async Task<ComponentType?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Service request received to fetch component type {ComponentTypeId}.", id);
        return await _componentTypeRepository.GetByIdAsync(id);
    }

    public async Task<ComponentType> CreateAsync(string name, string description)
    {
        _logger.LogInformation("Creating a new component type with name {ComponentTypeName}.", name);

        ValidateComponentType(name, description);

        var componentType = new ComponentType
        {
            Name = name.Trim(),
            Description = description.Trim()
        };

        var createdComponentType = await _componentTypeRepository.AddAsync(componentType);
        _logger.LogInformation("Component type creation completed for component type {ComponentTypeId}.", createdComponentType.Id);
        return createdComponentType;
    }

    public async Task<ComponentType?> UpdateAsync(int id, string name, string description)
    {
        _logger.LogInformation("Updating component type {ComponentTypeId}.", id);

        var existingComponentType = await _componentTypeRepository.GetByIdAsync(id);
        if (existingComponentType is null)
        {
            _logger.LogWarning("Update requested for missing component type {ComponentTypeId}.", id);
            return null;
        }

        ValidateComponentType(name, description);

        existingComponentType.Name = name.Trim();
        existingComponentType.Description = description.Trim();

        var updatedComponentType = await _componentTypeRepository.UpdateAsync(existingComponentType);
        _logger.LogInformation("Component type {ComponentTypeId} was successfully updated.", updatedComponentType.Id);
        return updatedComponentType;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Delete request received for component type {ComponentTypeId}.", id);
        var deleted = await _componentTypeRepository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete operation failed because component type {ComponentTypeId} was not found.", id);
        }

        return deleted;
    }
    

    private static void ValidateComponentType(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Component type name is required.", nameof(name));
        }

        if (description is null)
        {
            throw new ArgumentException("Component type description is required.", nameof(description));
        }
    }
}

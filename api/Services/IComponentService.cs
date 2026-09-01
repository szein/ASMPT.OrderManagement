using core.Models;
public interface IComponentService
{
    Task<List<Component>> GetAllAsync();
    Task<Component?> GetByIdAsync(int id);
    Task<Component> CreateAsync(int componentTypeId, int quantity);
    Task<Component> CreateAsync(ComponentType componentType, int quantity);
    Task<Component?> UpdateAsync(int id, int componentTypeId, int quantity);
    Task<bool> DeleteAsync(int id);
}

using core.Models;
public interface IComponentTypeService
{
    Task<List<ComponentType>> GetAllAsync();
    Task<ComponentType?> GetByIdAsync(int id);
    Task<ComponentType> CreateAsync(string name, string description);
    Task<ComponentType?> UpdateAsync(int id, string name, string description);
}

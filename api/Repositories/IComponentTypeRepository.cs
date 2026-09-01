public interface IComponentTypeRepository
{
    Task<List<ComponentType>> GetAllAsync();
    Task<ComponentType?> GetByIdAsync(int id);
    Task<ComponentType> AddAsync(ComponentType componentType);
    Task<ComponentType> UpdateAsync(ComponentType componentType);
    Task<bool> DeleteAsync(int id);
}

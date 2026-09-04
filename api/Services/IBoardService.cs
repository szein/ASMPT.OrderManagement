using core.Models;
public interface IBoardService
{
    Task<List<Board>> GetAllAsync();
    Task<List<OrderBoardComponent>> GetComponentsAsync(int boardId);
    Task<Board?> GetByIdAsync(int id);
    Task<Board> CreateAsync(string name, string description, double length, double width);
    Task<Board?> UpdateAsync(int id, string name, string description, double length, double width);
    Task<bool> DeleteAsync(int id);
}

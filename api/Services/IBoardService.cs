using core.Models;
public interface IBoardService
{
    Task<List<Board>> GetAllAsync();
    Task<Board?> GetByIdAsync(int id);
    Task<Board> CreateAsync(string name, string description, double length, double width);
    Task<Board?> UpdateAsync(int id, string name, string description, double length, double width);
    Task<Board?> UpdateComponentAssignmentsAsync(int boardId, IEnumerable<BoardComponentAssignment> componentAssignments);
    Task<bool> DeleteAsync(int id);
}

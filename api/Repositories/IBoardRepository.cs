public interface IBoardRepository
{
    Task<List<Board>> GetAllAsync();
    Task<Board?> GetByIdAsync(int id);
    Task<Board> AddAsync(Board board);
    Task<Board> UpdateAsync(Board board);
    Task<bool> DeleteAsync(int id);
}

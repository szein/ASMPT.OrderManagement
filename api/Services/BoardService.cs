using core.Models;
public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<BoardService> _logger;

    public BoardService(IBoardRepository boardRepository, ILogger<BoardService> logger)
    {
        _boardRepository = boardRepository;
        _logger = logger;
    }

    public async Task<List<Board>> GetAllAsync()
    {
        _logger.LogInformation("Service request received to fetch all boards.");
        return await _boardRepository.GetAllAsync();
    }

    public async Task<Board?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Service request received to fetch board {BoardId}.", id);
        return await _boardRepository.GetByIdAsync(id);
    }

    public async Task<Board> CreateAsync(string name, string description, double length, double width)
    {
        _logger.LogInformation("Creating a new board with name {BoardName}.", name);

        ValidateBoard(name, description, length, width);

        var board = new Board
        {
            Name = name.Trim(),
            Description = description.Trim(),
            Length = length,
            Width = width
        };

        var createdBoard = await _boardRepository.AddAsync(board);
        _logger.LogInformation("Board creation completed for board {BoardId}.", createdBoard.Id);
        return createdBoard;
    }

    public async Task<Board?> UpdateAsync(int id, string name, string description, double length, double width)
    {
        _logger.LogInformation("Updating board {BoardId}.", id);

        var existingBoard = await _boardRepository.GetByIdAsync(id);
        if (existingBoard is null)
        {
            _logger.LogWarning("Update requested for missing board {BoardId}.", id);
            return null;
        }

        ValidateBoard(name, description, length, width);

        existingBoard.Name = name.Trim();
        existingBoard.Description = description.Trim();
        existingBoard.Length = length;
        existingBoard.Width = width;

        var updatedBoard = await _boardRepository.UpdateAsync(existingBoard);
        _logger.LogInformation("Board {BoardId} was successfully updated.", updatedBoard.Id);
        return updatedBoard;
    }

    public async Task<Board?> UpdateComponentAssignmentsAsync(int boardId, IEnumerable<BoardComponentAssignment> componentAssignments)
    {
        _logger.LogInformation("Updating component assignments for board {BoardId}.", boardId);

        if (componentAssignments is null)
        {
            throw new ArgumentException("Component assignments are required.", nameof(componentAssignments));
        }

        var existingBoard = await _boardRepository.GetByIdAsync(boardId);
        if (existingBoard is null)
        {
            _logger.LogWarning("Component assignment update requested for missing board {BoardId}.", boardId);
            return null;
        }

        var normalizedAssignments = componentAssignments.ToList();
        if (normalizedAssignments.Any(a => a.ComponentId <= 0 || a.Quantity <= 0))
        {
            throw new ArgumentException("Each component assignment must include a valid component ID and quantity greater than zero.", nameof(componentAssignments));
        }

        return await _boardRepository.UpdateComponentAssignmentsAsync(boardId, normalizedAssignments);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Delete request received for board {BoardId}.", id);
        var deleted = await _boardRepository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete operation failed because board {BoardId} was not found.", id);
        }

        return deleted;
    }

    private static void ValidateBoard(string name, string description, double length, double width)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Board name is required.", nameof(name));
        }

        if (description is null)
        {
            throw new ArgumentException("Board description is required.", nameof(description));
        }

        if (length <= 0)
        {
            throw new ArgumentException("Board length must be greater than zero.", nameof(length));
        }

        if (width <= 0)
        {
            throw new ArgumentException("Board width must be greater than zero.", nameof(width));
        }
    }
}

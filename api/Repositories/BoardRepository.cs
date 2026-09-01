using Microsoft.EntityFrameworkCore;

public class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<BoardRepository> _logger;

    public BoardRepository(AppDbContext context, ILogger<BoardRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Board>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all boards from the database.");

        return await _context.Boards
            .Include(b => b.BoardComponents)
            .ThenInclude(bc => bc.Component)
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync();
    }

    public async Task<Board?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching board with Id {BoardId}.", id);

        return await _context.Boards
            .Include(b => b.BoardComponents)
            .ThenInclude(bc => bc.Component)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Board> AddAsync(Board board)
    {
        _logger.LogInformation("Adding new board named {BoardName}.", board.Name);

        await _context.Boards.AddAsync(board);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Board {BoardId} was created successfully.", board.Id);
        return board;
    }

    public async Task<Board> UpdateAsync(Board board)
    {
        _logger.LogInformation("Updating board {BoardId}.", board.Id);

        _context.Boards.Update(board);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Board {BoardId} was updated successfully.", board.Id);
        return board;
    }

    public async Task<Board?> UpdateComponentAssignmentsAsync(int boardId, IEnumerable<BoardComponentAssignment> componentAssignments)
    {
        _logger.LogInformation("Updating component assignments for board {BoardId}.", boardId);

        if (componentAssignments is null)
        {
            throw new ArgumentException("Component assignments are required.", nameof(componentAssignments));
        }

        var requestedAssignments = componentAssignments
            .Where(a => a is not null)
            .ToList();

        if (requestedAssignments.Any(a => a.ComponentId <= 0 || a.Quantity <= 0))
        {
            throw new ArgumentException("Each component assignment must include a valid component ID and a quantity greater than zero.", nameof(componentAssignments));
        }

        var duplicateComponentIds = requestedAssignments
            .GroupBy(a => a.ComponentId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateComponentIds.Count > 0)
        {
            throw new ArgumentException("Duplicate component IDs were supplied for the same board update.", nameof(componentAssignments));
        }

        var boardExists = await _context.Boards.AnyAsync(b => b.Id == boardId);
        if (!boardExists)
        {
            _logger.LogWarning("Component assignment update requested for missing board {BoardId}.", boardId);
            return null;
        }

        var requestedComponentIds = requestedAssignments.Select(a => a.ComponentId).ToHashSet();
        var validComponentIds = await _context.Components
            .Where(c => requestedComponentIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (validComponentIds.Count != requestedComponentIds.Count)
        {
            throw new ArgumentException("One or more component IDs do not exist.", nameof(componentAssignments));
        }

        var existingAssignments = await _context.BoardComponents
            .Where(bc => bc.BoardId == boardId)
            .ToListAsync();

        var currentAssignmentsByComponentId = existingAssignments
            .ToDictionary(bc => bc.ComponentId, bc => bc);

        var targetAssignmentsByComponentId = requestedAssignments
            .ToDictionary(a => a.ComponentId, a => a);

        var assignmentsToRemove = existingAssignments
            .Where(bc => !targetAssignmentsByComponentId.ContainsKey(bc.ComponentId))
            .ToList();

        if (assignmentsToRemove.Count > 0)
        {
            _context.BoardComponents.RemoveRange(assignmentsToRemove);
        }

        foreach (var assignment in requestedAssignments)
        {
            if (currentAssignmentsByComponentId.TryGetValue(assignment.ComponentId, out var existingAssignment))
            {
                existingAssignment.Quantity = assignment.Quantity;
            }
            else
            {
                _context.BoardComponents.Add(new BoardComponent
                {
                    BoardId = boardId,
                    ComponentId = assignment.ComponentId,
                    Quantity = assignment.Quantity
                });
            }
        }

        await _context.SaveChangesAsync();

        return await _context.Boards
            .Include(b => b.BoardComponents)
            .ThenInclude(bc => bc.Component)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete board {BoardId}.", id);

        var board = await _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
        if (board is null)
        {
            _logger.LogWarning("Delete requested for board {BoardId}, but it was not found.", id);
            return false;
        }

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Board {BoardId} was deleted successfully.", id);
        return true;
    }
}

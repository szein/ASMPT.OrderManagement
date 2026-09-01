using Microsoft.AspNetCore.Mvc;
using core.Models;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(IBoardService boardService, ILogger<BoardsController> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Board>>> GetAll()
    {
        _logger.LogInformation("GET /api/boards requested.");
        var boards = await _boardService.GetAllAsync();
        _logger.LogInformation("Returning {BoardCount} boards.", boards?.Count);
        return Ok(boards);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Board>> GetById(int id)
    {
        _logger.LogInformation("GET /api/boards/{BoardId} requested.", id);
        var board = await _boardService.GetByIdAsync(id);
        if (board is null)
        {
            _logger.LogWarning("Board {BoardId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Board {BoardId} returned successfully.", id);
        return Ok(board);
    }

    [HttpPost]
    public async Task<ActionResult<Board>> Create([FromBody] CreateBoardRequest request)
    {
        _logger.LogInformation("POST /api/boards requested for board {BoardName}.", request?.Name ?? "unknown");

        if (request is null)
        {
            _logger.LogWarning("Create board failed because the request body was null.");
            return BadRequest();
        }

        try
        {
            var board = await _boardService.CreateAsync(request.Name, request.Description, request.Length, request.Width);
            _logger.LogInformation("Board {BoardId} created successfully.", board.Id);
            return CreatedAtAction(nameof(GetById), new { id = board.Id }, board);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating a board.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Board>> Update(int id, [FromBody] UpdateBoardRequest request)
    {
        _logger.LogInformation("PUT /api/boards/{BoardId} requested.", id);

        if (request is null)
        {
            _logger.LogWarning("Update board failed because the request body was null for board {BoardId}.", id);
            return BadRequest();
        }

        try
        {
            var board = await _boardService.UpdateAsync(id, request.Name, request.Description, request.Length, request.Width);
            if (board is null)
            {
                _logger.LogWarning("Update failed because board {BoardId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Board {BoardId} updated successfully.", id);
            return Ok(board);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating board {BoardId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}/components")]
    public async Task<ActionResult<Board>> UpdateComponents(int id, [FromBody] UpdateBoardComponentsRequest request)
    {
        _logger.LogInformation("PUT /api/boards/{BoardId}/components requested.", id);

        if (request is null)
        {
            _logger.LogWarning("Update board components failed because the request body was null for board {BoardId}.", id);
            return BadRequest();
        }

        try
        {
            var board = await _boardService.UpdateComponentAssignmentsAsync(id, request.Components);
            if (board is null)
            {
                _logger.LogWarning("Board component update failed because board {BoardId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Board components for board {BoardId} updated successfully.", id);
            return Ok(board);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating board components for board {BoardId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/boards/{BoardId} requested.", id);
        var deleted = await _boardService.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete failed because board {BoardId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Board {BoardId} deleted successfully.", id);
        return NoContent();
    }
}

public record CreateBoardRequest(string Name, string Description, double Length, double Width);
public record UpdateBoardRequest(string Name, string Description, double Length, double Width);
public record UpdateBoardComponentsRequest(List<BoardComponentAssignment> Components);

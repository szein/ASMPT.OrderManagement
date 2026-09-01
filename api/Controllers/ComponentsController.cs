using Microsoft.AspNetCore.Mvc;
using core.Models;

[ApiController]
[Route("api/[controller]")]
public class ComponentsController : ControllerBase
{
    private readonly IComponentService _componentService;
    private readonly ILogger<ComponentsController> _logger;

    public ComponentsController(IComponentService componentService, ILogger<ComponentsController> logger)
    {
        _componentService = componentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Component>>> GetAll()
    {
        _logger.LogInformation("GET /api/components requested.");
        var components = await _componentService.GetAllAsync();
        _logger.LogInformation("Returning {ComponentCount} components.", components?.Count);
        return Ok(components);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Component>> GetById(int id)
    {
        _logger.LogInformation("GET /api/components/{ComponentId} requested.", id);
        var component = await _componentService.GetByIdAsync(id);
        if (component is null)
        {
            _logger.LogWarning("Component {ComponentId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Component {ComponentId} returned successfully.", id);
        return Ok(component);
    }

    [HttpPost]
    public async Task<ActionResult<Component>> Create([FromBody] CreateComponentRequest request)
    {
        _logger.LogInformation("POST /api/components requested for component type {ComponentTypeId}.", request?.ComponentTypeId ?? 0);

        if (request is null)
        {
            _logger.LogWarning("Create component failed because the request body was null.");
            return BadRequest();
        }

        try
        {
            var component = await _componentService.CreateAsync(request.ComponentTypeId, request.Quantity);
            _logger.LogInformation("Component {ComponentId} created successfully.", component.Id);
            return CreatedAtAction(nameof(GetById), new { id = component.Id }, component);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating a component.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Component>> Update(int id, [FromBody] UpdateComponentRequest request)
    {
        _logger.LogInformation("PUT /api/components/{ComponentId} requested.", id);

        if (request is null)
        {
            _logger.LogWarning("Update component failed because the request body was null for component {ComponentId}.", id);
            return BadRequest();
        }

        try
        {
            var component = await _componentService.UpdateAsync(id, request.ComponentTypeId, request.Quantity);
            if (component is null)
            {
                _logger.LogWarning("Update failed because component {ComponentId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Component {ComponentId} updated successfully.", id);
            return Ok(component);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating component {ComponentId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/components/{ComponentId} requested.", id);
        var deleted = await _componentService.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete failed because component {ComponentId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Component {ComponentId} deleted successfully.", id);
        return NoContent();
    }
}

public record CreateComponentRequest(int ComponentTypeId, int Quantity);
public record UpdateComponentRequest(int ComponentTypeId, int Quantity);

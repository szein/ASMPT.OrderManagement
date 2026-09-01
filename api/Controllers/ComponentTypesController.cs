using Microsoft.AspNetCore.Mvc;
using core.Models;

[ApiController]
[Route("api/[controller]")]
public class ComponentTypesController : ControllerBase
{
    private readonly IComponentTypeService _componentTypeService;
    private readonly ILogger<ComponentTypesController> _logger;

    public ComponentTypesController(IComponentTypeService componentTypeService, ILogger<ComponentTypesController> logger)
    {
        _componentTypeService = componentTypeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ComponentType>>> GetAll()
    {
        _logger.LogInformation("GET /api/componenttypes requested.");
        var componentTypes = await _componentTypeService.GetAllAsync();
        _logger.LogInformation("Returning {ComponentTypeCount} component types.", componentTypes?.Count);
        return Ok(componentTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComponentType>> GetById(int id)
    {
        _logger.LogInformation("GET /api/componenttypes/{ComponentTypeId} requested.", id);
        var componentType = await _componentTypeService.GetByIdAsync(id);
        if (componentType is null)
        {
            _logger.LogWarning("Component type {ComponentTypeId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Component type {ComponentTypeId} returned successfully.", id);
        return Ok(componentType);
    }

    [HttpPost]
    public async Task<ActionResult<ComponentType>> Create([FromBody] CreateComponentTypeRequest request)
    {
        _logger.LogInformation("POST /api/componenttypes requested for component type {ComponentTypeName}.", request?.Name ?? "unknown");

        if (request is null)
        {
            _logger.LogWarning("Create component type failed because the request body was null.");
            return BadRequest();
        }

        try
        {
            var componentType = await _componentTypeService.CreateAsync(request.Name, request.Description);
            _logger.LogInformation("Component type {ComponentTypeId} created successfully.", componentType.Id);
            return CreatedAtAction(nameof(GetById), new { id = componentType.Id }, componentType);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating a component type.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ComponentType>> Update(int id, [FromBody] UpdateComponentTypeRequest request)
    {
        _logger.LogInformation("PUT /api/componenttypes/{ComponentTypeId} requested.", id);

        if (request is null)
        {
            _logger.LogWarning("Update component type failed because the request body was null for component type {ComponentTypeId}.", id);
            return BadRequest();
        }

        try
        {
            var componentType = await _componentTypeService.UpdateAsync(id, request.Name, request.Description);
            if (componentType is null)
            {
                _logger.LogWarning("Update failed because component type {ComponentTypeId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Component type {ComponentTypeId} updated successfully.", id);
            return Ok(componentType);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating component type {ComponentTypeId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/componenttypes/{ComponentTypeId} requested.", id);
        var deleted = await _componentTypeService.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete failed because component type {ComponentTypeId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Component type {ComponentTypeId} deleted successfully.", id);
        return NoContent();
    }
}

public record CreateComponentTypeRequest(string Name, string Description);
public record UpdateComponentTypeRequest(string Name, string Description);

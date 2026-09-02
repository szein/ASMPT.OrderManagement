using Microsoft.AspNetCore.Mvc;
using core.Models;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        _logger.LogInformation("GET /api/orders requested.");
        var orders = await _orderService.GetAllAsync();
        _logger.LogInformation("Returning {OrderCount} orders.", orders?.Count);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/orders/{OrderId} requested.", id);
        var order = await _orderService.GetByIdAsync(id);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Order {OrderId} returned successfully.", id);
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("POST /api/orders requested for order {OrderName}.", request?.Name ?? "unknown");

        if (request is null)
        {
            _logger.LogWarning("Create order failed because the request body was null.");
            return BadRequest();
        }

        try
        {
            var order = await _orderService.CreateAsync(request.Name, request.Description, request.OrderDate);
            _logger.LogInformation("Order {OrderId} created successfully.", order.Id);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating an order.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Order>> Update(Guid id, [FromBody] UpdateOrderRequest request)
    {
        _logger.LogInformation("PUT /api/orders/{OrderId} requested.", id);

        if (request is null)
        {
            _logger.LogWarning("Update order failed because the request body was null for order {OrderId}.", id);
            return BadRequest();
        }

        try
        {
            var order = await _orderService.UpdateAsync(id, request.Name, request.Description, request.OrderDate);
            if (order is null)
            {
                _logger.LogWarning("Update failed because order {OrderId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Order {OrderId} updated successfully.", id);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating order {OrderId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/boards")]
    public async Task<ActionResult<Order>> UpdateBoards(Guid id, [FromBody] UpdateOrderBoardsRequest request)
    {
        _logger.LogInformation("PUT /api/orders/{OrderId}/boards requested.", id);

        if (request is null)
        {
            _logger.LogWarning("Update board assignments failed because the request body was null for order {OrderId}.", id);
            return BadRequest();
        }

        try
        {
            var order = await _orderService.UpdateBoardAssignmentsAsync(id, request.BoardIds);
            if (order is null)
            {
                _logger.LogWarning("Board assignment update failed because order {OrderId} was not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Board assignments for order {OrderId} updated successfully.", id);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating board assignments for order {OrderId}.", id);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("DELETE /api/orders/{OrderId} requested.", id);
        var deleted = await _orderService.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete failed because order {OrderId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Order {OrderId} deleted successfully.", id);
        return NoContent();
    }
}

public record CreateOrderRequest(string Name, string Description, DateTime OrderDate);
public record UpdateOrderRequest(string Name, string Description, DateTime OrderDate);
public record UpdateOrderBoardsRequest(List<int> BoardIds);

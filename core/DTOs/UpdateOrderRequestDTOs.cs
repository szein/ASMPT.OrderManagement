using System.ComponentModel.DataAnnotations;

public record OrderComponentAssignment(int ComponentId, int Quantity);
public record UpdateOrderRequest(
	[Required(ErrorMessage = "Order name is required.")] string Name,
	[Required(ErrorMessage = "Order description is required.")] string Description,
	[Range(typeof(DateTime), "1900-01-01", "9999-12-31", ErrorMessage = "Order date is required.")] DateTime OrderDate);
public record UpdateOrderBoardsRequest(List<int> BoardIds);
public record UpdateOrderComponentsRequest(List<OrderComponentAssignment> Components);
using core.Models;

public record OrderExportResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime OrderDate,
    OrderStatus Status,
    List<OrderExportBoard> Boards);

public record OrderExportBoard(
    int Id,
    string Name,
    string Description,
    double Length,
    double Width,
    List<OrderExportComponent> Components);

public record OrderExportComponent(
    int Id,
    string ComponentTypeName,
    int Quantity,
    ComponentStatus Status);
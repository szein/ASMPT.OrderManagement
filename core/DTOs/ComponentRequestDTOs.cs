
using System.ComponentModel.DataAnnotations;

public record CreateComponentRequest(
    [Required(ErrorMessage = "Component type name is required.")] string Name,
    [Required(ErrorMessage = "Component type description is required.")] string Description,
    [Range(1, int.MaxValue, ErrorMessage = "Component quantity must be greater than zero.")] int Quantity);
public record UpdateComponentRequest(
    [Required(ErrorMessage = "Component type name is required.")] string Name,
    [Required(ErrorMessage = "Component type description is required.")] string Description,
    [Range(1, int.MaxValue, ErrorMessage = "Component quantity must be greater than zero.")] int Quantity);
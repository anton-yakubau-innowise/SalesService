using System.ComponentModel.DataAnnotations;

namespace SalesService.Application.Dtos;

public record CreateOrderRequest(
    [Required] Guid CustomerId,
    [Required] Guid VehicleId
);


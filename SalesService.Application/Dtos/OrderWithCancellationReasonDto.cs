using SalesService.Domain.Enums;

namespace SalesService.Application.Dtos;

public record OrderWithCancellationReasonDto(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    OrderStatus Status,
    decimal TotalPriceAmount,
    string TotalPriceCurrency,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null,
    DateTime? PaidAt = null,
    DateTime? ConfirmedAt = null,
    DateTime? CancelledAt = null,
    string? CancellationReason = null
);

using System.ComponentModel.DataAnnotations;

namespace SalesService.Application.Dtos;

public record CancelOrderRequest(
    [Required]string CancellationReason
);
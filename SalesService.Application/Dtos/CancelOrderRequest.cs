using System.ComponentModel.DataAnnotations;

namespace SalesService.Application.Dtos;

public record CancelOrderRequest(
    [Required] [StringLength(5000)] string CancellationReason
);
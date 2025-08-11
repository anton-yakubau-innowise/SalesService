namespace SalesService.Application.Dtos;

public record VehicleDetailsDto(
    Guid Id,
    string Model,
    decimal Price,
    string Currency
);

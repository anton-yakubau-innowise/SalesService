namespace SalesService.Application.Dtos;

public record UserContactInfoDto(
    Guid Id,
    string Email,
    string? PhoneNumber = null
);

using System.ComponentModel.DataAnnotations;

namespace SalesService.Infrastructure.Options;

public class RabbitMqOptions
{
    public const string SectionName = "MassTransit";

    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

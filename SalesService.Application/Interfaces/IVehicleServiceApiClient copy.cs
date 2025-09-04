using SalesService.Application.Dtos;

namespace SalesService.Application.Interfaces;

public interface IUserServiceApiClient
{
    Task<UserContactInfoDto?> GetUserContactInfoAsync(Guid userId, CancellationToken cancellationToken);
}
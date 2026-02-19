using Grpc.Core;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using UserService.GRPC;

namespace SalesService.Infrastructure.ApiClients;

public class UserServiceApiClient(UserApi.UserApiClient grpcClient) : IUserServiceApiClient
{
    public async Task<UserContactInfoDto?> GetUserContactInfoAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetUserContactInfoRequest { UserId = userId.ToString() };
            var response = await grpcClient.GetUserContactInfoAsync(request, cancellationToken: cancellationToken);

            return new UserContactInfoDto(
                Guid.Parse(response.Id),
                response.Email,
                response.PhoneNumber
            );
        }

        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
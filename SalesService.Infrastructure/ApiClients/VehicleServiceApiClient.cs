using Grpc.Core;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using VehicleService.GRPC;

namespace SalesService.Infrastructure.ApiClients;

public class VehicleServiceApiClient(VehicleApi.VehicleApiClient grpcClient) : IVehicleServiceApiClient
{
    public async Task<VehicleDetailsDto?> GetVehicleDetailsAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetVehicleDetailsRequest { VehicleId = vehicleId.ToString() };
            var response = await grpcClient.GetVehicleDetailsAsync(request, cancellationToken: cancellationToken);

            if (!decimal.TryParse(response.Price, System.Globalization.CultureInfo.InvariantCulture, out var price))
            {
                throw new RpcException(new Status(StatusCode.Internal, "Invalid price format received from VehicleService."));
            }

            return new VehicleDetailsDto(
                Guid.Parse(response.Id),
                response.Model,
                price,
                response.Currency
            );
        }
        
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
using SalesService.Application.Dtos;

namespace SalesService.Application.Interfaces;

public interface IVehicleServiceApiClient
{
    Task<VehicleDetailsDto?> GetVehicleDetailsAsync(Guid vehicleId, CancellationToken cancellationToken);
}
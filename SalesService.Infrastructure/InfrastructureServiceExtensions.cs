using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Interfaces;
using SalesService.Domain.Repositories;
using SalesService.Infrastructure.ApiClients;
using SalesService.Infrastructure.Persistence;
using SalesService.Infrastructure.Persistence.Repositories;
using VehicleService.GRPC;

namespace SalesService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddGrpcClient<VehicleApi.VehicleApiClient>(o =>
        {
            o.Address = new Uri(configuration["ServiceUrls:VehicleService"]);
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleServiceApiClient, VehicleServiceApiClient>();

        return services;
    }
}
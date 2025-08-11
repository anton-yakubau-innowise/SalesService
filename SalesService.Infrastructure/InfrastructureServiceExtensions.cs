using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Exceptions;
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
            var serviceUrl = configuration["ServiceUrls:VehicleService"];

            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new ConfigurationException("Address for VehicleService not found in configuration (ServiceUrls:VehicleService).");
            }

            o.Address = new Uri(serviceUrl);
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleServiceApiClient, VehicleServiceApiClient>();

        return services;
    }
}
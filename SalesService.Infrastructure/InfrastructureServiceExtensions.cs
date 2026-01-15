using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SalesService.Application.Interfaces;
using SalesService.Domain.Repositories;
using SalesService.Infrastructure.ApiClients;
using SalesService.Infrastructure.Options;
using SalesService.Infrastructure.Persistence;
using SalesService.Infrastructure.Persistence.Repositories;
using UserService.GRPC;
using VehicleService.GRPC;

namespace SalesService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SalesDbContext>(options =>
            options.UseCosmos(
                connectionString: configuration.GetConnectionString("DefaultConnection") ?? throw new ConfigurationException("Cosmos DB connection string not found in configuration."),
                databaseName: configuration["CosmosDb:DatabaseName"] ?? throw new ConfigurationException("Cosmos DB database name not found in configuration (CosmosDb:DatabaseName).")));

        services.AddGrpcClient<VehicleApi.VehicleApiClient>(o =>
        {
            var serviceUrl = configuration["ServiceUrls:VehicleService"];

            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new Application.Exceptions.ConfigurationException("Address for VehicleService not found in configuration (ServiceUrls:VehicleService).");
            }

            o.Address = new Uri(serviceUrl);
        });
        
        services.AddGrpcClient<UserApi.UserApiClient>(o =>
        {
            var serviceUrl = configuration["ServiceUrls:UserService"];

            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new Application.Exceptions.ConfigurationException("Address for UserService not found in configuration (ServiceUrls:UserService).");
            }

            o.Address = new Uri(serviceUrl);
        });

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(options.Host, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
            });
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleServiceApiClient, VehicleServiceApiClient>();
        services.AddScoped<IUserServiceApiClient, UserServiceApiClient>();

        return services;
    }
}
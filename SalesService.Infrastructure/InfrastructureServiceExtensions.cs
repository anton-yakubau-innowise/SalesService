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
                throw new Application.Exceptions.ConfigurationException("Address for VehicleService not found in configuration (ServiceUrls:VehicleService).");
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

                cfg.Host(options.Host, "/", h =>
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
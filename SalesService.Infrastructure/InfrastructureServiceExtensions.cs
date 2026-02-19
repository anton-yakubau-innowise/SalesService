using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SalesService.Application.Interfaces;
using SalesService.Domain.Repositories;
using SalesService.Infrastructure.ApiClients;
using SalesService.Infrastructure.Options;
using SalesService.Infrastructure.Persistence;
using SalesService.Infrastructure.Persistence.Repositories;
using UserService.GRPC;
using VehicleService.GRPC;
using System.Net;

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
        })
        .ConfigureHttpClient(client =>
        {
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            ConnectTimeout = TimeSpan.FromSeconds(30)
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
        
        services.AddGrpcClient<UserApi.UserApiClient>(o =>
        {
            var serviceUrl = configuration["ServiceUrls:UserService"];

            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new Application.Exceptions.ConfigurationException("Address for UserService not found in configuration (ServiceUrls:UserService).");
            }

            o.Address = new Uri(serviceUrl);
        })
        .ConfigureHttpClient(client =>
        {
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            ConnectTimeout = TimeSpan.FromSeconds(30)
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());
        
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddOptions<AzureServiceBusOptions>()
            .Bind(configuration.GetSection(AzureServiceBusOptions.SectionName));
            
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            var asbOptions = configuration.GetSection(AzureServiceBusOptions.SectionName).Get<AzureServiceBusOptions>();
            var connectionString = asbOptions?.ConnectionString;

            if (!string.IsNullOrEmpty(connectionString))
            {
                busConfigurator.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
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
        }
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleServiceApiClient, VehicleServiceApiClient>();
        services.AddScoped<IUserServiceApiClient, UserServiceApiClient>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3, 
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
            );
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5, 
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
    }
}
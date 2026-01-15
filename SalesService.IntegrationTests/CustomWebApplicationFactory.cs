using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SalesService.Application.Interfaces;
using SalesService.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SalesService.IntegrationTests;


public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    public Mock<IVehicleServiceApiClient> VehicleServiceMock { get; }
    public Mock<IUserServiceApiClient> UserServiceMock { get; }
    public Mock<IPublishEndpoint> PublishEndpointMock { get; }
    private readonly PostgreSqlContainer dbContainer;

    public CustomWebApplicationFactory()
    {
        VehicleServiceMock = new Mock<IVehicleServiceApiClient>();
        UserServiceMock = new Mock<IUserServiceApiClient>();
        PublishEndpointMock = new Mock<IPublishEndpoint>();
        dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("test_sales_db")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SalesDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<SalesDbContext>(options =>
            {
                options.UseNpgsql(dbContainer.GetConnectionString());
            });

            var vehicleServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IVehicleServiceApiClient));
            if (vehicleServiceDescriptor != null)
            {
                services.Remove(vehicleServiceDescriptor);
            }

            var userServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserServiceApiClient));
            if (userServiceDescriptor != null)
            {
                services.Remove(userServiceDescriptor);
            }

            var publishEndpointDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPublishEndpoint));
            if (publishEndpointDescriptor != null)
            {
                services.Remove(publishEndpointDescriptor);
            }

            services.AddScoped<IVehicleServiceApiClient>(_ => VehicleServiceMock.Object);
            services.AddScoped<IUserServiceApiClient>(_ => UserServiceMock.Object);
            services.AddScoped<IPublishEndpoint>(_ => PublishEndpointMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        await dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await dbContainer.StopAsync();
    }
}

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
    private readonly PostgreSqlContainer dbContainer;

    public CustomWebApplicationFactory()
    {
        VehicleServiceMock = new Mock<IVehicleServiceApiClient>();
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
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseNpgsql(dbContainer.GetConnectionString());
            });

            var apiClientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IVehicleServiceApiClient));
            if (apiClientDescriptor != null)
            {
                services.Remove(apiClientDescriptor);
            }
            services.AddScoped<IVehicleServiceApiClient>(_ => VehicleServiceMock.Object);
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

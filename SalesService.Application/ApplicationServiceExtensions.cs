using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Interfaces;
using SalesService.Application.Services;

namespace SalesService.Application
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddScoped<IOrderApplicationService, OrderApplicationService>();

            return services;
        }
    }
}
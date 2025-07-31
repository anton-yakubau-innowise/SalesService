using Microsoft.EntityFrameworkCore;
using SalesService.API;
using SalesService.API.Middleware;
using SalesService.Application;
using SalesService.Infrastructure;
using SalesService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseCustomExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
using SalesService.Application.Interfaces;
using SalesService.Domain.Repositories;
using SalesService.Infrastructure.Persistence.Repositories;

namespace SalesService.Infrastructure.Persistence
{
public class UnitOfWork(OrderDbContext dbContext) : IUnitOfWork
{
    public IOrderRepository Orders { get; } = new OrderRepository(dbContext);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return dbContext.SaveChangesAsync(cancellationToken);
        }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
}
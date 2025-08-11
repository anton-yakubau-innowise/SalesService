using SalesService.Domain.Repositories;

namespace SalesService.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
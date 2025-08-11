using System.Linq.Expressions;
using SalesService.Domain.Entities;

namespace SalesService.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Order?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> ListAllAsNoTrackingAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> ListAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> ListAsNoTrackingAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default);
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        void Delete(Order order);
    }
}
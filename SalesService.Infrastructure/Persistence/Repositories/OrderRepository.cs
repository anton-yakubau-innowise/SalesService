using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SalesService.Domain.Entities;
using SalesService.Domain.Repositories;

namespace SalesService.Infrastructure.Persistence.Repositories
{
    public class OrderRepository(OrderDbContext dbContext) : IOrderRepository
    {
        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders
                                   .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<Order?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Order>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders
                                   .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> ListAllAsNoTrackingAsync(CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<Order>> ListAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders
                                   .Where(predicate)
                                   .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> ListAsNoTrackingAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await dbContext.Orders
                                   .AsNoTracking()
                                   .Where(predicate)
                                   .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await dbContext.Orders.AddAsync(order, cancellationToken);
        }

        public void Delete(Order order)
        {
            dbContext.Orders.Remove(order);
        }
    }
}
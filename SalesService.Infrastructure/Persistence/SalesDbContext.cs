using Microsoft.EntityFrameworkCore;
using System.Reflection;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.Persistence
{
    public class SalesDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public SalesDbContext(DbContextOptions<SalesDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
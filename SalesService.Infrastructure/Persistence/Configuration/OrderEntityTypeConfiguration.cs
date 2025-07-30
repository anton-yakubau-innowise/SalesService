using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.Persistence.Configuration
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerId)
                .IsRequired();

            builder.Property(o => o.VehicleId)
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.OwnsOne(v => v.TotalPrice, priceBuilder =>
            {

                priceBuilder.Property(p => p.Amount)
                    .HasColumnName("TotalPrice_Amount")
                    .HasColumnType("numeric(19,4)")
                    .IsRequired();

                priceBuilder.Property(p => p.Currency)
                    .HasColumnName("TotalPrice_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(o => o.CancellationReason)
                .HasMaxLength(5000);
        }
    }
}
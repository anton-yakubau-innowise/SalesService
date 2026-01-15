using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.Persistence.Configuration
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToContainer("Orders");
            builder.HasPartitionKey(o => o.CustomerId);
            builder.UseETagConcurrency();
            builder.HasNoDiscriminator();

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ToJsonProperty("id");

            builder.Property(o => o.CustomerId).ToJsonProperty("customerId");
            builder.Property(o => o.VehicleId).ToJsonProperty("vehicleId");
            builder.Property(o => o.CreatedAt).ToJsonProperty("createdAt");
            builder.Property(o => o.UpdatedAt).ToJsonProperty("updatedAt");
            builder.Property(o => o.PaidAt).ToJsonProperty("paidAt");
            builder.Property(o => o.ConfirmedAt).ToJsonProperty("confirmedAt");
            builder.Property(o => o.CancelledAt).ToJsonProperty("cancelledAt");
            builder.Property(o => o.CancellationReason).ToJsonProperty("cancellationReason");
            
            builder.Property(o => o.Status)
                .HasConversion<string>()
                .ToJsonProperty("status");

            builder.OwnsOne(v => v.TotalPrice, priceBuilder =>
            {
                priceBuilder.Property(p => p.Amount)
                    .IsRequired()
                    .ToJsonProperty("amount");

                priceBuilder.Property(p => p.Currency)
                    .IsRequired()
                    .ToJsonProperty("currency");
            });
        }
    }
}

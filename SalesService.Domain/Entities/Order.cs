using SalesService.Domain.Common;
using SalesService.Domain.Enums;
using SalesService.Domain.ValueObjects;

namespace SalesService.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid VehicleId { get; private set; }
        public Money TotalPrice { get; private set; } = null!;
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        private Order() { }
 
        private Order(Guid customerId, Guid vehicleId, Money totalPrice, OrderStatus status)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            VehicleId = vehicleId;
            TotalPrice = totalPrice;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }
        
        public static Order Create(Guid customerId, Guid vehicleId, Money totalPrice)
        {
            Guard.AgainstNull(totalPrice, nameof(totalPrice));
            Guard.AgainstEmptyGuid(customerId, nameof(customerId));
            Guard.AgainstEmptyGuid(vehicleId, nameof(vehicleId));

            var order = new Order(customerId, vehicleId, totalPrice, OrderStatus.Pending);

            return order;
        }
        
        public void CompleteProcessing()
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException($"Unable to complete processing for order in status '{Status}'.");
            }

            Status = OrderStatus.AwaitingPayment;
            SetUpdated();
        }

        public void ConfirmPayment()
        {
            if (Status != OrderStatus.AwaitingPayment)
            {
                throw new InvalidOperationException($"Unable to confirm payment for order in status '{Status}'.");
            }

            Status = OrderStatus.Paid;
            PaidAt = DateTime.UtcNow;
            SetUpdated();
        }
        
        public void ConfirmOrder()
        {
            if (Status != OrderStatus.Paid)
            {
                throw new InvalidOperationException($"Unable to confirm order in status '{Status}'.");
            }

            Status = OrderStatus.Confirmed;
            ConfirmedAt = DateTime.UtcNow;
            SetUpdated();
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Cancelled || Status == OrderStatus.Confirmed)
            {
                throw new InvalidOperationException($"Unable to cancel order in status '{Status}'.");
            }

            Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
            SetUpdated();
        }

        private void SetUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
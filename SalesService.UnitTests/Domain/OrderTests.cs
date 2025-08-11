using FluentAssertions;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Domain.ValueObjects;

namespace SalesService.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithValidParameters_InitializesOrderCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var price = new Money(50000, "USD");

        // Act
        var order = Order.Create(customerId, vehicleId, price);

        order.Should().NotBeNull();
        order.CustomerId.Should().Be(customerId);
        order.VehicleId.Should().Be(vehicleId);
        order.TotalPrice.Should().Be(price);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullPrice_ThrowsArgumentNullException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        Money? price = null;

        // Act
        Action act = () => Order.Create(customerId, vehicleId, price!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CompleteProcessing_WhenOrderIsPending_SetsStatusToProcessing()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));

        // Act
        order.CompleteProcessing();

        // Assert
        order.Status.Should().Be(OrderStatus.AwaitingPayment);
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CompleteProcessing_WhenOrderIsNotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));
        order.Cancel("Test reason");

        // Act
        Action act = () => order.CompleteProcessing();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConfirmPayment_WhenOrderIsAwaitingPayment_SetsStatusToPaid()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));
        order.CompleteProcessing();

        // Act
        order.ConfirmPayment();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAt.Should().NotBeNull();
        order.PaidAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ConfirmPayment_WhenStatusIsNotAwaitingPayment_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));
        order.Cancel("Test reason");

        // Act
        Action act = () => order.ConfirmPayment();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConfirmPayment_WhenStatusIsPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));

        // Act
        Action act = () => order.ConfirmPayment();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenOrderCanBeCancelled_SetsStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));
        order.CompleteProcessing();

        // Act
        order.Cancel("Test reason");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        order.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cancel_WhenOrderIsConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(50000, "USD"));
        order.CompleteProcessing();
        order.ConfirmPayment();
        order.ConfirmOrder();

        // Act
        Action act = () => order.Cancel("Test reason");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}